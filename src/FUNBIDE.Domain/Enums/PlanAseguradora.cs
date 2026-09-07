namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Plan de tarifario de una aseguradora (ver <see cref="Entities.TarifarioProcedimiento"/>):
/// mismo procedimiento, montos distintos de cobertura/copago según el plan del paciente.
/// SENASA es la única aseguradora con subdivisión real (<see cref="Subsidiado"/>,
/// <see cref="Contributivo"/>, <see cref="Pensionado"/>, <see cref="Larimar"/>); las demás
/// (Renacer, Aps) usan <see cref="Estandar"/> como su único "plan", ya que no tienen esa
/// subdivisión. Se guarda como string en la base de datos (ver
/// <c>TarifarioProcedimientoConfiguration</c>), así que agregar un valor nuevo acá no
/// requiere migración — solo entra en juego cuando se importa o edita tarifario de ese plan.
/// "PlanEspecial" existió acá hasta que se discontinuó (2026-08); "Larimar" se discontinuó
/// y se restauró el mismo mes al confirmarse que sí tiene tarifario real y vigente. Si
/// quedan filas viejas en la base con "PlanEspecial", hay que migrarlas o borrarlas aparte
/// — el enum ya no lo reconoce.
/// </summary>
public enum PlanAseguradora
{
    Subsidiado,
    Contributivo,
    Pensionado,
    Larimar,
    Estandar
}
