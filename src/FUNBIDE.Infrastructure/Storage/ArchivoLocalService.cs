using System.Security.Cryptography;
using System.Text;
using FUNBIDE.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure.Storage;

/// <summary>
/// Verifica la firma HMAC generada por <see cref="LocalDiskStorageService"/> y sirve el
/// archivo del almacenamiento local — el equivalente local a "descargar una URL firmada de
/// Supabase Storage". Solo se registra en DI cuando Auth:Provider=Local.
/// </summary>
public sealed class ArchivoLocalService(IOptions<LocalStorageOptions> opciones) : IArchivoLocalService
{
    public Task<ArchivoLocalResultado?> AbrirSiFirmaValidaAsync(
        string ruta, long expiraUnix, string firma, CancellationToken cancellationToken)
    {
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiraUnix)
        {
            return Task.FromResult<ArchivoLocalResultado?>(null);
        }

        var firmaEsperada = ArchivoLocalRutas.Firmar(ruta, expiraUnix, opciones.Value.FirmaKeyBase64);
        if (!FirmasCoinciden(firma, firmaEsperada))
        {
            return Task.FromResult<ArchivoLocalResultado?>(null);
        }

        var rutaAbsoluta = ArchivoLocalRutas.ResolverRutaSegura(opciones.Value.DirectorioBase, ruta);
        if (rutaAbsoluta is null || !File.Exists(rutaAbsoluta))
        {
            return Task.FromResult<ArchivoLocalResultado?>(null);
        }

        Stream contenido = new FileStream(
            rutaAbsoluta, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        return Task.FromResult<ArchivoLocalResultado?>(new ArchivoLocalResultado(contenido, ContentTypeParaRuta(ruta)));
    }

    private static bool FirmasCoinciden(string recibida, string esperada)
    {
        var bytesRecibidos = Encoding.UTF8.GetBytes(recibida);
        var bytesEsperados = Encoding.UTF8.GetBytes(esperada);
        return bytesRecibidos.Length == bytesEsperados.Length &&
            CryptographicOperations.FixedTimeEquals(bytesRecibidos, bytesEsperados);
    }

    private static string ContentTypeParaRuta(string ruta) => Path.GetExtension(ruta).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "application/octet-stream",
    };
}

/// <summary>
/// Se registra en modo Auth:Provider=Supabase, donde el almacenamiento sigue siendo
/// Supabase Storage y <c>ArchivoLocalController</c> nunca debería recibir tráfico real.
/// Sin esta implementación de relleno, la resolución de DI fallaría porque nada más
/// implementa <see cref="IArchivoLocalService"/> en ese modo.
/// </summary>
public sealed class ArchivoLocalNoDisponibleService : IArchivoLocalService
{
    public Task<ArchivoLocalResultado?> AbrirSiFirmaValidaAsync(
        string ruta, long expiraUnix, string firma, CancellationToken cancellationToken) =>
        Task.FromResult<ArchivoLocalResultado?>(null);
}
