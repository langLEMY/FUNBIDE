using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Citas;

/// <summary>
/// Variante de <see cref="CitaDto"/> enriquecida con nombres (para Agenda/Recepción, que
/// listan citas de todos los doctores y no pueden asumir que el cliente ya tiene el
/// paciente/doctor cargado como sí puede el dashboard del Doctor, que solo ve las suyas).
/// </summary>
public sealed record CitaAgendaDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNombre,
    Guid DoctorId,
    string DoctorNombre,
    string Motivo,
    EstadoCita Estado,
    DateTimeOffset? Inicio,
    DateTimeOffset? Fin,
    decimal DeudaPendiente);
