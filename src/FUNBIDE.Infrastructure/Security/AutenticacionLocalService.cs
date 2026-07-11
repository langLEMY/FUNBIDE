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
public sealed class AutenticacionLocalService(
    FunbideDbContext dbContext, IOptions<LocalJwtOptions> opciones) : IAutenticacionLocalService
{
    private readonly PasswordHasher<CredencialLocal> hasher = new();

    public async Task<TokenLocalResultado?> IniciarSesionAsync(
        string correo, string contrasena, CancellationToken cancellationToken)
    {
        var correoNormalizado = correo.Trim().ToLowerInvariant();
        var usuario = await dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == correoNormalizado, cancellationToken);

        if (usuario is null || !usuario.Activo || usuario.EliminadoPermanentemente)
        {
            return null;
        }

        var credencial = await dbContext.CredencialesLocales
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == usuario.SupabaseUserId, cancellationToken);

        if (credencial is null ||
            hasher.VerifyHashedPassword(credencial, credencial.PasswordHash, contrasena) == PasswordVerificationResult.Failed)
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
}
