namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new or updated profile's username collides with another profile's
/// username ("usuarios.NombreUsuario" es único). Maps to HTTP 409.
/// </summary>
public sealed class NombreUsuarioEnUsoException : DomainException
{
    public NombreUsuarioEnUsoException(string nombreUsuario)
        : base($"El nombre de usuario '{nombreUsuario}' ya está en uso por otro perfil.")
    {
    }
}
