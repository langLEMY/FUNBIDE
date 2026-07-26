namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record ImportarInventarioResultDto(
    int TotalFilas,
    int Creados,
    int Actualizados,
    int Omitidos,
    IReadOnlyList<string> Omisiones);
