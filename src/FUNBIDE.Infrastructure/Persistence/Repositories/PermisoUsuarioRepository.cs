using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class PermisoUsuarioRepository(FunbideDbContext dbContext) : IPermisoUsuarioRepository
{
    public async Task<IReadOnlyList<PermisoUsuario>> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        await dbContext.PermisosUsuario
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PermisoUsuario>> ObtenerPorSupabaseUserIdAsync(Guid supabaseUserId, CancellationToken cancellationToken) =>
        await dbContext.PermisosUsuario
            .AsNoTracking()
            .Where(p => dbContext.Usuarios.Any(u => u.Id == p.UsuarioId && u.SupabaseUserId == supabaseUserId))
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(PermisoUsuario permiso, CancellationToken cancellationToken) =>
        await dbContext.PermisosUsuario.AddAsync(permiso, cancellationToken);

    public void Eliminar(PermisoUsuario permiso) => dbContext.PermisosUsuario.Remove(permiso);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
