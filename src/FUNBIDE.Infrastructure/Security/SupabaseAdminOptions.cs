namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Credenciales para invocar la GoTrue Admin API de Supabase (crear/actualizar/revocar
/// usuarios). El "service role key" tiene privilegios totales sobre el proyecto de
/// Supabase: nunca debe exponerse al frontend ni registrarse en logs, y en producción
/// debe provenir de una variable de entorno o de un vault, igual que el JWT secret.
/// </summary>
public sealed class SupabaseAdminOptions
{
    public const string SeccionConfiguracion = "Supabase:Admin";

    /// <summary>URL del proyecto Supabase, p. ej. https://xyzcompany.supabase.co</summary>
    public required string ProjectUrl { get; init; }

    /// <summary>Service role key (Settings → API → service_role). Privilegios de administrador.</summary>
    public required string ServiceRoleKey { get; init; }
}
