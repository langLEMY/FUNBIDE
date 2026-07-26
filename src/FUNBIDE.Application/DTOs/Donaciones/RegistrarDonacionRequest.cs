namespace FUNBIDE.Application.DTOs.Donaciones;

public sealed record RegistrarDonacionRequest(string DonanteNombre, string? DonanteContacto, decimal Monto, string Concepto);
