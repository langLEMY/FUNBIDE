using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Infrastructure.Persistence;
using FUNBIDE.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FUNBIDE.API.Tests.Security;

/// <summary>
/// Cubre el camino de login local de mayor riesgo: la detección dual bcrypt/PasswordHasher
/// en <see cref="AutenticacionLocalService"/> (agregada para preservar contraseñas
/// migradas 1:1 desde Supabase), antes sin ningún test.
/// </summary>
public class AutenticacionLocalServiceTests
{
    private static FunbideDbContext CrearDbContext()
    {
        var opciones = new DbContextOptionsBuilder<FunbideDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FunbideDbContext(opciones);
    }

    private static AutenticacionLocalService CrearServicio(FunbideDbContext dbContext) =>
        new(dbContext, Options.Create(new LocalJwtOptions
        {
            SigningKeyBase64 = Convert.ToBase64String(new byte[32]),
        }));

    private static async Task<Usuario> CrearUsuarioConCredencialAsync(
        FunbideDbContext dbContext, string passwordHash, string correo = "doctora@funbide.local",
        bool activo = true, bool eliminadoPermanentemente = false)
    {
        var usuario = new Usuario(Guid.NewGuid(), "Doctora De Prueba", correo, "doctora.prueba", RolUsuario.Doctor);
        if (!activo) usuario.Desactivar();
        if (eliminadoPermanentemente) usuario.EliminarPermanentemente();

        dbContext.Usuarios.Add(usuario);
        dbContext.CredencialesLocales.Add(new CredencialLocal(usuario.SupabaseUserId, passwordHash));
        await dbContext.SaveChangesAsync(CancellationToken.None);
        return usuario;
    }

    [Fact]
    public async Task IniciarSesionAsync_HashBcryptMigrado_AutenticaConLaContrasenaOriginal()
    {
        using var dbContext = CrearDbContext();
        var hashBcrypt = BCrypt.Net.BCrypt.HashPassword("ContrasenaMigrada123!");
        await CrearUsuarioConCredencialAsync(dbContext, hashBcrypt);

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("doctora@funbide.local", "ContrasenaMigrada123!", CancellationToken.None);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_HashBcryptMigrado_RechazaContrasenaIncorrecta()
    {
        using var dbContext = CrearDbContext();
        var hashBcrypt = BCrypt.Net.BCrypt.HashPassword("ContrasenaMigrada123!");
        await CrearUsuarioConCredencialAsync(dbContext, hashBcrypt);

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("doctora@funbide.local", "otra-contrasena", CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_HashCreadoLocalmente_Autentica()
    {
        using var dbContext = CrearDbContext();
        var hasher = new PasswordHasher<CredencialLocal>();
        var hashLocal = hasher.HashPassword(new CredencialLocal(Guid.Empty, string.Empty), "ContrasenaLocal123!");
        await CrearUsuarioConCredencialAsync(dbContext, hashLocal);

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("doctora@funbide.local", "ContrasenaLocal123!", CancellationToken.None);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_HashBcryptMalformado_NoLanzaYRechaza()
    {
        using var dbContext = CrearDbContext();
        // "$2" con el resto corrupto: dispara SaltParseException dentro de BCrypt.Verify,
        // que VerificarContrasena debe atrapar y traducir en "credenciales inválidas", no
        // en una excepción no controlada que tumbe el login.
        await CrearUsuarioConCredencialAsync(dbContext, "$2a$notunhashvalido");

        var resultado = await Record.ExceptionAsync(() =>
            CrearServicio(dbContext).IniciarSesionAsync("doctora@funbide.local", "cualquiera", CancellationToken.None));

        Assert.Null(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_CorreoInexistente_DevuelveNuloSinLanzar()
    {
        using var dbContext = CrearDbContext();

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("no-existe@funbide.local", "cualquiera", CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_UsuarioInactivo_RechazaAunqueLaContrasenaSeaCorrecta()
    {
        using var dbContext = CrearDbContext();
        var hashBcrypt = BCrypt.Net.BCrypt.HashPassword("ContrasenaMigrada123!");
        await CrearUsuarioConCredencialAsync(dbContext, hashBcrypt, activo: false);

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("doctora@funbide.local", "ContrasenaMigrada123!", CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task IniciarSesionAsync_UsuarioEliminadoPermanentemente_RechazaAunqueLaContrasenaSeaCorrecta()
    {
        using var dbContext = CrearDbContext();
        var hashBcrypt = BCrypt.Net.BCrypt.HashPassword("ContrasenaMigrada123!");
        await CrearUsuarioConCredencialAsync(dbContext, hashBcrypt, eliminadoPermanentemente: true);

        var resultado = await CrearServicio(dbContext)
            .IniciarSesionAsync("doctora@funbide.local", "ContrasenaMigrada123!", CancellationToken.None);

        Assert.Null(resultado);
    }
}
