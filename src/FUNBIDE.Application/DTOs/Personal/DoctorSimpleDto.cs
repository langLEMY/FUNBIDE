using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Personal;

public sealed record DoctorSimpleDto(Guid Id, string NombreCompleto, EspecialidadMedica? Especialidad);
