namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new aseguradora's nombre collides with an existing one
/// ("seguros_medicos.nombre" es único). Maps to HTTP 409.
/// </summary>
public sealed class NombreSeguroMedicoEnUsoException : DomainException
{
    public NombreSeguroMedicoEnUsoException(string nombre)
        : base($"Ya existe una aseguradora registrada con el nombre '{nombre}'.")
    {
    }
}
