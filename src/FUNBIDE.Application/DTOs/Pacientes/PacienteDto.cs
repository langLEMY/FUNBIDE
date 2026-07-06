using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record PacienteDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string Cedula,
    string? Telefono,
    bool TieneFotoCedula,
    int? Edad,
    string? Condicion,
    EstadoPaciente Estado,
    DateOnly? UltimaVisita);
