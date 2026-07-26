namespace FUNBIDE.Application.DTOs.Sistema;

public sealed record EstadoSistemaDto(
    bool BaseDeDatosOperativa,
    DateTimeOffset? UltimoBackupUtc,
    bool? UltimoBackupExitoso,
    bool AlmacenamientoOperativo,
    bool ModoMantenimientoActivo,
    string? ModoMantenimientoMensaje);
