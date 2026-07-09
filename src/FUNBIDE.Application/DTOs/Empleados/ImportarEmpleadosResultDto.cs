namespace FUNBIDE.Application.DTOs.Empleados;

public sealed record ImportarEmpleadosResultDto(int TotalFilas, int Creados, int Omitidos, IReadOnlyList<string> Omisiones);
