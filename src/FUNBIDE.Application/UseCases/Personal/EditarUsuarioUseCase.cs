using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
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

        var correoNormalizado = request.Correo.Trim().ToLowerInvariant();

        if (!string.Equals(usuario.Correo, correoNormalizado, StringComparison.Ordinal))
        {
            var otroUsuarioConEseCorreo = await usuarioRepository.ObtenerPorCorreoAsync(correoNormalizado, cancellationToken);
            if (otroUsuarioConEseCorreo is not null && otroUsuarioConEseCorreo.Id != usuario.Id)
            {
                throw new CorreoEnUsoException(correoNormalizado);
            }

            await supabaseAdmin.CambiarCorreoAsync(usuario.SupabaseUserId, correoNormalizado, cancellationToken);
        }

        usuario.ActualizarDatos(request.NombreCompleto, request.Correo);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.editar",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.NombreCompleto, usuario.Correo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
