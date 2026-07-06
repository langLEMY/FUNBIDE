using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record EditarPacienteRequest(
    Guid PacienteId,
    string Nombre,
    string Apellido,
    string Cedula,
    string? Telefono,
    int? Edad,
    string? Condicion,
    EstadoPaciente Estado);
