using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Finanzas;

public sealed record MovimientoFinancieroDto(
    Guid Id,
    TipoMovimientoFinanciero Tipo,
    decimal Monto,
    string Concepto,
    Guid? CitaId,
    Guid? TurnoCajaId,
    DateTimeOffset RegistradoEn);
