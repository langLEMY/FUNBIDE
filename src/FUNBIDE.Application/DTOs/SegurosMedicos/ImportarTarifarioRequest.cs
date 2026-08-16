using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.SegurosMedicos;

/// <summary>
/// El Excel de origen (ver PRECIOS ODO SENASA) trae una hoja por plan, así que a
/// diferencia de los demás importadores (Empleados, Pacientes, Inventario) acá el plan y
/// la aseguradora no salen del archivo: hay que informarlos aparte, uno por importación.
/// </summary>
public sealed record ImportarTarifarioRequest(Guid SeguroMedicoId, PlanSenasa Plan, Stream Contenido);
