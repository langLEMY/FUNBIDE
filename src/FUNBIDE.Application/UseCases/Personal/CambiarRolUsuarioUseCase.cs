using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICambiarRolUsuarioUseCase : IUseCase<CambiarRolRequest, UsuarioDto>
{
}

/// <summary>
/// Sincroniza app_metadata.role en Supabase Auth (fuente real del claim de rol del
/// JWT) antes de actualizar la proyección local, para que ambos nunca queden
/// desalineados si la llamada a Supabase falla.
/// </summary>
public sealed class CambiarRolUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICambiarRolUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CambiarRolRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.SupabaseUserId == currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException("No puedes cambiar tu propio rol.");
        }

        if ((usuario.Rol == RolUsuario.Lemy || request.NuevoRol == RolUsuario.Lemy) && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        await supabaseAdmin.CambiarRolAsync(usuario.SupabaseUserId, request.NuevoRol, cancellationToken);
        usuario.CambiarRol(request.NuevoRol);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.cambiar-rol",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { NuevoRol = request.NuevoRol },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
