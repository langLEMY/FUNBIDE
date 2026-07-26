using System.Security.Cryptography;
using System.Text;
using FUNBIDE.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure.Storage;

/// <summary>
/// Implementa <see cref="ISupabaseStorageService"/> guardando los archivos en disco, para
/// el modo Auth:Provider=Local (instalación offline/USB, sin Supabase Storage). Las fotos
/// de perfil (bucket "público" en Supabase) y las de cédula (bucket privado + URL firmada)
/// se sirven ambas a través de <see cref="ArchivoLocalController"/> con una URL firmada por
/// HMAC — la única diferencia es la duración de la firma: prácticamente indefinida para
/// perfil, de 5 minutos para cédula, igual que <see cref="SupabaseStorageService"/>.
/// </summary>
public sealed class LocalDiskStorageService(IOptions<LocalStorageOptions> opciones) : ISupabaseStorageService
{
    private const string CarpetaFotosPerfil = "fotos-perfil";
    private const string CarpetaFotosCedulas = "fotos-cedulas";
    private static readonly TimeSpan DuracionFirmaCedula = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DuracionFirmaPerfil = TimeSpan.FromDays(365 * 50);

    public async Task<string> SubirFotoPerfilAsync(
        Guid usuarioId, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        var ruta = $"{CarpetaFotosPerfil}/{usuarioId}{ExtensionParaContentType(contentType)}";
        await GuardarAsync(ruta, contenido, cancellationToken);
        return GenerarUrlFirmada(ruta, DuracionFirmaPerfil);
    }

    public async Task<string> SubirFotoCedulaAsync(
        Guid pacienteId, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        var ruta = $"{CarpetaFotosCedulas}/{pacienteId}{ExtensionParaContentType(contentType)}";
        await GuardarAsync(ruta, contenido, cancellationToken);
        return ruta;
    }

    public Task<string> GenerarUrlFirmadaCedulaAsync(string rutaObjeto, CancellationToken cancellationToken) =>
        Task.FromResult(GenerarUrlFirmada(rutaObjeto, DuracionFirmaCedula));

    public Task<bool> VerificarConexionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // No hay red que probar en modo local: alcanza con confirmar que el directorio
            // base es accesible (existe o se puede crear) en este disco.
            Directory.CreateDirectory(opciones.Value.DirectorioBase);
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(false);
        }
    }

    private async Task GuardarAsync(string rutaRelativa, Stream contenido, CancellationToken cancellationToken)
    {
        var rutaAbsoluta = ArchivoLocalRutas.ResolverRutaSegura(opciones.Value.DirectorioBase, rutaRelativa)
            ?? throw new InvalidOperationException($"Ruta de almacenamiento inválida: '{rutaRelativa}'.");

        Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);

        await using var destino = new FileStream(rutaAbsoluta, FileMode.Create, FileAccess.Write, FileShare.None);
        await contenido.CopyToAsync(destino, cancellationToken);
    }

    private string GenerarUrlFirmada(string rutaObjeto, TimeSpan duracion)
    {
        var expiraUnix = DateTimeOffset.UtcNow.Add(duracion).ToUnixTimeSeconds();
        var firma = ArchivoLocalRutas.Firmar(rutaObjeto, expiraUnix, opciones.Value.FirmaKeyBase64);

        return $"/api/archivos-locales?ruta={Uri.EscapeDataString(rutaObjeto)}&expira={expiraUnix}&firma={Uri.EscapeDataString(firma)}";
    }

    private static string ExtensionParaContentType(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => throw new ArgumentException($"Tipo de contenido no soportado: '{contentType}'.", nameof(contentType)),
    };
}

/// <summary>
/// Firma HMAC y resolución de rutas compartidas entre <see cref="LocalDiskStorageService"/>
/// (escribe/firma) y <see cref="ArchivoLocalService"/> (verifica/lee) — deben calcular
/// exactamente la misma firma y resolver exactamente la misma ruta de disco.
/// </summary>
internal static class ArchivoLocalRutas
{
    private static readonly string[] CarpetasPermitidas = ["fotos-perfil", "fotos-cedulas"];

    public static string Firmar(string rutaObjeto, long expiraUnix, string firmaKeyBase64)
    {
        var clave = Convert.FromBase64String(firmaKeyBase64);
        var payload = Encoding.UTF8.GetBytes($"{rutaObjeto}|{expiraUnix}");
        var hash = HMACSHA256.HashData(clave, payload);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Devuelve la ruta absoluta en disco si <paramref name="rutaRelativa"/> cae dentro de
    /// una carpeta permitida y no se escapa de <paramref name="directorioBase"/> (path
    /// traversal vía "..", rutas absolutas, etc.); <c>null</c> si no es válida.
    /// </summary>
    public static string? ResolverRutaSegura(string directorioBase, string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa) ||
            !CarpetasPermitidas.Any(carpeta => rutaRelativa.StartsWith($"{carpeta}/", StringComparison.Ordinal)))
        {
            return null;
        }

        var raiz = Path.GetFullPath(directorioBase);
        var rutaAbsoluta = Path.GetFullPath(Path.Combine(raiz, rutaRelativa));

        if (!rutaAbsoluta.StartsWith(raiz + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            return null;
        }

        return rutaAbsoluta;
    }
}
