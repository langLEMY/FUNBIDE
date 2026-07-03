namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record CrearPacienteRequest(string Nombre, string Apellido, string Cedula, string? Telefono);
