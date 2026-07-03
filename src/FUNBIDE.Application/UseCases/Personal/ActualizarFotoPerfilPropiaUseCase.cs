using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IActualizarFotoPerfilPropiaUseCase : IUseCase<ActualizarFotoPerfilPropiaRequest, UsuarioDto>
{
}

/// <summary>
/// Alimenta la opción "Cambiar foto de perfil" del menú, disponible para cualquier rol
/// autenticado sobre su propio perfil (a diferencia de <see cref="ActualizarFotoPerfilUseCase"/>,
/// que es LEMY actuando sobre el perfil de otro miembro del personal).
/// </summary>
public sealed class ActualizarFotoPerfilPropiaUseCase(
    IUsuarioRepository usuarioRepository,
    ISupabaseStorageService storageService,
    ICurrentUserService currentUser) : IActualizarFotoPerfilPropiaUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(ActualizarFotoPerfilPropiaRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorSupabaseUserIdAsync(currentUser.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), currentUser.UsuarioId);

        var url = await storageService.SubirFotoPerfilAsync(
            usuario.Id, request.Contenido, request.ContentType, cancellationToken);
        usuario.ActualizarFotoPerfil(url);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
