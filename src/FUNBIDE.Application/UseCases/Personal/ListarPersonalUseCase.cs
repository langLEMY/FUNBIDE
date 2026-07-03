using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IListarPersonalUseCase : IUseCase<IReadOnlyList<UsuarioDto>>
{
}

public sealed class ListarPersonalUseCase(IUsuarioRepository usuarioRepository) : IListarPersonalUseCase
{
    public async Task<IReadOnlyList<UsuarioDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.ObtenerTodosAsync(cancellationToken);

        return usuarios
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Correo, u.Rol, u.Activo, u.FotoPerfilUrl))
            .ToList();
    }
}
