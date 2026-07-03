namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record PacienteDto(
    Guid Id, string Nombre, string Apellido, string Cedula, string? Telefono, bool TieneFotoCedula);
