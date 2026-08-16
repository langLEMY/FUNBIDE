using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IActualizarFotoPerfilUseCase : IUseCase<ActualizarFotoPerfilRequest, UsuarioDto>
{
}

/// <summary>
/// Usado por LEMY y ADMIN para subir o reemplazar la foto de referencia de cualquier
/// miembro del personal (por id local), p. ej. el ícono de un doctor mostrado en el
/// panel. Igual que el resto de <c>PersonalController</c>, una cuenta Lemy solo puede
/// ser tocada por otra cuenta Lemy.
/// </summary>
public sealed class ActualizarFotoPerfilUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseStorageService storageService,
    ICurrentUserService currentUser) : IActualizarFotoPerfilUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(ActualizarFotoPerfilRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.Rol == RolUsuario.Lemy && currentUser.Rol != RolUsuario.Lemy)
        {
            throw new OperacionNoPermitidaException("Solo una cuenta Lemy puede administrar cuentas con rol Lemy.");
        }

        var url = await storageService.SubirFotoPerfilAsync(
            usuario.Id, request.Contenido, request.ContentType, cancellationToken);
        usuario.ActualizarFotoPerfil(url);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad);
    }
}
