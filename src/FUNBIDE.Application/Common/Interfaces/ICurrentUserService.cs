using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Abstrae el acceso al usuario autenticado (claims del JWT de Supabase) para que
/// los casos de uso no dependan de HttpContext.
/// </summary>
public interface ICurrentUserService
{
    Guid UsuarioId { get; }
    RolUsuario Rol { get; }
    string Correo { get; }
}
