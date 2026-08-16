using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Comprobante inmutable de un cobro a un paciente: append-only, igual que
/// <see cref="MovimientoFinanciero"/>. Si hay seguro médico, <see cref="MontoCobertura"/>
/// es lo que cubre la ARS (nunca entra a la caja física) y <see cref="MontoACargoPaciente"/>
/// es el resto, que el paciente paga — posiblemente dividido en varias
/// <see cref="Pagos"/> (ej. una parte con tarjeta, otra en efectivo).
/// <see cref="MontoPagado"/> (la suma de <see cref="Pagos"/>) puede quedar por debajo de
/// <see cref="MontoACargoPaciente"/> (pago parcial): la diferencia
/// (<see cref="MontoPendiente"/>) es la deuda que arrastra el paciente.
/// </summary>
public sealed class Cobro : AppendOnlyEntity
{
    public Guid PacienteId { get; private set; }
    public Guid? CitaId { get; private set; }
    public Guid TurnoCajaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Concepto { get; private set; } = string.Empty;
    public decimal MontoTotal { get; private set; }
    public Guid? SeguroMedicoId { get; private set; }
    public decimal? PorcentajeCobertura { get; private set; }
    public decimal? MontoCobertura { get; private set; }
    public Guid? TarifarioProcedimientoId { get; private set; }
    public string? CodigoAutorizacion { get; private set; }

    private readonly List<PagoRecibido> _pagos = [];
    /// <summary>Cómo entró el dinero de este cobro, desglosado por método. Puede estar vacía (nada pagado todavía, todo a deuda).</summary>
    public IReadOnlyList<PagoRecibido> Pagos => _pagos.AsReadOnly();

    /// <summary>
    /// Suma de <see cref="Pagos"/>, congelada en una columna real (no una propiedad
    /// calculada) a propósito: varias consultas de deuda en <c>CobroRepository</c> hacen
    /// aritmética con este campo dentro de un <c>Where</c>/<c>Sum</c> que EF Core traduce a
    /// SQL, y eso exige que sea una columna mapeada de verdad, no una suma en memoria
    /// sobre la colección owned de <see cref="Pagos"/>.
    /// </summary>
    public decimal MontoPagado { get; private set; }

    public decimal MontoACargoPaciente => MontoTotal - (MontoCobertura ?? 0);

    public decimal MontoPendiente => MontoACargoPaciente - MontoPagado;

    private Cobro() { }

    /// <param name="pagos">
    /// Desglose de cómo se pagó lo que queda a cargo del paciente — puede tener 0, 1 o
    /// varias líneas (ej. una parte con tarjeta y otra en efectivo), pero nunca dos
    /// líneas del mismo <see cref="MetodoPago"/> (se consolidan antes de llegar acá). La
    /// suma de todas define <see cref="MontoPagado"/>: no es un parámetro aparte.
    /// </param>
    /// <param name="porcentajeCobertura">
    /// Ignorado si <paramref name="montoCoberturaExacto"/> viene informado (caso tarifario:
    /// ver <paramref name="tarifarioProcedimientoId"/>) — ambos existen porque son dos
    /// formas de calcular <see cref="MontoCobertura"/> que no se combinan.
    /// </param>
    /// <param name="tarifarioProcedimientoId">
    /// Fila del tarifario (ver <see cref="TarifarioProcedimiento"/>) usada para este cobro,
    /// si el seguro tiene tarifario por procedimiento (hoy solo SENASA). Cuando está
    /// presente, <paramref name="montoCoberturaExacto"/> también debe venir informado.
    /// </param>
    /// <param name="montoCoberturaExacto">
    /// Monto fijo que cubre el seguro para este procedimiento puntual, tal cual el
    /// tarifario negociado — a diferencia de <paramref name="porcentajeCobertura"/>, no se
    /// deriva de un porcentaje sobre <paramref name="montoTotal"/>. Reemplaza el cálculo
    /// porcentual cuando se informa.
    /// </param>
    public Cobro(
        Guid pacienteId,
        Guid? citaId,
        Guid turnoCajaId,
        Guid usuarioId,
        string concepto,
        decimal montoTotal,
        IReadOnlyList<PagoRecibido> pagos,
        Guid? seguroMedicoId = null,
        decimal? porcentajeCobertura = null,
        string? codigoAutorizacion = null,
        Guid? tarifarioProcedimientoId = null,
        decimal? montoCoberturaExacto = null)
    {
        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new ArgumentException("El concepto del cobro es obligatorio.", nameof(concepto));
        }

        if (montoTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoTotal), "El monto total debe ser mayor que cero.");
        }

        if (pagos.Select(p => p.Metodo).Distinct().Count() != pagos.Count)
        {
            throw new ArgumentException(
                "No puede haber dos pagos con el mismo método — súmalos en una sola línea.", nameof(pagos));
        }

        if (seguroMedicoId.HasValue)
        {
            if (montoCoberturaExacto is null)
            {
                if (porcentajeCobertura is null or <= 0 or > 100)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(porcentajeCobertura), "El porcentaje de cobertura debe estar entre 0 y 100.");
                }
            }
            else if (montoCoberturaExacto < 0 || montoCoberturaExacto > montoTotal)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(montoCoberturaExacto), "El monto que cubre el seguro debe estar entre 0 y el monto total.");
            }

            if (string.IsNullOrWhiteSpace(codigoAutorizacion))
            {
                throw new ArgumentException(
                    "El código de autorización es obligatorio cuando el cobro usa seguro médico.", nameof(codigoAutorizacion));
            }
        }

        PacienteId = pacienteId;
        CitaId = citaId;
        TurnoCajaId = turnoCajaId;
        UsuarioId = usuarioId;
        Concepto = concepto.Trim();
        MontoTotal = montoTotal;
        SeguroMedicoId = seguroMedicoId;
        TarifarioProcedimientoId = seguroMedicoId.HasValue ? tarifarioProcedimientoId : null;
        PorcentajeCobertura = seguroMedicoId.HasValue && montoCoberturaExacto is null ? porcentajeCobertura : null;
        MontoCobertura = seguroMedicoId.HasValue
            ? montoCoberturaExacto ?? Math.Round(montoTotal * porcentajeCobertura!.Value / 100m, 2)
            : null;
        CodigoAutorizacion = seguroMedicoId.HasValue ? codigoAutorizacion!.Trim() : null;

        var montoPagado = pagos.Sum(p => p.Monto);
        if (montoPagado < 0 || montoPagado > MontoACargoPaciente)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pagos), $"La suma de los pagos debe estar entre 0 y {MontoACargoPaciente} (lo que queda a cargo del paciente).");
        }

        MontoPagado = montoPagado;
        _pagos.AddRange(pagos);
    }
}
