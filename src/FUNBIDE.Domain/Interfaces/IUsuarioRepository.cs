using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorSupabaseUserIdAsync(Guid supabaseUserId, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken);

    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
