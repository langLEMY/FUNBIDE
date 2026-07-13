using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IEliminarUsuarioUseCase : IUseCase<Guid, UsuarioDto>
{
}

/// <summary>
/// "Eliminar" un perfil revoca su acceso en Supabase Auth (no puede volver a iniciar
/// sesión) y desactiva la proyección local, pero no borra la fila: citas, historial
/// clínico y movimientos de inventario ya registrados referencian este id y deben
/// poder seguir mostrando quién los generó.
/// </summary>
public sealed class EliminarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEliminarUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), usuarioId);

        if (usuario.SupabaseUserId == currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException("No puedes eliminar tu propio perfil.");
        }

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        await supabaseAdmin.RevocarAccesoAsync(usuario.SupabaseUserId, cancellationToken);
        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.eliminar",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.Correo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
