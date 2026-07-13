namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when scheduling a cita would overlap another cita already programmed for the
/// same doctor.
/// </summary>
public sealed class HorarioNoDisponibleException : DomainException
{
    public HorarioNoDisponibleException(Guid doctorId, DateTimeOffset inicio, DateTimeOffset fin)
        : base($"El doctor '{doctorId}' ya tiene una cita programada entre {inicio:g} y {fin:g}.")
    {
        DoctorId = doctorId;
        Inicio = inicio;
        Fin = fin;
    }

    public Guid DoctorId { get; }
    public DateTimeOffset Inicio { get; }
    public DateTimeOffset Fin { get; }
}
