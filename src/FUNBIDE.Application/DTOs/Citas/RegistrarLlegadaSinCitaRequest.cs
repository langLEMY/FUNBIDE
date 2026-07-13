namespace FUNBIDE.Application.DTOs.Citas;

public sealed record RegistrarLlegadaSinCitaRequest(Guid PacienteId, Guid DoctorId, string Motivo);
