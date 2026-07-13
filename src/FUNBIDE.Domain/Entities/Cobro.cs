using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Comprobante inmutable de un cobro a un paciente: append-only, igual que
/// <see cref="MovimientoFinanciero"/>. Si hay seguro médico, <see cref="MontoCobertura"/>
/// es lo que cubre la ARS (nunca entra a la caja física) y <see cref="MontoACargoPaciente"/>
/// es el resto, que el paciente paga por <see cref="MetodoPago"/>. <see cref="MontoPagado"/>
/// puede quedar por debajo de <see cref="MontoACargoPaciente"/> (pago parcial): la
/// diferencia (<see cref="MontoPendiente"/>) es la deuda que arrastra el paciente.
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
    public string? CodigoAutorizacion { get; private set; }
    public MetodoPago MetodoPago { get; private set; }
    public decimal MontoPagado { get; private set; }

    public decimal MontoACargoPaciente => MontoTotal - (MontoCobertura ?? 0);

    public decimal MontoPendiente => MontoACargoPaciente - MontoPagado;

    private Cobro() { }

    public Cobro(
        Guid pacienteId,
        Guid? citaId,
        Guid turnoCajaId,
        Guid usuarioId,
        string concepto,
        decimal montoTotal,
        MetodoPago metodoPago,
        decimal montoPagado,
        Guid? seguroMedicoId = null,
        decimal? porcentajeCobertura = null,
        string? codigoAutorizacion = null)
    {
        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new ArgumentException("El concepto del cobro es obligatorio.", nameof(concepto));
        }

        if (montoTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoTotal), "El monto total debe ser mayor que cero.");
        }

        if (seguroMedicoId.HasValue)
        {
            if (porcentajeCobertura is null or <= 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(porcentajeCobertura), "El porcentaje de cobertura debe estar entre 0 y 100.");
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
        MetodoPago = metodoPago;
        SeguroMedicoId = seguroMedicoId;
        PorcentajeCobertura = seguroMedicoId.HasValue ? porcentajeCobertura : null;
        MontoCobertura = seguroMedicoId.HasValue ? Math.Round(montoTotal * porcentajeCobertura!.Value / 100m, 2) : null;
        CodigoAutorizacion = seguroMedicoId.HasValue ? codigoAutorizacion!.Trim() : null;

        if (montoPagado < 0 || montoPagado > MontoACargoPaciente)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoPagado), $"El monto pagado debe estar entre 0 y {MontoACargoPaciente} (lo que queda a cargo del paciente).");
        }

        MontoPagado = montoPagado;
    }
}
