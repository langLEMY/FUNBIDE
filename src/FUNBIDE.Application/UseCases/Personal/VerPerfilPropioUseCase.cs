using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IVerPerfilPropioUseCase : IUseCase<UsuarioDto>
{
}

/// <summary>
/// Alimenta la opción "Ver perfil" del menú de la foto de perfil, disponible para
/// cualquier rol autenticado (no solo LEMY).
/// </summary>
public sealed class VerPerfilPropioUseCase(
    IUsuarioRepository usuarioRepository,
    ICurrentUserService currentUser) : IVerPerfilPropioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorSupabaseUserIdAsync(currentUser.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), currentUser.UsuarioId);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl);
    }
}
