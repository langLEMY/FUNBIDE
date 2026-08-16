using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Permisos;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Permisos;

public interface IObtenerPermisosDeUsuarioUseCase : IUseCase<Guid, PermisosUsuarioDto>
{
}

/// <summary>
/// Arma el catálogo completo de <see cref="ModuloPermiso"/> para un usuario puntual,
/// cruzando el default de su rol (<see cref="PermisosPorRolDefault"/>) con sus
/// overrides explícitos. Alimenta la pantalla de edición de permisos.
/// </summary>
public sealed class ObtenerPermisosDeUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IPermisoUsuarioRepository permisoUsuarioRepository,
    ICurrentUserService currentUser) : IObtenerPermisosDeUsuarioUseCase
{
    public async Task<PermisosUsuarioDto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Usuario), usuarioId);

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede ver los permisos de una cuenta Lemy.");
        }

        var overrides = await permisoUsuarioRepository.ObtenerPorUsuarioIdAsync(usuario.Id, cancellationToken);
        var overridesPorModulo = overrides.ToDictionary(o => o.Modulo);
        var defaultsDelRol = PermisosPorRolDefault.Para(usuario.Rol);

        var modulos = Enum.GetValues<ModuloPermiso>()
            .Select(modulo =>
            {
                var defaultDelRol = defaultsDelRol.Contains(modulo);
                var tieneOverride = overridesPorModulo.TryGetValue(modulo, out var permisoOverride);
                var concedido = tieneOverride ? permisoOverride!.Concedido : defaultDelRol;
                return new ModuloPermisoEstadoDto(modulo, concedido, tieneOverride, defaultDelRol);
            })
            .ToList();

        return new PermisosUsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Rol, modulos);
    }
}
