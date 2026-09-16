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
    private static readonly TimeSpan RangoPorDefecto = TimeSpan.FromDays(30);

    // La tabla de auditoría crece sin límite (cada login, cada cambio, cada acción queda
    // registrada) — a diferencia del span, que protege contra un rango de fechas absurdo,
    // esto protege contra un rango corto pero con muchísimo volumen (ej. un día con miles
    // de eventos). Nadie revisa manualmente más de esto de una sola vez.
    private const int SpanMaximoDias = 366;
    private const int TamanoSinPaginacionExplicita = 10_000;
    private const int TamanoPaginaPorDefecto = 50;
    private const int TamanoPaginaMaximo = 100;

    public async Task<IReadOnlyList<AuditoriaLogDto>> EjecutarAsync(
        ConsultarAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var hasta = request.Hasta ?? DateTimeOffset.UtcNow;
        var desde = request.Desde ?? hasta - RangoPorDefecto;

        // Igual que el clamp de página en ListarPacientesUseCase: protege contra un rango
        // absurdo (ej. "desde el año 1" hasta hoy) sin devolver un error, simplemente
        // recorta al span máximo razonable.
        if (hasta - desde > TimeSpan.FromDays(SpanMaximoDias))
        {
            desde = hasta - TimeSpan.FromDays(SpanMaximoDias);
        }

        int pagina;
        int tamanoPagina;
        if (request.Pagina is null && request.TamanoPagina is null)
        {
            // Nadie pidió paginación explícitamente (caso de hoy): se devuelve todo el
            // rango de una sola vez, como siempre, pero con un techo duro por si el rango
            // de fechas (ya acotado arriba) igual contiene un volumen fuera de lo normal.
            pagina = 1;
            tamanoPagina = TamanoSinPaginacionExplicita;
        }
        else
        {
            pagina = Math.Clamp(request.Pagina ?? 1, 1, 1_000_000);
            tamanoPagina = request.TamanoPagina is null or < 1
                ? TamanoPaginaPorDefecto
                : Math.Min(request.TamanoPagina.Value, TamanoPaginaMaximo);
        }

        var (logs, _) = await auditoriaLogRepository.ObtenerPaginadoAsync(
            desde, hasta, request.Recurso, pagina, tamanoPagina, cancellationToken);

        return logs
            .Select(l => new AuditoriaLogDto(
                l.Id, l.UsuarioId, l.Accion, l.Recurso, l.CodigoRespuestaHttp, l.Detalle.Valor, l.RegistradoEn))
            .ToList();
    }
}
