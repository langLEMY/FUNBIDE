using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.FinanzasAdmin;

public sealed record ObtenerResumenPorPeriodoRequest(DateTimeOffset Desde, DateTimeOffset Hasta, GranularidadResumen Granularidad);
