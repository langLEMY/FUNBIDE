using System.Security.Cryptography;

namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Cifra un archivo de backup con AES-256-CBC + HMAC-SHA256 (Encrypt-then-MAC), no CBC a
/// secas: CBC sin autenticar no detecta manipulación del archivo cifrado y abre un riesgo
/// teórico de padding oracle si esos bytes cifrados llegan a estar bajo control de un
/// atacante. Se eligió esta composición en vez de AES-GCM porque <see cref="AesGcm"/> en
/// .NET opera sobre buffers completos en memoria, y un dump de base de datos puede crecer
/// con los años — CBC+HMAC preserva el streaming actual (<see cref="CryptoStream"/> +
/// <c>CopyToAsync</c>) sin cargar el archivo entero en RAM.
///
/// Formato: [16 bytes IV][ciphertext][32 bytes HMAC-SHA256 sobre IV+ciphertext]. Las claves
/// de cifrado y de HMAC se derivan por separado con HKDF a partir de la misma clave maestra
/// para no reusar una única clave en dos algoritmos distintos.
/// </summary>
public sealed class AesBackupEncryptor
{
    private static readonly byte[] InfoClaveCifrado = "FUNBIDE-backup-aes-key"u8.ToArray();
    private static readonly byte[] InfoClaveHmac = "FUNBIDE-backup-hmac-key"u8.ToArray();

    public async Task<string> CifrarArchivoAsync(string rutaOrigen, byte[] claveAes256, CancellationToken cancellationToken)
    {
        if (claveAes256.Length != 32)
        {
            throw new ArgumentException("La clave AES debe tener 256 bits (32 bytes).", nameof(claveAes256));
        }

        var rutaDestino = rutaOrigen + ".enc";

        var claveCifrado = HKDF.DeriveKey(HashAlgorithmName.SHA256, claveAes256, 32, info: InfoClaveCifrado);
        var claveHmac = HKDF.DeriveKey(HashAlgorithmName.SHA256, claveAes256, 32, info: InfoClaveHmac);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = claveCifrado;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var hmac = new HMACSHA256(claveHmac);

        await using var origen = File.OpenRead(rutaOrigen);
        await using var destino = File.Create(rutaDestino);

        // El IV se escribe una sola vez, a través de cryptoStreamHmac: eso lo manda al
        // archivo (passthrough del transform de HMAC) y a la vez lo incluye en el cómputo
        // del HMAC — escribirlo también directo a `destino` lo duplicaría en el archivo.
        using (var cryptoStreamHmac = new CryptoStream(destino, hmac, CryptoStreamMode.Write, leaveOpen: true))
        {
            await cryptoStreamHmac.WriteAsync(aes.IV, cancellationToken);

            using (var cryptoStreamCifrado = new CryptoStream(cryptoStreamHmac, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
            {
                await origen.CopyToAsync(cryptoStreamCifrado, cancellationToken);
                await cryptoStreamCifrado.FlushFinalBlockAsync(cancellationToken);
            }
        }

        await destino.WriteAsync(hmac.Hash, cancellationToken);

        return rutaDestino;
    }
}
