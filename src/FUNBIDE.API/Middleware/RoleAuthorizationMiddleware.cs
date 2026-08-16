using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Valida el rol de negocio del usuario autenticado contra <see cref="RequiereRolAttribute"/>
/// del endpoint resuelto. Debe ejecutarse después de UseAuthentication (para tener
/// HttpContext.User poblado) y antes de que el request llegue al controlador.
/// Endpoints sin el atributo quedan abiertos a cualquier usuario autenticado.
/// </summary>
public sealed class RoleAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requisito = context.GetEndpoint()?.Metadata.GetMetadata<RequiereRolAttribute>();

        if (requisito is null)
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "No autenticado",
                Detail = "Esta operación requiere un token de acceso válido.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
            });
            return;
        }

        var rolClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (rolClaim is null ||
            !Enum.TryParse<RolUsuario>(rolClaim, ignoreCase: true, out var rolUsuario) ||
            !requisito.RolesPermitidos.Contains(rolUsuario))
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Acceso denegado",
                Detail = $"El rol requerido para este recurso es: {string.Join(", ", requisito.RolesPermitidos)}.",
                Status = StatusCodes.Status403Forbidden,
                Instance = context.Request.Path
            });
            return;
        }

        await next(context);
    }
}
