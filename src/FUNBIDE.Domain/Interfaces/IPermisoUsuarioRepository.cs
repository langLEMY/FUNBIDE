using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IPermisoUsuarioRepository
{
    Task<IReadOnlyList<PermisoUsuario>> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken);

    /// <summary>Usado por el resolver de permisos, que solo conoce el SupabaseUserId del JWT.</summary>
    Task<IReadOnlyList<PermisoUsuario>> ObtenerPorSupabaseUserIdAsync(Guid supabaseUserId, CancellationToken cancellationToken);

    Task AgregarAsync(PermisoUsuario permiso, CancellationToken cancellationToken);

    void Eliminar(PermisoUsuario permiso);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
