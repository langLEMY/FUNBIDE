namespace FUNBIDE.Application.DTOs.Citas;

public sealed record ProgramarCitaRequest(Guid CitaId, DateTimeOffset Inicio, DateTimeOffset Fin);
