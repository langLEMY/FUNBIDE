using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FUNBIDE.Launcher.Actualizacion;

/// <summary>
/// Chequea/descarga actualizaciones desde GitHub Releases (repo langLEMY/FUNBIDE).
/// Nunca lanza desde <see cref="BuscarActualizacionAsync"/> — sin internet, con rate
/// limit de la API, o con un release mal formado, simplemente no hay actualización
/// (ver Program.cs: esto corre al arranque y jamás debe impedir que FUNBIDE abra,
/// pensando en las instalaciones "Local" offline/USB que nunca van a tener red).
/// </summary>
public sealed class ServicioActualizacion(HttpClient http)
{
    private const string UrlUltimoRelease = "https://api.github.com/repos/langLEMY/FUNBIDE/releases/latest";

    public async Task<InfoActualizacion?> BuscarActualizacionAsync(Version versionLocal, CancellationToken cancellationToken)
    {
        try
        {
            using var solicitud = new HttpRequestMessage(HttpMethod.Get, UrlUltimoRelease);
            // La API de GitHub devuelve 403 sin un User-Agent en el request.
            solicitud.Headers.UserAgent.ParseAdd("FUNBIDE-Launcher");
            solicitud.Headers.Accept.ParseAdd("application/vnd.github+json");

            using var respuesta = await http.SendAsync(solicitud, cancellationToken);
            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            var release = await respuesta.Content.ReadFromJsonAsync<ReleaseGitHub>(cancellationToken);
            if (release?.TagName is null || !ComparadorVersiones.HayVersionMasNueva(release.TagName, versionLocal))
            {
                return null;
            }

            var assetInstalador = release.Assets?.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (assetInstalador is null)
            {
                return null;
            }

            // El hash SHA256 se publica como asset aparte, junto al instalador, en cada
            // Release (ver scripts/generar-instalador.ps1) — sin él no hay forma de
            // verificar integridad, así que no se ofrece la actualización.
            var assetHash = release.Assets?.FirstOrDefault(a =>
                string.Equals(a.Name, assetInstalador.Name + ".sha256", StringComparison.OrdinalIgnoreCase));
            if (assetHash is null)
            {
                return null;
            }

            return new InfoActualizacion(
                release.TagName,
                new Uri(assetInstalador.BrowserDownloadUrl),
                new Uri(assetHash.BrowserDownloadUrl),
                assetInstalador.Name);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Descarga el instalador a un archivo temporal y lo verifica contra el hash
    /// publicado ANTES de devolver la ruta — quien llama nunca recibe un archivo sin
    /// verificar. Si el hash no coincide, borra lo descargado y lanza: quien llama debe
    /// avisar al usuario y no ejecutar nada (ver FormActualizacionDisponible).
    /// </summary>
    public async Task<string> DescargarInstaladorAsync(InfoActualizacion info, CancellationToken cancellationToken)
    {
        var hashEsperado = await http.GetStringAsync(info.UrlDescargaHash, cancellationToken);
        var contenido = await http.GetByteArrayAsync(info.UrlDescargaInstalador, cancellationToken);

        if (!VerificadorHash.Coincide(contenido, hashEsperado))
        {
            throw new InvalidOperationException(
                "El instalador descargado no coincide con el hash publicado — puede estar incompleto o alterado.");
        }

        var rutaDestino = Path.Combine(Path.GetTempPath(), info.NombreArchivo);
        await File.WriteAllBytesAsync(rutaDestino, contenido, cancellationToken);
        return rutaDestino;
    }

    private sealed record ReleaseGitHub(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("assets")] List<AssetGitHub>? Assets);

    private sealed record AssetGitHub(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl);
}
