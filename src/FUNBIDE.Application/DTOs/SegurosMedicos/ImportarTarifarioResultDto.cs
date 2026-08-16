namespace FUNBIDE.Application.DTOs.SegurosMedicos;

public sealed record ImportarTarifarioResultDto(
    int TotalFilas, int Creados, int Actualizados, int Omitidos, IReadOnlyList<string> Omisiones);
