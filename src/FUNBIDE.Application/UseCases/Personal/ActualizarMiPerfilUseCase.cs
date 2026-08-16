using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IActualizarMiPerfilUseCase : IUseCase<ActualizarMiPerfilRequest, UsuarioDto>
{
}

/// <summary>
/// Autoservicio de "Editar nombre y correo", disponible para cualquier rol autenticado
/// (a diferencia de <see cref="EditarUsuarioUseCase"/>, que es LEMY editando a otra
/// persona): usa <see cref="ICurrentUserService.UsuarioId"/> en vez de recibir un
/// <c>UsuarioId</c> externo, pero sincroniza el correo en Supabase igual que aquel.
/// </summary>
public sealed class ActualizarMiPerfilUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IActualizarMiPerfilUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(ActualizarMiPerfilRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorSupabaseUserIdAsync(currentUser.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), currentUser.UsuarioId);

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

        // El nombre de usuario no es autoservicio (evita que alguien se cambie el
        // identificador de login sin que quede claro para el resto del equipo);
        // solo LEMY/Admin lo tocan desde EditarUsuarioUseCase.
        usuario.ActualizarDatos(request.NombreCompleto, request.Correo, usuario.NombreUsuario);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "mi-perfil.actualizar",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.NombreCompleto, usuario.Correo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad);
    }
}
