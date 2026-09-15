using System.Security.Cryptography;

namespace FUNBIDE.Launcher.Actualizacion;

public static class VerificadorHash
{
    /// <summary>
    /// Compara el SHA256 de <paramref name="contenido"/> contra el hash esperado. Acepta
    /// el hash esperado en mayúsculas/minúsculas mezcladas y con espacios o saltos de
    /// línea alrededor — el formato típico de un archivo .sha256 (Get-FileHash,
    /// sha256sum) a veces trae el nombre del archivo después del hash, separado por
    /// espacio; acá solo se usa el primer token.
    /// </summary>
    public static bool Coincide(byte[] contenido, string hashEsperado)
    {
        var hashCalculado = Convert.ToHexString(SHA256.HashData(contenido));
        var primerToken = hashEsperado
            .Trim()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;

        return string.Equals(hashCalculado, primerToken, StringComparison.OrdinalIgnoreCase);
    }
}
