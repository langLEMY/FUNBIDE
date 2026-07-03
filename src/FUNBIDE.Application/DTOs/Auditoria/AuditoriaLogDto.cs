namespace FUNBIDE.Application.DTOs.Auditoria;

public sealed record AuditoriaLogDto(
    Guid Id,
    Guid? UsuarioId,
    string Accion,
    string Recurso,
    int? CodigoRespuestaHttp,
    string Detalle,
    DateTimeOffset RegistradoEn);
