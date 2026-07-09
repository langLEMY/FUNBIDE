using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IListarPersonalUseCase : IUseCase<IReadOnlyList<UsuarioDto>>
{
}

/// <summary>
/// El rol LEMY es de administración interna del sistema y no debe ser visible para
/// ADMIN: quien llama con ese rol nunca ve cuentas LEMY en el listado, ni siquiera
/// las suyas propias si las tuviera. LEMY sí ve todo el personal, incluido otro LEMY.
/// </summary>
public sealed class ListarPersonalUseCase(
    IUsuarioRepository usuarioRepository, ICurrentUserService currentUser) : IListarPersonalUseCase
{
    public async Task<IReadOnlyList<UsuarioDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.ObtenerTodosAsync(cancellationToken);

        if (currentUser.Rol != RolUsuario.Lemy)
        {
            usuarios = usuarios.Where(u => u.Rol != RolUsuario.Lemy).ToList();
        }

        return usuarios
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Correo, u.Rol, u.Activo, u.FotoPerfilUrl))
            .ToList();
    }
}
