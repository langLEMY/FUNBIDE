namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Opciones de validación del JWT emitido por Supabase Auth (GoTrue).
/// Se enlazan desde la sección "Supabase:Jwt" de la configuración. No incluye un JWT
/// secret: las claves de firma se descubren dinámicamente vía JWKS
/// (ver <see cref="JwtAuthenticationExtensions"/>).
/// </summary>
public sealed class JwtSupabaseOptions
{
    public const string SeccionConfiguracion = "Supabase:Jwt";

    /// <summary>URL del proyecto Supabase, p. ej. https://xyzcompany.supabase.co</summary>
    public required string ProjectUrl { get; init; }

    public string ValidAudience { get; init; } = "authenticated";
}
