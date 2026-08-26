namespace FUNBIDE.Application.DTOs.Servicios;

public sealed record ServicioDto(
    Guid Id,
    string Codigo,
    string Nombre,
    decimal Precio1,
    decimal Precio2,
    decimal Precio3,
    string? Especialidad,
    bool Activo);
