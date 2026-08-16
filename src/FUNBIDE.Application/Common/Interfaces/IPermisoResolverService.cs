using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Calcula el set efectivo de <see cref="ModuloPermiso"/> de un usuario: el default de
/// su rol (<see cref="PermisosPorRolDefault"/>) combinado con sus overrides explícitos.
/// A propósito, es un servicio aparte de <see cref="ICurrentUserService"/> (que sigue
/// siendo solo-claims, sin tocar la base de datos): este sí consulta la tabla de
/// permisos, así que solo se inyecta donde realmente hace falta.
/// </summary>
public interface IPermisoResolverService
{
    Task<IReadOnlySet<ModuloPermiso>> ObtenerPermisosEfectivosAsync(
        Guid supabaseUserId, RolUsuario rol, CancellationToken cancellationToken);
}
