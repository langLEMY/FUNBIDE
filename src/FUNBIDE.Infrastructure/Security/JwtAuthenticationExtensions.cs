using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FUNBIDE.Infrastructure.Security;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Configura la autenticación JWT contra los tokens emitidos por Supabase Auth y
    /// proyecta el rol de negocio (ADMIN, DOCTOR, FONDOS, LEMY) —guardado en
    /// app_metadata.role— como <see cref="ClaimTypes.Role"/> para que funcione con
    /// [Authorize(Roles = ...)] y con el middleware de validación de roles.
    /// </summary>
    /// <remarks>
    /// Usa <see cref="JwtBearerOptions.Authority"/> en vez de una clave fija: GoTrue
    /// publica un documento OIDC estándar en /auth/v1/.well-known/openid-configuration
    /// con su jwks_uri, así que el middleware descubre y cachea las claves públicas
    /// automáticamente. Los proyectos de Supabase creados hoy firman con ES256
    /// (asimétrico, rotable) por defecto, no con el HS256 de secreto compartido de la
    /// generación anterior — validado contra un proyecto real: la sección de
    /// configuración "Supabase:Jwt:JwtSecret" quedó obsoleta y se eliminó a propósito.
    /// </remarks>
    public static IServiceCollection AddSupabaseJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtSupabaseOptions>()
            .Bind(configuration.GetSection(JwtSupabaseOptions.SeccionConfiguracion))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration
                    .GetSection(JwtSupabaseOptions.SeccionConfiguracion)
                    .Get<JwtSupabaseOptions>()
                    ?? throw new InvalidOperationException(
                        $"Falta la sección de configuración '{JwtSupabaseOptions.SeccionConfiguracion}'.");

                var issuer = $"{jwtOptions.ProjectUrl.TrimEnd('/')}/auth/v1";

                options.Authority = issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.ValidAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "sub",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ProyectarRolDeAppMetadataAsync,
                    OnMessageReceived = LeerTokenDeQueryStringParaSignalRAsync,
                    OnChallenge = EscribirDesafioComoProblemDetailsAsync
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Configura la autenticación JWT del modo Auth:Provider=Local: sin Authority ni
    /// descubrimiento OIDC, valida con la misma clave simétrica con la que
    /// <see cref="AutenticacionLocalService"/> firma el token al iniciar sesión. No
    /// hace falta el truco de <see cref="ProyectarRolDeAppMetadataAsync"/> porque acá
    /// no existe un segundo claim "role" de Postgres con el que choque: el único
    /// claim "role" es el que esta misma API puso, y el mapeo entrante por defecto de
    /// ASP.NET Core ya lo proyecta a <see cref="ClaimTypes.Role"/>.
    /// </summary>
    public static IServiceCollection AddLocalJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<LocalJwtOptions>()
            .Bind(configuration.GetSection(LocalJwtOptions.SeccionConfiguracion))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration
                    .GetSection(LocalJwtOptions.SeccionConfiguracion)
                    .Get<LocalJwtOptions>()
                    ?? throw new InvalidOperationException(
                        $"Falta la sección de configuración '{LocalJwtOptions.SeccionConfiguracion}'.");

                var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.SigningKeyBase64));

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    NameClaimType = "sub",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = LeerTokenDeQueryStringParaSignalRAsync,
                    OnChallenge = EscribirDesafioComoProblemDetailsAsync
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Sin esto, un token ausente/vencido/con firma inválida hace que JwtBearerHandler
    /// escriba su 401 por defecto: solo el header WWW-Authenticate, sin cuerpo. Eso
    /// rompe la consistencia con RoleAuthorizationMiddleware/PermisoAuthorizationMiddleware/
    /// SesionRevocadaMiddleware, que sí devuelven ProblemDetails (RFC 7807) — acá se hace
    /// lo mismo, para que "no autenticado" se vea igual sin importar en qué capa falló.
    /// </summary>
    private static async Task EscribirDesafioComoProblemDetailsAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var detalle = context.AuthenticateFailure switch
        {
            SecurityTokenExpiredException => "Tu sesión expiró. Iniciá sesión de nuevo.",
            null => "Esta operación requiere un token de acceso válido.",
            _ => "El token de acceso no es válido."
        };

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "No autenticado",
            Detail = detalle,
            Status = StatusCodes.Status401Unauthorized,
            Instance = context.Request.Path
        });
    }

    /// <summary>
    /// El navegador no puede mandar un header Authorization en el handshake de WebSocket de
    /// SignalR, así que el cliente manda el JWT como query string (?access_token=...) —
    /// convención estándar de SignalR. Sin esto, JwtBearerHandler solo mira el header y el
    /// Hub queda siempre no autenticado. Acotado a rutas bajo /hubs para no aflojar la
    /// validación de ningún endpoint REST normal.
    /// </summary>
    private static Task LeerTokenDeQueryStringParaSignalRAsync(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;

        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }

    private static Task ProyectarRolDeAppMetadataAsync(TokenValidatedContext context)
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        var appMetadataClaim = context.Principal?.FindFirst("app_metadata");

        if (identity is null || appMetadataClaim is null)
        {
            return Task.CompletedTask;
        }

        using var appMetadata = JsonDocument.Parse(appMetadataClaim.Value);
        if (appMetadata.RootElement.TryGetProperty("role", out var rolElement) &&
            rolElement.ValueKind == JsonValueKind.String)
        {
            // El propio JWT de Supabase ya trae un claim "role" con el rol de Postgres
            // ("authenticated", para RLS) que .NET mapea automáticamente a
            // ClaimTypes.Role antes de que este handler corra. Sin quitarlo primero,
            // el identity queda con dos claims ClaimTypes.Role y FindFirst devuelve
            // "authenticated" en vez del rol de negocio real.
            foreach (var rolExistente in identity.FindAll(ClaimTypes.Role).ToList())
            {
                identity.RemoveClaim(rolExistente);
            }

            identity.AddClaim(new Claim(ClaimTypes.Role, rolElement.GetString()!));
        }

        return Task.CompletedTask;
    }
}
