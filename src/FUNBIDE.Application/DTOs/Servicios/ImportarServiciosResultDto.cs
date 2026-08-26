namespace FUNBIDE.Application.DTOs.Servicios;

public sealed record ImportarServiciosResultDto(
    int TotalFilas, int Creados, int Actualizados, int Omitidos, IReadOnlyList<string> Omisiones);
