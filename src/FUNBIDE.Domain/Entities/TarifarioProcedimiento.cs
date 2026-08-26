using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Fila del tarifario negociado de una aseguradora: cuánto cubre el seguro y cuánto paga
/// el paciente por un procedimiento puntual, según el plan (<see cref="PlanAseguradora"/> —
/// <see cref="PlanAseguradora.Estandar"/> para aseguradoras sin subdivisión de planes, como
/// Renacer o Aps). A diferencia de <see cref="SeguroMedico.PorcentajeCobertura"/> (un %
/// plano sobre cualquier monto), acá los montos son fijos y vienen del tarifario oficial —
/// <see cref="RegistrarCobroUseCase"/> nunca los deriva de un porcentaje. Se carga por
/// import de Excel (uno por plan, ver <c>ImportarTarifarioUseCase</c>) y se desactiva en
/// vez de borrarse para no perder la referencia de cobros ya registrados.
/// </summary>
public sealed class TarifarioProcedimiento : Entity
{
    public Guid SeguroMedicoId { get; private set; }
    public PlanAseguradora Plan { get; private set; }
    public string Procedimiento { get; private set; } = string.Empty;
    public decimal MontoSeguro { get; private set; }
    public decimal MontoPaciente { get; private set; }
    public decimal MontoTotal { get; private set; }

    /// <summary>
    /// Excedente que la aseguradora paga por encima de <see cref="MontoSeguro"/> y que no
    /// se le reconoce al paciente ni entra a la caja física, sino que queda como ingreso
    /// interno de la fundación (ver <c>RegistrarCobroUseCase</c>, que lo vuelca a un
    /// <see cref="MovimientoFinanciero"/> aparte). Null cuando la aseguradora no negocia
    /// este excedente (hoy: todas salvo Renacer).
    /// </summary>
    public decimal? MontoFondo { get; private set; }

    /// <summary>
    /// Especialidad de FUNBIDE a la que corresponde este procedimiento, solo para agrupar
    /// y filtrar en la UI (selección encadenada especialidad → procedimiento → doctor) —
    /// no es una validación de negocio. Null cuando el procedimiento no tiene un
    /// equivalente claro entre las <see cref="EspecialidadMedica"/> que maneja FUNBIDE
    /// (p. ej. la mayoría del tarifario de un asegurador externo, que cubre más
    /// especialidades de las que la fundación ofrece).
    /// </summary>
    public EspecialidadMedica? Especialidad { get; private set; }

    public bool Activo { get; private set; } = true;

    private TarifarioProcedimiento() { }

    public TarifarioProcedimiento(
        Guid seguroMedicoId, PlanAseguradora plan, string procedimiento,
        decimal montoSeguro, decimal montoPaciente, decimal montoTotal,
        decimal? montoFondo = null, EspecialidadMedica? especialidad = null)
    {
        ValidarDatos(procedimiento, montoSeguro, montoPaciente, montoTotal, montoFondo);

        SeguroMedicoId = seguroMedicoId;
        Plan = plan;
        Procedimiento = procedimiento.Trim();
        MontoSeguro = montoSeguro;
        MontoPaciente = montoPaciente;
        MontoTotal = montoTotal;
        MontoFondo = montoFondo;
        Especialidad = especialidad;
    }

    public void ActualizarMontos(decimal montoSeguro, decimal montoPaciente, decimal montoTotal, decimal? montoFondo = null)
    {
        ValidarDatos(Procedimiento, montoSeguro, montoPaciente, montoTotal, montoFondo);

        MontoSeguro = montoSeguro;
        MontoPaciente = montoPaciente;
        MontoTotal = montoTotal;
        MontoFondo = montoFondo;
    }

    public void AsignarEspecialidad(EspecialidadMedica? especialidad) => Especialidad = especialidad;

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    private static void ValidarDatos(
        string procedimiento, decimal montoSeguro, decimal montoPaciente, decimal montoTotal, decimal? montoFondo)
    {
        if (string.IsNullOrWhiteSpace(procedimiento))
        {
            throw new ArgumentException("El nombre del procedimiento es obligatorio.", nameof(procedimiento));
        }

        if (montoSeguro < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoSeguro), "El monto que cubre el seguro no puede ser negativo.");
        }

        if (montoPaciente < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoPaciente), "El monto a cargo del paciente no puede ser negativo.");
        }

        if (montoTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoTotal), "El monto total debe ser mayor que cero.");
        }

        if (montoFondo is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoFondo), "El monto del fondo interno no puede ser negativo.");
        }
    }
}
