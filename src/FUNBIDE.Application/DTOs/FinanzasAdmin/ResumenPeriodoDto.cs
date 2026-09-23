namespace FUNBIDE.Application.DTOs.FinanzasAdmin;

/// <summary>
/// Una fila del gráfico de tendencia diaria/semanal (ver <c>ObtenerResumenPorPeriodoUseCase</c>),
/// hermano de <see cref="ResumenMensualDto"/> pero con granularidad más fina. <c>Periodo</c> es
/// el primer día de ese punto — la fecha exacta para granularidad diaria, o el lunes de esa
/// semana (ISO 8601) para granularidad semanal.
/// </summary>
public sealed record ResumenPeriodoDto(DateOnly Periodo, decimal Ingresos, decimal Gastos, decimal Ganancia, decimal FondoGanancias);
