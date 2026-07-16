using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Infrastructure.Persistence;
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
        var usuario = await dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == correoNormalizado, cancellationToken);

        CredencialLocal? credencial = null;
        if (usuario is not null && usuario.Activo && !usuario.EliminadoPermanentemente)
        {
            credencial = await dbContext.CredencialesLocales
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.SupabaseUserId, cancellationToken);
        }

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
