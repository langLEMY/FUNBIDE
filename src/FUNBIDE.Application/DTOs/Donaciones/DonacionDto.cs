namespace FUNBIDE.Application.DTOs.Donaciones;

public sealed record DonacionDto(
    Guid Id,
    string DonanteNombre,
    string? DonanteContacto,
    decimal Monto,
    string Concepto,
    DateTimeOffset RegistradoEn);
