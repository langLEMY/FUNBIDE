using System.Text.Json;

namespace FUNBIDE.Domain.ValueObjects;

/// <summary>
/// Envoltorio inmutable sobre un documento JSON válido. Se usa para el contenido de
/// historiales clínicos y payloads de auditoría, que se persisten como columnas JSONB.
/// Solo depende de System.Text.Json (BCL), no de un proveedor de base de datos concreto.
/// </summary>
public sealed record DocumentoJson
{
    public string Valor { get; }

    private DocumentoJson(string valor) => Valor = valor;

    public static DocumentoJson Crear(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("El documento JSON no puede estar vacío.", nameof(json));
        }

        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("El contenido no es un JSON válido.", nameof(json), ex);
        }

        return new DocumentoJson(json);
    }

    public override string ToString() => Valor;
}
