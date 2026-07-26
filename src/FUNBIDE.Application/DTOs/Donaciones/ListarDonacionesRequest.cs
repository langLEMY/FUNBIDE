namespace FUNBIDE.Application.DTOs.Donaciones;

public sealed record ListarDonacionesRequest(DateTimeOffset Desde, DateTimeOffset Hasta);
