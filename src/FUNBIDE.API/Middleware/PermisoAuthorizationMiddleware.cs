using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Valida el set efectivo de permisos (default del rol + overrides por usuario, ver
/// <see cref="IPermisoResolverService"/>) contra <see cref="RequierePermisoAttribute"/>
/// del endpoint resuelto. Se ejecuta justo después de <see cref="RoleAuthorizationMiddleware"/>:
/// en los controllers donde <c>[RequiereRol]</c> de clase fue reemplazado por
/// <c>[RequierePermiso]</c>, ese middleware ya no encuentra atributo y deja pasar, así
/// que este es quien realmente autoriza el acceso — por eso repite el chequeo de
/// autenticación (401) en vez de asumir que ya se hizo.
/// </summary>
public sealed class PermisoAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPermisoResolverService permisoResolver)
    {
        var requisito = context.GetEndpoint()?.Metadata.GetMetadata<RequierePermisoAttribute>();

        if (requisito is null)
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await EscribirErrorAsync(context, StatusCodes.Status401Unauthorized, "No autenticado",
                "Esta operación requiere un token de acceso válido.");
            return;
        }

        var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst("sub");
        var rolClaim = context.User.FindFirst(ClaimTypes.Role) ?? context.User.FindFirst("role");

        if (subClaim is null || !Guid.TryParse(subClaim.Value, out var usuarioId) ||
            rolClaim is null || !Enum.TryParse<RolUsuario>(rolClaim.Value, ignoreCase: true, out var rol))
        {
            await EscribirErrorAsync(context, StatusCodes.Status403Forbidden, "Acceso denegado",
                "El token no contiene una identidad reconocida por FUNBIDE.");
            return;
        }

        var permisosEfectivos = await permisoResolver.ObtenerPermisosEfectivosAsync(usuarioId, rol, context.RequestAborted);

        if (!requisito.ModulosPermitidos.Any(permisosEfectivos.Contains))
        {
            await EscribirErrorAsync(context, StatusCodes.Status403Forbidden, "Acceso denegado",
                "No tenés acceso concedido a este módulo. Pedile a un administrador que te lo habilite.");
            return;
        }

        await next(context);
    }

    private static Task EscribirErrorAsync(HttpContext context, int statusCode, string titulo, string detalle)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = titulo,
            Detail = detalle,
            Status = statusCode,
            Instance = context.Request.Path
        });
    }
}
