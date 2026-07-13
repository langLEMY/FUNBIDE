using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICambiarContrasenaUsuarioUseCase : IUseCase<CambiarContrasenaRequest, UsuarioDto>
{
}

public sealed class CambiarContrasenaUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICambiarContrasenaUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CambiarContrasenaRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        await supabaseAdmin.CambiarContrasenaAsync(usuario.SupabaseUserId, request.NuevaContrasena, cancellationToken);

        // Deliberadamente no se incluye la contraseña (ni un hash) en el detalle auditado.
        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.cambiar-contrasena",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
