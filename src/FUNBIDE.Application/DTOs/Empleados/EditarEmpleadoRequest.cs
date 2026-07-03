namespace FUNBIDE.Application.DTOs.Empleados;

public sealed record EditarEmpleadoRequest(Guid EmpleadoId, string NombreCompleto, string? Cargo);
