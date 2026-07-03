using System.Security.Claims;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Extrae la identidad del usuario a partir de los claims ya validados del JWT de
/// Supabase colocados en <see cref="HttpContext.User"/> por el middleware de
/// autenticación. No vuelve a tocar la base de datos: confía en el token.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal Usuario =>
        httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No hay un HttpContext con un usuario autenticado disponible.");

    public Guid UsuarioId
    {
        get
        {
            var claim = Usuario.FindFirst(ClaimTypes.NameIdentifier) ?? Usuario.FindFirst("sub");
            if (claim is null || !Guid.TryParse(claim.Value, out var id))
            {
                throw new InvalidOperationException("El token no contiene un identificador de usuario válido (claim 'sub').");
            }

            return id;
        }
    }

    public RolUsuario Rol
    {
        get
        {
            var claim = Usuario.FindFirst(ClaimTypes.Role) ?? Usuario.FindFirst("role");
            if (claim is null || !Enum.TryParse<RolUsuario>(claim.Value, ignoreCase: true, out var rol))
            {
                throw new InvalidOperationException("El token no contiene un rol reconocido por FUNBIDE.");
            }

            return rol;
        }
    }

    public string Correo => Usuario.FindFirst(ClaimTypes.Email)?.Value
        ?? Usuario.FindFirst("email")?.Value
        ?? string.Empty;
}
