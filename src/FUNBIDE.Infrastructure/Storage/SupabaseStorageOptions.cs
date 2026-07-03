namespace FUNBIDE.Infrastructure.Storage;

/// <summary>
/// Opciones del bucket de Supabase Storage donde se guardan las fotos de perfil del
/// personal. Las credenciales (URL del proyecto y service role key) se reutilizan de
/// <see cref="Security.SupabaseAdminOptions"/> para no duplicar el mismo secreto en
/// dos secciones de configuración distintas.
/// </summary>
public sealed class SupabaseStorageOptions
{
    public const string SeccionConfiguracion = "Supabase:Storage";

    /// <summary>Bucket de Supabase Storage donde se guardan las fotos de perfil.</summary>
    public string BucketFotosPerfil { get; init; } = "fotos-perfil";

    /// <summary>Bucket PRIVADO donde se guardan las fotos de cédula de pacientes.</summary>
    public string BucketFotosCedulas { get; init; } = "fotos-cedulas";
}
