using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Infrastructure.Persistence;
using FUNBIDE.Infrastructure.Resiliencia;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Verifica credenciales contra <see cref="CredencialLocal"/> y emite el JWT del modo
/// Auth:Provider=Local. El rol/correo se leen frescos de <c>Usuario</c> en cada login
/// — a diferencia de Supabase, acá no hay un token pre-emitido que pueda quedar
/// desactualizado, así que no hace falta sincronizar nada por separado.
/// </summary>
/// <remarks>
/// Soporta dos formatos de hash en <see cref="CredencialLocal.PasswordHash"/>: bcrypt
/// (prefijo "$2") para cuentas migradas 1:1 desde <c>auth.users.encrypted_password</c> de
/// Supabase (preserva la contraseña real de quien ya la tenía, sin obligar a un reseteo),
/// y el formato de <see cref="PasswordHasher{TUser}"/> para cualquier credencial creada o
/// cambiada después dentro de este modo Local (sembrador inicial, "cambiar contraseña").
/// </remarks>
public sealed class AutenticacionLocalService(
    FunbideDbContext dbContext, IOptions<LocalJwtOptions> opciones) : IAutenticacionLocalService
{
    // Hash ficticio de una contraseña aleatoria fija, calculado una sola vez: cuando el
    // correo no existe (o la cuenta está inactiva/borrada), igual corremos una
    // verificación contra esto antes de devolver null. Sin esto, "correo no existe"
    // devolvía al instante mientras "correo existe" tardaba lo que tarda hashear/verificar
    // — una diferencia de tiempo medible que permite enumerar correos válidos por fuerza bruta.
    private static readonly string HashFicticio =
        new PasswordHasher<CredencialLocal>().HashPassword(new CredencialLocal(Guid.Empty, string.Empty), Guid.NewGuid().ToString());

    public async Task<TokenLocalResultado?> IniciarSesionAsync(
        string correo, string contrasena, CancellationToken cancellationToken)
    {
        var correoNormalizado = correo.Trim().ToLowerInvariant();

        // Envuelto en PoliticaReintentoLectura (mismo patrón que PacienteRepository): es la
        // PRIMERA consulta que toca la base tras abrir la app, así que un corte de red breve
        // acá terminaba disparando la pantalla de mantenimiento completa (ExceptionHandlingMiddleware
        // -> 503) en el peor momento posible, justo al iniciar sesión, en vez de reintentarse
        // solo. Es una lectura pura (SELECT), así que reintentarla es seguro.
        var (usuario, credencial) = await PoliticaReintentoLectura.EjecutarAsync(async ct =>
        {
            // "!EliminadoPermanentemente" es necesario, no cosmético: desde que el índice
            // único de Correo quedó filtrado (para poder reactivar un correo de una cuenta
            // borrada permanentemente, ver UsuarioConfiguration), puede haber DOS filas con
            // el mismo correo -- la vieja (desactivada, tombstone) y la nueva cuenta real.
            // Sin este filtro, esta consulta podía traer cualquiera de las dos sin orden
            // definido, y si tocaba la vieja, el login fallaba siempre para la cuenta nueva
            // aunque la contraseña fuera correcta.
            var usuarioEncontrado = await dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado && !u.EliminadoPermanentemente, ct);

            CredencialLocal? credencialEncontrada = null;
            if (usuarioEncontrado is not null && usuarioEncontrado.Activo && !usuarioEncontrado.EliminadoPermanentemente)
            {
                credencialEncontrada = await dbContext.CredencialesLocales
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioEncontrado.SupabaseUserId, ct);
            }

            return (usuarioEncontrado, credencialEncontrada);
        }, cancellationToken);

        var passwordValida = VerificarContrasena(credencial?.PasswordHash ?? HashFicticio, contrasena);

        if (usuario is null || !usuario.Activo || usuario.EliminadoPermanentemente ||
            credencial is null || !passwordValida)
        {
            return null;
        }

        var opts = opciones.Value;
        var ahora = DateTimeOffset.UtcNow;
        var expira = ahora.Add(opts.DuracionToken);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.SupabaseUserId.ToString()),
            new Claim("role", usuario.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            // Requerido por SesionRevocadaMiddleware para comparar contra
            // ConfiguracionSistema.SesionesRevocadasEn — el constructor de
            // JwtSecurityToken usado abajo NO lo agrega solo (a diferencia de "nbf"/"exp",
            // que sí toma de notBefore/expires), así que sin esto todo token local queda
            // sin "iat" y el middleware lo trata como anterior a cualquier revocación.
            new Claim(JwtRegisteredClaimNames.Iat, ahora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(opts.SigningKeyBase64));
        var credenciales = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            notBefore: ahora.UtcDateTime,
            expires: expira.UtcDateTime,
            signingCredentials: credenciales);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenLocalResultado(accessToken, expira);
    }

    // Los hashes bcrypt (los que trae una cuenta migrada 1:1 desde Supabase) siempre
    // empiezan con "$2" (variantes $2a$/$2b$/$2y$) — cualquier otra cosa es un hash de
    // PasswordHasher<T> generado por este mismo modo Local (sembrador o cambio de
    // contraseña posterior). BCrypt.Verify ya corre en tiempo constante internamente.
    private static bool VerificarContrasena(string hash, string contrasena)
    {
        if (hash.StartsWith("$2", StringComparison.Ordinal))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(contrasena, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        var hasher = new PasswordHasher<CredencialLocal>();
        var resultado = hasher.VerifyHashedPassword(new CredencialLocal(Guid.Empty, string.Empty), hash, contrasena);
        return resultado != PasswordVerificationResult.Failed;
    }
}
