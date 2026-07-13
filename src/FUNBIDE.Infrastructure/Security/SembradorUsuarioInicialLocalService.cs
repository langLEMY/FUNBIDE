using System.Security.Cryptography;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FUNBIDE.Infrastructure.Security;

public interface ISembradorUsuarioInicialLocalService
{
    /// <summary>
    /// Si no hay ningún usuario en la base (instalación offline recién creada), crea uno
    /// con rol Lemy para poder entrar por primera vez y dar de alta al resto del personal.
    /// No hace nada si ya existe al menos un usuario.
    /// </summary>
    Task SembrarSiNecesarioAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Solo tiene sentido en modo Auth:Provider=Local: en modo Supabase el primer usuario se
/// crea a mano desde el dashboard de Supabase, no hay ningún "arranque en frío" que resolver
/// acá. Sin esto, una instalación offline con la base vacía queda sin forma de entrar —
/// todo endpoint exige sesión y crear personal exige rol Lemy (ver PersonalController).
/// </summary>
public sealed class SembradorUsuarioInicialLocalService(
    FunbideDbContext dbContext, ILogger<SembradorUsuarioInicialLocalService> logger) : ISembradorUsuarioInicialLocalService
{
    private const string CorreoAdminInicial = "admin@funbide.local";
    private const string NombreArchivoCredenciales = "credenciales-iniciales.txt";
    private const string AlfabetoContrasena = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
    private const int LongitudContrasena = 14;

    private readonly PasswordHasher<CredencialLocal> hasher = new();

    public async Task SembrarSiNecesarioAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        var contrasenaTemporal = GenerarContrasenaAleatoria();

        var usuario = new Usuario(Guid.NewGuid(), "Administrador", CorreoAdminInicial, RolUsuario.Lemy);
        var credencial = new CredencialLocal(usuario.SupabaseUserId, string.Empty);
        credencial.ActualizarPasswordHash(hasher.HashPassword(credencial, contrasenaTemporal));

        await dbContext.Usuarios.AddAsync(usuario, cancellationToken);
        await dbContext.CredencialesLocales.AddAsync(credencial, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        EscribirArchivoCredenciales(CorreoAdminInicial, contrasenaTemporal);

        // La contraseña va también en el log (no solo en el archivo): en el launcher de
        // escritorio el archivo alcanza (la muestra en un MessageBox y la borra), pero en
        // una instalación en contenedor no hay ventana ni acceso fácil al filesystem del
        // contenedor — "docker compose logs" es la única vía práctica ahí. Instalación de
        // un solo puesto/tenant (ver AuthOptions), no es un secreto de vida larga: la
        // primera acción esperada del administrador es cambiarla.
        logger.LogWarning(
            "Instalación local sin usuarios: se creó la cuenta inicial {Correo} (rol Lemy) con contraseña " +
            "temporal {ContrasenaTemporal} — también quedó en {Archivo}, junto al ejecutable. Cámbiala apenas " +
            "entres; el archivo se borra solo la próxima vez que abras la app.",
            CorreoAdminInicial, contrasenaTemporal, NombreArchivoCredenciales);
    }

    private static void EscribirArchivoCredenciales(string correo, string contrasena)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, NombreArchivoCredenciales);
        var contenido =
            $"""
            FUNBIDE — cuenta inicial de esta instalación

            Correo: {correo}
            Contraseña temporal: {contrasena}

            Cambia esta contraseña apenas entres (menú de tu foto de perfil → Mi perfil).
            Este archivo se borra automáticamente la próxima vez que abras FUNBIDE.
            """;

        File.WriteAllText(ruta, contenido);
    }

    private static string GenerarContrasenaAleatoria()
    {
        Span<char> caracteres = stackalloc char[LongitudContrasena];
        for (var i = 0; i < LongitudContrasena; i++)
        {
            caracteres[i] = AlfabetoContrasena[RandomNumberGenerator.GetInt32(AlfabetoContrasena.Length)];
        }

        return new string(caracteres);
    }
}
