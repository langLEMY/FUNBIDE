namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new inventory item's código collides with an existing one
/// ("inventario_items.codigo" es único). Maps to HTTP 409.
/// </summary>
public sealed class CodigoInventarioEnUsoException : DomainException
{
    public CodigoInventarioEnUsoException(string codigo)
        : base($"El código '{codigo}' ya está registrado para otro ítem de inventario.")
    {
    }
}
