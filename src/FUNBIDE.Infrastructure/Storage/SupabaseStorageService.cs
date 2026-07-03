using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure.Storage;

/// <summary>
/// Sube fotos a Supabase Storage vía su REST API, usando el mismo service role key que
/// <see cref="SupabaseAdminService"/> (bypassa las políticas de RLS del bucket,
/// apropiado para una subida hecha por el backend en nombre del usuario, no
/// directamente desde el navegador).
/// </summary>
public sealed class SupabaseStorageService(
    HttpClient httpClient,
    IOptions<SupabaseAdminOptions> adminOptions,
    IOptions<SupabaseStorageOptions> storageOptions) : ISupabaseStorageService
{
    public Task<string> SubirFotoPerfilAsync(
        Guid usuarioId, Stream contenido, string contentType, CancellationToken cancellationToken) =>
        SubirYObtenerUrlPublicaAsync(storageOptions.Value.BucketFotosPerfil, usuarioId.ToString(), contenido, contentType, cancellationToken);

    public async Task<string> SubirFotoCedulaAsync(
        Guid pacienteId, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        var bucket = storageOptions.Value.BucketFotosCedulas;
        var rutaObjeto = $"{bucket}/{pacienteId}";

        await SubirAsync(rutaObjeto, contenido, contentType, cancellationToken);

        // Solo la ruta: el bucket es privado, no existe una URL pública válida que guardar.
        return rutaObjeto;
    }

    public async Task<string> GenerarUrlFirmadaCedulaAsync(string rutaObjeto, CancellationToken cancellationToken)
    {
        var payload = new UrlFirmadaPayload(ExpiresIn: 300);

        using var respuesta = await httpClient.PostAsJsonAsync($"object/sign/{rutaObjeto}", payload, cancellationToken);

        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"No se pudo generar la URL firmada de la foto de cédula (HTTP {(int)respuesta.StatusCode}): {detalle}");
        }

        var resultado = await respuesta.Content.ReadFromJsonAsync<UrlFirmadaRespuesta>(cancellationToken)
            ?? throw new InvalidOperationException("Supabase Storage no devolvió una URL firmada.");

        // La respuesta trae una ruta relativa a /storage/v1 (ej. "/object/sign/bucket/obj?token=..."),
        // no una URL completa ni relativa a la raíz del proyecto — verificado contra la API real.
        var projectUrl = adminOptions.Value.ProjectUrl.TrimEnd('/');
        return $"{projectUrl}/storage/v1{resultado.SignedURL}";
    }

    private async Task<string> SubirYObtenerUrlPublicaAsync(
        string bucket, string nombreObjeto, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        var rutaObjeto = $"{bucket}/{nombreObjeto}";
        await SubirAsync(rutaObjeto, contenido, contentType, cancellationToken);

        var projectUrl = adminOptions.Value.ProjectUrl.TrimEnd('/');
        return $"{projectUrl}/storage/v1/object/public/{rutaObjeto}";
    }

    private async Task SubirAsync(string rutaObjeto, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        using var contenidoHttp = new StreamContent(contenido);
        contenidoHttp.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"object/{rutaObjeto}")
        {
            Content = contenidoHttp
        };
        solicitud.Headers.Add("x-upsert", "true");

        using var respuesta = await httpClient.SendAsync(solicitud, cancellationToken);

        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"No se pudo subir el archivo a Supabase Storage (HTTP {(int)respuesta.StatusCode}): {detalle}");
        }
    }

    private sealed record UrlFirmadaPayload([property: JsonPropertyName("expiresIn")] int ExpiresIn);

    private sealed record UrlFirmadaRespuesta([property: JsonPropertyName("signedURL")] string SignedURL);
}
