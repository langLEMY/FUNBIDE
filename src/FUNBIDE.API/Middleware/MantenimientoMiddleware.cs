using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

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
    public async Task InvokeAsync(HttpContext context, IConfiguracionSistemaRepository configuracionRepository, IMemoryCache cache)
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

        // Cacheado con TTL corto (más la invalidación explícita de
        // CambiarModoMantenimientoUseCase) para no consultar Postgres por este flag global
        // en cada request autenticado — antes era una lectura de base por request.
        if (!cache.TryGetValue(ConfiguracionSistemaCacheClave.Valor, out ConfiguracionSistema? configuracion))
        {
            configuracion = await configuracionRepository.ObtenerAsync(context.RequestAborted);
            cache.Set(ConfiguracionSistemaCacheClave.Valor, configuracion, ConfiguracionSistemaCacheClave.Duracion);
        }

        if (configuracion is { ModoMantenimientoActivo: true })
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            // Detail queda null si LEMY no puso un mensaje propio a propósito: el frontend
            // (ver ModoMantenimientoScreen.tsx) ya tiene su propio texto genérico para ese
            // caso — evita mantener el mismo texto por defecto duplicado en dos capas.
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Sistema en mantenimiento",
                Detail = configuracion.ModoMantenimientoMensaje,
                Status = StatusCodes.Status503ServiceUnavailable,
                Instance = context.Request.Path
            });
            return;
        }

        await next(context);
    }
}
