using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Caja;

/// <summary>
/// Turno de caja para la vista de supervisión de Admin: a diferencia de
/// <see cref="TurnoCajaDto"/>, trae los nombres de quién abrió/cerró en vez de solo el
/// id, para no obligar al frontend a resolverlos por separado.
/// </summary>
public sealed record TurnoCajaAdminDto(
    Guid Id,
    string UsuarioAperturaNombre,
    decimal MontoInicial,
    DateTimeOffset AbiertoEn,
    EstadoTurnoCaja Estado,
    string? UsuarioCierreNombre,
    decimal? MontoFinalContado,
    decimal? MontoEsperado,
    decimal? Diferencia,
    string? Notas,
    DateTimeOffset? CerradoEn);
