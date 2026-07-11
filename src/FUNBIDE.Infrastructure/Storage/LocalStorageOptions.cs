namespace FUNBIDE.Infrastructure.Storage;

/// <summary>
/// Opciones del almacenamiento local de archivos (modo Auth:Provider=Local, sin
/// Supabase Storage). Se enlaza desde la sección "Storage:Local".
/// </summary>
public sealed class LocalStorageOptions
{
    public const string SeccionConfiguracion = "Storage:Local";

    /// <summary>Carpeta raíz donde se guardan los archivos subidos. Se crea si no existe.</summary>
    public string DirectorioBase { get; init; } = "almacenamiento-local";

    /// <summary>
    /// Clave de firma HMAC en base64 (mínimo 32 bytes decodificados) para las URLs
    /// firmadas del almacenamiento local — separada de <see cref="Security.LocalJwtOptions.SigningKeyBase64"/>
    /// a propósito: son dos garantías distintas (identidad vs. acceso a un archivo puntual),
    /// no hay razón para que compartan clave. Generarla una vez por instalación.
    /// </summary>
    public required string FirmaKeyBase64 { get; init; }
}
