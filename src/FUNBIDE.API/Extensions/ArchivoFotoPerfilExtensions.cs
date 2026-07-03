using Microsoft.AspNetCore.Http;

namespace FUNBIDE.API.Extensions;

/// <summary>
/// Validación compartida por <c>PersonalController</c> y <c>MiPerfilController</c> para
/// los archivos subidos como foto de perfil.
/// </summary>
public static class ArchivoFotoPerfilExtensions
{
    private const long TamanoMaximoBytes = 5 * 1024 * 1024;

    public static bool EsFotoPerfilValida(this IFormFile? archivo, out string? error)
    {
        if (archivo is null || archivo.Length == 0)
        {
            error = "Debes adjuntar un archivo de imagen.";
            return false;
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            error = "La foto no puede superar los 5 MB.";
            return false;
        }

        if (string.IsNullOrEmpty(archivo.ContentType) ||
            !archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            error = "El archivo debe ser una imagen (JPEG, PNG, WEBP, etc.).";
            return false;
        }

        error = null;
        return true;
    }
}
