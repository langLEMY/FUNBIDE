namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new servicio's código collides with an existing one
/// ("servicios.codigo" es único). Maps to HTTP 409.
/// </summary>
public sealed class CodigoServicioEnUsoException : DomainException
{
    public CodigoServicioEnUsoException(string codigo)
        : base($"El código '{codigo}' ya está registrado para otro servicio.")
    {
    }
}
