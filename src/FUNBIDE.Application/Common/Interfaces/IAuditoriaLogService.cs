namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Puerto de aplicación para dejar constancia de eventos de negocio relevantes
/// (p. ej. un descargo de inventario) en la tabla append-only de auditoría,
/// independientemente del logging estructurado de peticiones HTTP que hace Serilog.
/// </summary>
public interface IAuditoriaLogService
{
    Task RegistrarEventoAsync(
        string accion,
        string recurso,
        object detalle,
        Guid? usuarioId,
        int? codigoRespuestaHttp,
        CancellationToken cancellationToken);
}
