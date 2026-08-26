namespace FUNBIDE.Application.DTOs.SegurosMedicos;

public sealed record TarifarioProcedimientoDto(
    Guid Id,
    Guid SeguroMedicoId,
    string Plan,
    string Procedimiento,
    decimal MontoSeguro,
    decimal MontoPaciente,
    decimal MontoTotal,
    decimal? MontoFondo,
    string? Especialidad);
