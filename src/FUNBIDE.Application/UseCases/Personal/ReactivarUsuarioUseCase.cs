using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
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

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
