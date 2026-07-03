using System.Text.Json;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Infrastructure.Logging;

public sealed class AuditoriaLogService(IAuditoriaLogRepository auditoriaLogRepository) : IAuditoriaLogService
{
    public Task RegistrarEventoAsync(
        string accion,
        string recurso,
        object detalle,
        Guid? usuarioId,
        int? codigoRespuestaHttp,
        CancellationToken cancellationToken)
    {
        var detalleJson = DocumentoJson.Crear(JsonSerializer.Serialize(detalle));
        var log = new AuditoriaLog(accion, recurso, detalleJson, usuarioId, codigoRespuestaHttp);

        return auditoriaLogRepository.RegistrarAsync(log, cancellationToken);
    }
}
