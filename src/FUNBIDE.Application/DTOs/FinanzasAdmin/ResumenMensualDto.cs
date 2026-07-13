namespace FUNBIDE.Application.DTOs.FinanzasAdmin;

/// <summary>Una fila del gráfico de ganancias anuales: <c>Mes</c> va de 1 (enero) a 12 (diciembre).</summary>
public sealed record ResumenMensualDto(int Mes, decimal Ingresos, decimal Gastos, decimal Ganancia);
