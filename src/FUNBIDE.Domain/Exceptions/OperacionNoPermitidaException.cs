namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a caller attempts an operation it is not entitled to perform: mutar un
/// recurso append-only (historial clínico, auditoría) o actuar sobre un recurso que
/// pertenece a otro usuario (p. ej. la cita de otro doctor). Maps to HTTP 403.
/// </summary>
public sealed class OperacionNoPermitidaException : DomainException
{
    public OperacionNoPermitidaException(string mensaje) : base(mensaje)
    {
    }

    public static OperacionNoPermitidaException RecursoSoloLecturaEInsercion(string recurso) =>
        new($"El recurso '{recurso}' es de solo lectura e inserción. No se permiten actualizaciones ni eliminaciones.");
}
