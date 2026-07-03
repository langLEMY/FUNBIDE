namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new or updated profile's email collides with another profile's
/// email ("usuarios.correo" es único). Maps to HTTP 409.
/// </summary>
public sealed class CorreoEnUsoException : DomainException
{
    public CorreoEnUsoException(string correo)
        : base($"El correo '{correo}' ya está en uso por otro perfil.")
    {
    }
}
