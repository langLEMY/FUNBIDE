namespace FUNBIDE.Application.DTOs.Dashboard;

public sealed record ResumenDiarioDto(DateOnly Fecha, int PacientesAtendidos, decimal DineroMovido);
