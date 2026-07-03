using System.Security.Cryptography;

namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Cifra un archivo de backup con AES-256-CBC. El IV se genera por archivo y se
/// antepone al flujo cifrado (formato estándar: [16 bytes IV][ciphertext]) para que
/// el descifrado no dependa de almacenar el IV por separado.
/// </summary>
public sealed class AesBackupEncryptor
{
    public async Task<string> CifrarArchivoAsync(string rutaOrigen, byte[] claveAes256, CancellationToken cancellationToken)
    {
        if (claveAes256.Length != 32)
        {
            throw new ArgumentException("La clave AES debe tener 256 bits (32 bytes).", nameof(claveAes256));
        }

        var rutaDestino = rutaOrigen + ".enc";

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = claveAes256;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        await using var origen = File.OpenRead(rutaOrigen);
        await using var destino = File.Create(rutaDestino);

        await destino.WriteAsync(aes.IV, cancellationToken);

        await using var cryptoStream = new CryptoStream(destino, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await origen.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);

        return rutaDestino;
    }
}
