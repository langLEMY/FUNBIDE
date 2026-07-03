using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IEliminarUsuarioPermanentementeUseCase : IUseCase<Guid, UsuarioDto>
{
}

/// <summary>
/// A diferencia de <see cref="EliminarUsuarioUseCase"/> (revoca acceso, reversible con
/// <c>ReactivarUsuarioUseCase</c>), esto borra al usuario de Supabase Auth de forma
/// irreversible — no puede deshacerse. La fila local en "usuarios" igual se conserva
/// (desactivada) porque citas, historial clínico, movimientos financieros/de
/// inventario y auditoría ya registrados referencian este id y deben poder seguir
/// mostrando quién los generó.
/// </summary>
public sealed class EliminarUsuarioPermanentementeUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEliminarUsuarioPermanentementeUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), usuarioId);

        if (usuario.SupabaseUserId == currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException("No puedes eliminar permanentemente tu propio perfil.");
        }

        await supabaseAdmin.EliminarPermanentementeAsync(usuario.SupabaseUserId, cancellationToken);
        usuario.EliminarPermanentemente();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.eliminar-permanente",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.Correo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
