using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Cuando LEMY ejecuta "Reiniciar servicios" (ver <c>ReiniciarServiciosUseCase</c>), cualquier
/// JWT ya emitido —de cualquier rol, incluido LEMY— deja de servir: se compara el claim "iat"
/// (fecha de emisión, presente en todo JWT válido) contra <c>SesionesRevocadasEn</c>. Si el
/// token es de antes, 401: el frontend debe tratarlo como sesión inválida y volver a /login.
/// Comparte la misma fila/caché que <see cref="MantenimientoMiddleware"/> (ver
/// <see cref="ConfiguracionSistemaCacheClave"/>) para no sumar una segunda lectura a Postgres
/// por request.
/// </summary>
/// <remarks>
/// Limitación conocida (inherente a JWT, no un bug): en el modo de login con Supabase, el
/// cliente puede refrescar el access token en segundo plano y obtener uno nuevo (con un "iat"
/// posterior) antes de que este middleware llegue a rechazarlo. En la práctica esto solo
/// importa en la ventana exacta alrededor del refresh automático; cualquier request normal
/// entremedio ya fuerza el login de nuevo.
/// </remarks>
public sealed class SesionRevocadaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IConfiguracionSistemaRepository configuracionRepository, IMemoryCache cache)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        if (!cache.TryGetValue(ConfiguracionSistemaCacheClave.Valor, out ConfiguracionSistema? configuracion))
        {
            configuracion = await configuracionRepository.ObtenerAsync(context.RequestAborted);
            cache.Set(ConfiguracionSistemaCacheClave.Valor, configuracion, ConfiguracionSistemaCacheClave.Duracion);
        }

        if (configuracion?.SesionesRevocadasEn is { } revocadasEn)
        {
            var iatClaim = context.User.FindFirst("iat")?.Value;
            var tokenEsAnterior = !long.TryParse(iatClaim, out var iatUnix) ||
                DateTimeOffset.FromUnixTimeSeconds(iatUnix) < revocadasEn;

            if (tokenEsAnterior)
            {
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Sesión cerrada de forma remota",
                    Detail = "Tu sesión fue cerrada desde otro dispositivo. Iniciá sesión de nuevo.",
                    Status = StatusCodes.Status401Unauthorized,
                    Instance = context.Request.Path
                });
                return;
            }
        }

        await next(context);
    }
}
