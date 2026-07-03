using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(FunbideDbContext dbContext) : IUsuarioRepository
{
    // Tracked (no AsNoTracking): CurrentUserService resuelve la identidad del llamador
    // a partir de este método y varios casos de uso de autoservicio (mi-perfil) mutan
    // y guardan la entidad devuelta en la misma unidad de trabajo.
    public Task<Usuario?> ObtenerPorSupabaseUserIdAsync(Guid supabaseUserId, CancellationToken cancellationToken) =>
        dbContext.Usuarios.FirstOrDefaultAsync(u => u.SupabaseUserId == supabaseUserId, cancellationToken);

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken) =>
        dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo, cancellationToken);

    // Excluye a los borrados permanentemente: ya no existen en Supabase Auth, así que
    // no tiene sentido que sigan apareciendo en el listado de personal.
    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Where(u => !u.EliminadoPermanentemente)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AddAsync(usuario, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
