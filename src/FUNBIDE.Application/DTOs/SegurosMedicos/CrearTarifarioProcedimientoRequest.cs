using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.SegurosMedicos;

/// <summary>
/// Alta manual de un procedimiento puntual del tarifario de una aseguradora — para cuando
/// solo hace falta cargar uno o dos procedimientos (p. ej. una ARS nueva que arranca con una
/// sola consulta negociada) y no vale la pena armar un Excel para el import masivo.
/// </summary>
public sealed record CrearTarifarioProcedimientoRequest(
    Guid SeguroMedicoId,
    PlanAseguradora Plan,
    string Procedimiento,
    decimal MontoSeguro,
    decimal MontoPaciente,
    decimal MontoTotal,
    decimal? MontoFondo,
    EspecialidadMedica? Especialidad);
