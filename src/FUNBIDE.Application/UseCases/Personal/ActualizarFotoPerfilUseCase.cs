using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IActualizarFotoPerfilUseCase : IUseCase<ActualizarFotoPerfilRequest, UsuarioDto>
{
}

/// <summary>
/// Usado por LEMY para subir o reemplazar la foto de referencia de cualquier miembro
/// del personal (por id local), p. ej. el ícono de un doctor mostrado en el panel.
/// </summary>
public sealed class ActualizarFotoPerfilUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseStorageService storageService) : IActualizarFotoPerfilUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(ActualizarFotoPerfilRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        var url = await storageService.SubirFotoPerfilAsync(
            usuario.Id, request.Contenido, request.ContentType, cancellationToken);
        usuario.ActualizarFotoPerfil(url);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
