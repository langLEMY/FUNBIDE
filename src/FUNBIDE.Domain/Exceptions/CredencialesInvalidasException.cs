namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// El correo no existe, la contraseña no coincide, o el usuario está inactivo, en el
/// modo de autenticación local (Auth:Provider=Local). Maps to HTTP 401.
/// </summary>
public sealed class CredencialesInvalidasException : DomainException
{
    public CredencialesInvalidasException()
        : base("Correo o contraseña incorrectos.")
    {
    }
}
