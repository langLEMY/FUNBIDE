namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when registering a walk-in arrival for a patient that already has an active
/// (Programada or EnEspera) cita with the same doctor, to avoid duplicate entries in the
/// waiting room for the same visit.
/// </summary>
public sealed class PacienteConCitaActivaException : DomainException
{
    public PacienteConCitaActivaException(Guid pacienteId, Guid doctorId)
        : base($"El paciente '{pacienteId}' ya tiene una cita activa con el doctor '{doctorId}'.")
    {
        PacienteId = pacienteId;
        DoctorId = doctorId;
    }

    public Guid PacienteId { get; }
    public Guid DoctorId { get; }
}
