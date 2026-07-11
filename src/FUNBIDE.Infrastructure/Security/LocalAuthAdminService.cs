using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Implementa <see cref="ISupabaseAdminService"/> para el modo Auth:Provider=Local: no
/// hay Supabase, así que solo hace falta generar un id de usuario y guardar el hash de
/// la contraseña. El rol/correo/estado activo ya viven en <c>Usuario</c> (la tabla local
/// de FUNBIDE) y los casos de uso de Personal ya los actualizan ahí mismo junto con esta
/// llamada — por eso <see cref="CambiarRolAsync"/>/<see cref="CambiarCorreoAsync"/>/
/// <see cref="RevocarAccesoAsync"/>/<see cref="RestaurarAccesoAsync"/>/
/// <see cref="EliminarPermanentementeAsync"/> son no-ops acá: no hay una segunda fuente
/// de verdad que sincronizar, a diferencia de Supabase Auth.
/// </summary>
public sealed class LocalAuthAdminService(FunbideDbContext dbContext) : ISupabaseAdminService
{
    private readonly PasswordHasher<CredencialLocal> hasher = new();

    public async Task<Guid> CrearUsuarioAsync(
        string correo, string contrasenaTemporal, RolUsuario rol, CancellationToken cancellationToken)
    {
        var usuarioId = Guid.NewGuid();
        var credencial = new CredencialLocal(usuarioId, string.Empty);
        credencial.ActualizarPasswordHash(hasher.HashPassword(credencial, contrasenaTemporal));

        await dbContext.CredencialesLocales.AddAsync(credencial, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return usuarioId;
    }

    public async Task CambiarContrasenaAsync(
        Guid supabaseUserId, string nuevaContrasena, CancellationToken cancellationToken)
    {
        var credencial = await dbContext.CredencialesLocales
            .FirstOrDefaultAsync(c => c.UsuarioId == supabaseUserId, cancellationToken)
            ?? throw new InvalidOperationException("No hay una credencial local para este usuario.");

        credencial.ActualizarPasswordHash(hasher.HashPassword(credencial, nuevaContrasena));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task CambiarRolAsync(Guid supabaseUserId, RolUsuario nuevoRol, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task CambiarCorreoAsync(Guid supabaseUserId, string nuevoCorreo, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task RevocarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task RestaurarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task EliminarPermanentementeAsync(Guid supabaseUserId, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
