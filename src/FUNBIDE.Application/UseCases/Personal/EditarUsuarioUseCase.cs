using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IEditarUsuarioUseCase : IUseCase<EditarUsuarioRequest, UsuarioDto>
{
}

/// <summary>
/// Edita nombre y correo de un perfil existente. Si el correo cambió, primero lo
/// sincroniza en Supabase Auth (ahí vive la credencial real de inicio de sesión) antes
/// de tocar la proyección local, igual que <see cref="CambiarRolUsuarioUseCase"/>.
/// </summary>
public sealed class EditarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEditarUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(EditarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        var correoNormalizado = request.Correo.Trim().ToLowerInvariant();
        var nombreUsuarioNormalizado = request.NombreUsuario.Trim().ToLowerInvariant();

        if (!string.Equals(usuario.Correo, correoNormalizado, StringComparison.Ordinal))
        {
            var otroUsuarioConEseCorreo = await usuarioRepository.ObtenerPorCorreoAsync(correoNormalizado, cancellationToken);
            if (otroUsuarioConEseCorreo is not null && otroUsuarioConEseCorreo.Id != usuario.Id)
            {
                throw new CorreoEnUsoException(correoNormalizado);
            }

            await supabaseAdmin.CambiarCorreoAsync(usuario.SupabaseUserId, correoNormalizado, cancellationToken);
        }

        if (!string.Equals(usuario.NombreUsuario, nombreUsuarioNormalizado, StringComparison.Ordinal))
        {
            var otroUsuarioConEseNombre = await usuarioRepository.ObtenerPorNombreUsuarioAsync(nombreUsuarioNormalizado, cancellationToken);
            if (otroUsuarioConEseNombre is not null && otroUsuarioConEseNombre.Id != usuario.Id)
            {
                throw new NombreUsuarioEnUsoException(nombreUsuarioNormalizado);
            }
        }

        usuario.ActualizarDatos(request.NombreCompleto, request.Correo, nombreUsuarioNormalizado);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.editar",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(
            usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol,
            usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad);
    }
}
