using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Citas;

public sealed record CitaDto(
    Guid Id,
    Guid PacienteId,
    Guid DoctorId,
    string Motivo,
    EstadoCita Estado,
    DateTimeOffset? Inicio,
    DateTimeOffset? Fin,
    string? NotasCierre);
