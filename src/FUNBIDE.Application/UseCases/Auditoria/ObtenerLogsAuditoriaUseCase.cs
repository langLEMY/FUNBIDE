using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Auditoria;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Auditoria;

public interface IObtenerLogsAuditoriaUseCase : IUseCase<ConsultarAuditoriaRequest, IReadOnlyList<AuditoriaLogDto>>
{
}

public sealed class ObtenerLogsAuditoriaUseCase(
    IAuditoriaLogRepository auditoriaLogRepository) : IObtenerLogsAuditoriaUseCase
{
    public async Task<IReadOnlyList<AuditoriaLogDto>> EjecutarAsync(
        ConsultarAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var logs = await auditoriaLogRepository.ObtenerAsync(
            request.Desde, request.Hasta, request.Recurso, cancellationToken);

        return logs
            .OrderByDescending(l => l.RegistradoEn)
            .Select(l => new AuditoriaLogDto(
                l.Id, l.UsuarioId, l.Accion, l.Recurso, l.CodigoRespuestaHttp, l.Detalle.Valor, l.RegistradoEn))
            .ToList();
    }
}
