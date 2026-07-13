using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.FinanzasAdmin;

/// <summary>
/// Une <see cref="Domain.Entities.Cobro"/> y <see cref="Domain.Entities.MovimientoFinanciero"/> en una sola
/// fila para el panel de Finanzas de Admin. <c>Origen</c> distingue de cuál de las dos tablas viene
/// ("Cobro" siempre es Ingreso; "Manual" puede ser Ingreso o Egreso, incluidos los gastos de Admin).
/// </summary>
public sealed record MovimientoImportanteDto(
    Guid Id,
    DateTimeOffset Fecha,
    string Origen,
    TipoMovimientoFinanciero Tipo,
    string Concepto,
    decimal Monto,
    string? PacienteNombre);
