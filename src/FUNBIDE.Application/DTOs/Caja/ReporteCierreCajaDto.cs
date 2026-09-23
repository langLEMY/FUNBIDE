using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Caja;

/// <summary>Una línea de egreso o ingreso manual dentro del reporte de cierre.</summary>
public sealed record MovimientoReporteCierreDto(string Concepto, decimal Monto, DateTimeOffset RegistradoEn);

/// <summary>
/// Detalle imprimible del arqueo de un turno ya cerrado (o del que se está cerrando):
/// desglose por método de pago y el detalle de cada egreso/ingreso manual del turno, para
/// que la cajera pueda dejar constancia impresa de lo que hizo en el día, no solo el total.
/// </summary>
public sealed record ReporteCierreCajaDto(
    Guid TurnoId,
    string UsuarioAperturaNombre,
    string? UsuarioCierreNombre,
    decimal FondoInicial,
    DateTimeOffset AbiertoEn,
    DateTimeOffset? CerradoEn,
    decimal MontoFinalContado,
    decimal MontoEsperado,
    decimal Diferencia,
    string? Notas,
    int CantidadCobros,
    decimal TotalFacturado,
    IReadOnlyDictionary<MetodoPago, decimal> TotalesPorMetodoPago,
    IReadOnlyList<MovimientoReporteCierreDto> Egresos,
    IReadOnlyList<MovimientoReporteCierreDto> IngresosManuales);
