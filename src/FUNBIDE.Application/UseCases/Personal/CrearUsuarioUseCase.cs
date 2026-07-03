using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICrearUsuarioUseCase : IUseCase<CrearUsuarioRequest, UsuarioDto>
{
}

/// <summary>
/// Crea el usuario primero en Supabase Auth (fuente real de credenciales y del claim
/// de rol) y solo si eso tiene éxito persiste la proyección local en "usuarios". Si la
/// persistencia local falla, revoca el acceso recién creado en Supabase para no dejar
/// una cuenta huérfana capaz de iniciar sesión sin proyección local.
/// </summary>
public sealed class CrearUsuarioUseCase(
    ISupabaseAdminService supabaseAdmin,
    IUsuarioRepository usuarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICrearUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        var correoNormalizado = request.Correo.Trim().ToLowerInvariant();
        if (await usuarioRepository.ObtenerPorCorreoAsync(correoNormalizado, cancellationToken) is not null)
        {
            throw new CorreoEnUsoException(correoNormalizado);
        }

        var supabaseUserId = await supabaseAdmin.CrearUsuarioAsync(
            request.Correo, request.ContrasenaTemporal, request.Rol, cancellationToken);

        var usuario = new Usuario(supabaseUserId, request.NombreCompleto, request.Correo, request.Rol);

        try
        {
            await usuarioRepository.AgregarAsync(usuario, cancellationToken);
            await usuarioRepository.GuardarCambiosAsync(cancellationToken);
        }
        catch
        {
            await supabaseAdmin.RevocarAccesoAsync(supabaseUserId, CancellationToken.None);
            throw;
        }

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.crear",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { usuario.Correo, usuario.Rol },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
