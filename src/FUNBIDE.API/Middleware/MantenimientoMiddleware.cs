using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Cuando LEMY activa el modo mantenimiento (ver <c>CambiarModoMantenimientoUseCase</c>),
/// bloquea con 503 cualquier request autenticado que no sea de una cuenta LEMY — LEMY
/// sigue operando normal para poder apagarlo de nuevo. Los endpoints marcados con
/// <see cref="VisibleDuranteMantenimientoAttribute"/> (el panel "Estado del sistema") no
/// se bloquean, para que Admin siga viendo por qué el resto no responde. Las requests
/// anónimas (login, health check, el frontend estático) tampoco se tocan.
/// </summary>
public sealed class MantenimientoMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IConfiguracionSistemaRepository configuracionRepository)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var rolClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.Equals(rolClaim, nameof(RolUsuario.Lemy), StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (context.GetEndpoint()?.Metadata.GetMetadata<VisibleDuranteMantenimientoAttribute>() is not null)
        {
            await next(context);
            return;
        }

        var configuracion = await configuracionRepository.ObtenerAsync(context.RequestAborted);
        if (configuracion is { ModoMantenimientoActivo: true })
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                titulo = "Sistema en mantenimiento",
                detalle = configuracion.ModoMantenimientoMensaje
                    ?? "El sistema está temporalmente en mantenimiento. Intenta de nuevo más tarde."
            });
            return;
        }

        await next(context);
    }
}
