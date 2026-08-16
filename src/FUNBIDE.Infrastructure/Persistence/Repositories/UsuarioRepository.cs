using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
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

    // Excluye a los borrados permanentemente: su correo debe quedar libre para reusarse
    // (nueva contratación, re-contratar a la misma persona), igual que ObtenerTodosAsync
    // ya los excluye del listado. Sin este filtro, CrearUsuarioUseCase seguía rechazando
    // ese correo para siempre con CorreoEnUsoException.
    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken) =>
        dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo && !u.EliminadoPermanentemente, cancellationToken);

    // Mismo criterio que ObtenerPorCorreoAsync: un usuario borrado permanentemente libera
    // su nombre de usuario para poder reusarse.
    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken) =>
        dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && !u.EliminadoPermanentemente, cancellationToken);

    // Excluye a los borrados permanentemente: ya no existen en Supabase Auth, así que
    // no tiene sentido que sigan apareciendo en el listado de personal.
    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Where(u => !u.EliminadoPermanentemente)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(cancellationToken);

    // Recibe SupabaseUserId (no el Id local): así es como Cita.DoctorId identifica al
    // doctor (ver ListarDoctoresUseCase/CurrentUserService), y este método solo se usa
    // para resolver nombres de doctor a partir de ese mismo DoctorId.
    public async Task<IReadOnlyDictionary<Guid, string>> ObtenerNombresPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await dbContext.Usuarios
            .AsNoTracking()
            .Where(u => ids.Contains(u.SupabaseUserId))
            .ToDictionaryAsync(u => u.SupabaseUserId, u => u.NombreCompleto, cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerActivosPorRolAsync(RolUsuario rol, CancellationToken cancellationToken) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == rol && u.Activo && !u.EliminadoPermanentemente)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AddAsync(usuario, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
