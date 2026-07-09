using Microsoft.AspNetCore.Http;

namespace FUNBIDE.API.Extensions;

/// <summary>
/// Validación compartida por los endpoints de importación masiva desde Excel
/// (pacientes, personal).
/// </summary>
public static class ArchivoExcelExtensions
{
    private const long TamanoMaximoBytes = 20 * 1024 * 1024;
    private static readonly string[] ExtensionesPermitidas = [".xls", ".xlsx"];

    public static bool EsExcelValido(this IFormFile? archivo, out string? error)
    {
        if (archivo is null || archivo.Length == 0)
        {
            error = "Debes adjuntar un archivo de Excel.";
            return false;
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            error = "El archivo no puede superar los 20 MB.";
            return false;
        }

        var extension = Path.GetExtension(archivo.FileName);
        if (!ExtensionesPermitidas.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            error = "El archivo debe ser .xls o .xlsx.";
            return false;
        }

        error = null;
        return true;
    }
}
