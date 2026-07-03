namespace FUNBIDE.Domain.ValueObjects;

/// <summary>
/// Documento de identidad de un paciente. Value Object inmutable con auto-validación.
/// </summary>
public sealed record DocumentoIdentidad
{
    public string Valor { get; }

    private DocumentoIdentidad(string valor) => Valor = valor;

    public static DocumentoIdentidad Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("El documento de identidad no puede estar vacío.", nameof(valor));
        }

        var normalizado = valor.Trim();
        if (normalizado.Length is < 5 or > 20)
        {
            throw new ArgumentException("El documento de identidad debe tener entre 5 y 20 caracteres.", nameof(valor));
        }

        return new DocumentoIdentidad(normalizado);
    }

    public override string ToString() => Valor;
}
