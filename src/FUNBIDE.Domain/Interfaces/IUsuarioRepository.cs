using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorSupabaseUserIdAsync(Guid supabaseUserId, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken);

    /// <summary>Nombre completo por SupabaseUserId (no el Id local — así identifica Cita.DoctorId al doctor), en lote: para enriquecer listados de citas/cobros sin hacer N consultas.</summary>
    Task<IReadOnlyDictionary<Guid, string>> ObtenerNombresPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken);

    /// <summary>Para los selectores de doctor de Agenda/Recepción (Caja no puede ver el listado completo de personal).</summary>
    Task<IReadOnlyList<Usuario>> ObtenerActivosPorRolAsync(RolUsuario rol, CancellationToken cancellationToken);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
