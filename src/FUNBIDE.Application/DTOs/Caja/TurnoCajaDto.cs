using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Caja;

public sealed record TurnoCajaDto(
    Guid Id,
    Guid UsuarioAperturaId,
    decimal MontoInicial,
    DateTimeOffset AbiertoEn,
    EstadoTurnoCaja Estado,
    Guid? UsuarioCierreId,
    decimal? MontoFinalContado,
    decimal? MontoEsperado,
    decimal? Diferencia,
    string? Notas,
    DateTimeOffset? CerradoEn);
