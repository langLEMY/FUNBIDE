using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IReactivarUsuarioUseCase : IUseCase<Guid, UsuarioDto>
{
}

/// <summary>Deshace <see cref="EliminarUsuarioUseCase"/>: restaura el acceso en Supabase Auth y reactiva la proyección local.</summary>
public sealed class ReactivarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IReactivarUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), usuarioId);

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        // Una cuenta borrada permanentemente ya no existe en Supabase Auth (ver
        // EliminarUsuarioPermanentementeUseCase); reactivarla dejaría Activo=true con
        // EliminadoPermanentemente=true, un estado inconsistente e irreversible.
        if (usuario.EliminadoPermanentemente)
        {
            throw new OperacionNoPermitidaException(
                "Esta cuenta fue eliminada permanentemente y no puede reactivarse. Creá una cuenta nueva.");
        }

        await supabaseAdmin.RestaurarAccesoAsync(usuario.SupabaseUserId, cancellationToken);
        usuario.Reactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.reactivar",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.Correo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad);
    }
}
