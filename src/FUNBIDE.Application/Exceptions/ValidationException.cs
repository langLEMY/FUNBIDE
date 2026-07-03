using FluentValidation.Results;

namespace FUNBIDE.Application.Exceptions;

/// <summary>
/// Agrega los errores de un <see cref="FluentValidation.IValidator{T}"/> en una única
/// excepción que la API traduce a HTTP 400 con el detalle campo a campo.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures) : base("Se encontraron errores de validación.")
    {
        Errores = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }
}
