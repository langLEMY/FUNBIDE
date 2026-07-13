namespace FUNBIDE.Application.DTOs.Citas;

public sealed record ListarAgendaRequest(DateOnly? Fecha, Guid? DoctorId);
