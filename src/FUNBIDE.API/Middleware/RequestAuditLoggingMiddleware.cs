using System.Diagnostics;
using System.Security.Claims;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Emite un evento estructurado de Serilog por cada petición HTTP autenticada, con
/// las propiedades UsuarioId/Accion/Recurso/CodigoRespuestaHttp que el sink de
/// PostgreSQL (ver <c>SerilogConfiguration</c>) proyecta directamente sobre las
/// columnas de "funbide.auditoria_logs". Así toda la API queda auditada sin que
/// cada caso de uso tenga que registrar manualmente el log de acceso.
/// </summary>
public sealed class RequestAuditLoggingMiddleware(RequestDelegate next, ILogger<RequestAuditLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var cronometro = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            cronometro.Stop();

            var usuarioIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("sub")?.Value;
            Guid? usuarioId = Guid.TryParse(usuarioIdClaim, out var id) ? id : null;

            logger.LogInformation(
                "{Accion} respondió {CodigoRespuestaHttp} en {DuracionMs}ms para {UsuarioId} sobre {Recurso}",
                $"http.{context.Request.Method.ToLowerInvariant()}",
                context.Response.StatusCode,
                cronometro.ElapsedMilliseconds,
                usuarioId,
                context.Request.Path.Value);
        }
    }
}
