using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Fila del tarifario negociado de una aseguradora (hoy solo SENASA): cuánto cubre el
/// seguro y cuánto paga el paciente por un procedimiento puntual, según el plan
/// (<see cref="PlanSenasa"/>). A diferencia de <see cref="SeguroMedico.PorcentajeCobertura"/>
/// (un % plano sobre cualquier monto), acá los montos son fijos y vienen del tarifario
/// oficial — <see cref="RegistrarCobroUseCase"/> nunca los deriva de un porcentaje. Se
/// carga por import de Excel (uno por plan, ver <c>ImportarTarifarioUseCase</c>) y se
/// desactiva en vez de borrarse para no perder la referencia de cobros ya registrados.
/// </summary>
public sealed class TarifarioProcedimiento : Entity
{
    public Guid SeguroMedicoId { get; private set; }
    public PlanSenasa Plan { get; private set; }
    public string Procedimiento { get; private set; } = string.Empty;
    public decimal MontoSeguro { get; private set; }
    public decimal MontoPaciente { get; private set; }
    public decimal MontoTotal { get; private set; }
    public bool Activo { get; private set; } = true;

    private TarifarioProcedimiento() { }

    public TarifarioProcedimiento(
        Guid seguroMedicoId, PlanSenasa plan, string procedimiento,
        decimal montoSeguro, decimal montoPaciente, decimal montoTotal)
    {
        ValidarDatos(procedimiento, montoSeguro, montoPaciente, montoTotal);

        SeguroMedicoId = seguroMedicoId;
        Plan = plan;
        Procedimiento = procedimiento.Trim();
        MontoSeguro = montoSeguro;
        MontoPaciente = montoPaciente;
        MontoTotal = montoTotal;
    }

    public void ActualizarMontos(decimal montoSeguro, decimal montoPaciente, decimal montoTotal)
    {
        ValidarDatos(Procedimiento, montoSeguro, montoPaciente, montoTotal);

        MontoSeguro = montoSeguro;
        MontoPaciente = montoPaciente;
        MontoTotal = montoTotal;
    }

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    private static void ValidarDatos(string procedimiento, decimal montoSeguro, decimal montoPaciente, decimal montoTotal)
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
    }
}
