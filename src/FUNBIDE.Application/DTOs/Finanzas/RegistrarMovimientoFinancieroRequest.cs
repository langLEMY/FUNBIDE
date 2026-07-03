using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Finanzas;

public sealed record RegistrarMovimientoFinancieroRequest(
    TipoMovimientoFinanciero Tipo, decimal Monto, string Concepto, Guid? CitaId);
