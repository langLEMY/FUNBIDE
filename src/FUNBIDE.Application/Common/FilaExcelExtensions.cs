using System.Globalization;
using System.Text;

namespace FUNBIDE.Application.Common;

/// <summary>
/// Busca un valor de fila probando varios nombres de columna candidatos, ignorando
/// acentos/mayúsculas/puntuación — los reportes exportados desde distintos sistemas
/// nombran la misma columna de formas ligeramente distintas ("Identificación" vs
/// "Cédula" vs "Identificacion").
/// </summary>
public static class FilaExcelExtensions
{
    public static string? Buscar(this IReadOnlyDictionary<string, string?> fila, params string[] candidatos)
    {
        foreach (var candidato in candidatos)
        {
            if (fila.TryGetValue(candidato, out var valorExacto) && !string.IsNullOrWhiteSpace(valorExacto))
            {
                return valorExacto;
            }
        }

        var candidatosNormalizados = candidatos.Select(Normalizar).ToHashSet();
        foreach (var (clave, valor) in fila)
        {
            if (candidatosNormalizados.Contains(Normalizar(clave)) && !string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }
        }

        return null;
    }

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sinDiacriticos = new StringBuilder();
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sinDiacriticos.Append(c);
            }
        }

        return new string(sinDiacriticos.ToString().Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}
