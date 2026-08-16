namespace FUNBIDE.Application.DTOs.Sistema;

public sealed record ResultadoBackupManualDto(bool Exitoso, DateTimeOffset EjecutadoEn, string? Mensaje);
