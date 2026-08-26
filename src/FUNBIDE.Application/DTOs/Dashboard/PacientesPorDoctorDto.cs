namespace FUNBIDE.Application.DTOs.Dashboard;

/// <summary>
/// Cuántos pacientes atendió cada doctor activo, para la tarjeta "Pacientes por doctor"
/// del Dashboard de Admin. Histórico acumulado (no acotado a un período): <see cref="Domain.Entities.Cita"/>
/// no guarda cuándo se completó, así que no hay una fecha confiable para filtrar por mes.
/// </summary>
public sealed record PacientesPorDoctorDto(
    Guid DoctorId,
    string NombreCompleto,
    string? Especialidad,
    int CitasCompletadas,
    int PacientesDistintos);
