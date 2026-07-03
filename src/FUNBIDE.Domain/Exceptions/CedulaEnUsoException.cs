namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a new or updated patient's cédula collides with another patient's
/// ("pacientes.documento_identidad" es único). Maps to HTTP 409.
/// </summary>
public sealed class CedulaEnUsoException : DomainException
{
    public CedulaEnUsoException(string cedula)
        : base($"La cédula '{cedula}' ya está registrada para otro paciente.")
    {
    }
}
