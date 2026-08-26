namespace FUNBIDE.Application.DTOs.FinanzasAdmin;

/// <summary>
/// Una fila del gráfico de ganancias anuales: <c>Mes</c> va de 1 (enero) a 12 (diciembre).
/// <see cref="FondoGanancias"/> es el subconjunto de <see cref="Ingresos"/> que corresponde
/// al fondo interno de la fundación (ver <see cref="Domain.Entities.Cobro.MontoFondo"/>) —
/// no es un ingreso aparte, sino un desglose de cuánto de la ganancia neta viene
/// específicamente de ese excedente negociado con las aseguradoras.
/// </summary>
public sealed record ResumenMensualDto(int Mes, decimal Ingresos, decimal Gastos, decimal Ganancia, decimal FondoGanancias);
