namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Los tres planes de SENASA con tarifario propio (ver <see cref="Entities.TarifarioProcedimiento"/>):
/// mismo procedimiento, montos distintos de cobertura/copago según el plan del paciente.
/// </summary>
public enum PlanSenasa
{
    Contributivo,
    Pensionado,
    Larimar
}
