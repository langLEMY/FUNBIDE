namespace FUNBIDE.Application.DTOs.Citas;

public sealed record AgendarCitaRequest(Guid PacienteId, Guid DoctorId, string Motivo, DateTimeOffset Inicio, DateTimeOffset Fin);
