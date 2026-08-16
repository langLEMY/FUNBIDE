namespace FUNBIDE.Application.DTOs.Sistema;

public sealed record EstadoDiscoDto(string Unidad, double EspacioLibreGb, double EspacioTotalGb, bool EspacioBajo);
