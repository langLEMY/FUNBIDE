namespace FUNBIDE.Domain.Exceptions;

/// <summary>Raised when registering a Cobro for a cita that already has one, to prevent billing the same visit twice.</summary>
public sealed class CitaYaCobradaException : DomainException
{
    public CitaYaCobradaException(Guid citaId)
        : base($"La cita '{citaId}' ya tiene un cobro registrado.")
    {
        CitaId = citaId;
    }

    public Guid CitaId { get; }
}
