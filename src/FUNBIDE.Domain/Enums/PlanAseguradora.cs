namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Plan de tarifario de una aseguradora (ver <see cref="Entities.TarifarioProcedimiento"/>):
/// mismo procedimiento, montos distintos de cobertura/copago según el plan del paciente.
/// SENASA es la única aseguradora con subdivisión real (<see cref="Subsidiado"/>,
/// <see cref="Contributivo"/>, <see cref="Pensionado"/>, <see cref="PlanEspecial"/>,
/// <see cref="Larimar"/>); las demás (Renacer, Aps) usan <see cref="Estandar"/> como su
/// único "plan", ya que no tienen esa subdivisión. Se guarda como string en la base de
/// datos (ver <c>TarifarioProcedimientoConfiguration</c>), así que agregar un valor nuevo
/// acá no requiere migración — solo entra en juego cuando se importa o edita tarifario de
/// ese plan.
/// </summary>
public enum PlanAseguradora
{
    Subsidiado,
    Contributivo,
    Pensionado,
    PlanEspecial,
    Larimar,
    Estandar
}
