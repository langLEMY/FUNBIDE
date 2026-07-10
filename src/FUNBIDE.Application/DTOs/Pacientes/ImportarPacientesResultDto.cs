namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record ImportarPacientesResultDto(
    int TotalFilas,
    int Creados,
    int Actualizados,
    int Omitidos,
    int IdentificacionesAjustadas,
    IReadOnlyList<string> Omisiones);
