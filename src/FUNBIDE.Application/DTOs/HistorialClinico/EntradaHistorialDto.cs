namespace FUNBIDE.Application.DTOs.HistorialClinico;

public sealed record EntradaHistorialDto(
    Guid Id,
    Guid PacienteId,
    Guid DoctorId,
    Guid? CitaId,
    string Contenido,
    DateTimeOffset RegistradoEn);
