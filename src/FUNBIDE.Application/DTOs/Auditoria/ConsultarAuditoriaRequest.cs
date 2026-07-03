namespace FUNBIDE.Application.DTOs.Auditoria;

public sealed record ConsultarAuditoriaRequest(DateTimeOffset? Desde, DateTimeOffset? Hasta, string? Recurso);
