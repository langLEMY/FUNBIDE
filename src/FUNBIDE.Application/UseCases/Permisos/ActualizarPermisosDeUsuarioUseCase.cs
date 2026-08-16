using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Permisos;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Permisos;

public interface IActualizarPermisosDeUsuarioUseCase : IUseCase<ActualizarPermisosDeUsuarioRequest, PermisosUsuarioDto>
{
}

/// <summary>
/// Guarda overrides de permisos para un usuario: por cada módulo deseado, si coincide
/// con el default de su rol borra el override (vuelve a heredar), si difiere lo crea o
/// actualiza. Replica las mismas reglas de negocio que <c>CambiarRolUsuarioUseCase</c>:
/// nadie modifica sus propios permisos, y solo una cuenta Lemy administra permisos de
/// otra cuenta Lemy.
/// </summary>
public sealed class ActualizarPermisosDeUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IPermisoUsuarioRepository permisoUsuarioRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService,
    IObtenerPermisosDeUsuarioUseCase obtenerPermisosDeUsuario) : IActualizarPermisosDeUsuarioUseCase
{
    public async Task<PermisosUsuarioDto> EjecutarAsync(ActualizarPermisosDeUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Usuario), request.UsuarioId);

        if (usuario.SupabaseUserId == currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException("No puedes modificar tus propios permisos.");
        }

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar los permisos de una cuenta Lemy.");
        }

        var ahora = dateTimeProvider.UtcNow;
        var defaultsDelRol = PermisosPorRolDefault.Para(usuario.Rol);
        var overrides = await permisoUsuarioRepository.ObtenerPorUsuarioIdAsync(usuario.Id, cancellationToken);
        var overridesPorModulo = overrides.ToDictionary(o => o.Modulo);

        foreach (var deseado in request.Permisos)
        {
            var defaultDelRol = defaultsDelRol.Contains(deseado.Modulo);
            var tieneOverride = overridesPorModulo.TryGetValue(deseado.Modulo, out var existente);

            if (deseado.Concedido == defaultDelRol)
            {
                if (tieneOverride)
                {
                    permisoUsuarioRepository.Eliminar(existente!);
                }
            }
            else if (tieneOverride)
            {
                existente!.Actualizar(deseado.Concedido, currentUser.UsuarioId, ahora);
            }
            else
            {
                await permisoUsuarioRepository.AgregarAsync(
                    new PermisoUsuario(usuario.Id, deseado.Modulo, deseado.Concedido, currentUser.UsuarioId, ahora),
                    cancellationToken);
            }
        }

        await permisoUsuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "permisos.actualizar",
            recurso: $"usuarios/{usuario.Id}/permisos",
            detalle: request.Permisos,
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return await obtenerPermisosDeUsuario.EjecutarAsync(usuario.Id, cancellationToken);
    }
}
