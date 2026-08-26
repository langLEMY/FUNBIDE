using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.SegurosMedicos;

/// <summary>
/// Ajuste puntual de una fila ya cargada del tarifario — pensado sobre todo para dar de
/// alta o corregir <see cref="Domain.Entities.TarifarioProcedimiento.MontoFondo"/> (la
/// ganancia interna que la fundación negocia con la aseguradora) en un procedimiento
/// puntual sin tener que rehacer el import completo del Excel de ese plan.
/// </summary>
public sealed record EditarTarifarioProcedimientoRequest(
    Guid TarifarioProcedimientoId,
    decimal MontoSeguro,
    decimal MontoPaciente,
    decimal MontoTotal,
    decimal? MontoFondo,
    EspecialidadMedica? Especialidad);
