namespace FUNBIDE.Application.Common.Interfaces;

public sealed record ArchivoLocalResultado(Stream Contenido, string ContentType);

/// <summary>
/// Sirve un archivo del almacenamiento local validando la firma/expiración generada
/// por el almacenamiento local (ver <c>LocalDiskStorageService.GenerarUrlFirmadaCedulaAsync</c>)
/// — el equivalente local a una URL firmada de Supabase Storage. La "autenticación"
/// viaja en la propia URL (query string), no en un header, para poder consumirse
/// desde un <c>&lt;img src&gt;</c> directo.
/// </summary>
public interface IArchivoLocalService
{
    /// <summary>Devuelve null si la firma no coincide, si expiró, o si el archivo no existe.</summary>
    Task<ArchivoLocalResultado?> AbrirSiFirmaValidaAsync(
        string ruta, long expiraUnix, string firma, CancellationToken cancellationToken);
}
