using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.FinanzasAdmin;

public interface IObtenerResumenAnualUseCase : IUseCase<int, IReadOnlyList<ResumenMensualDto>>
{
}

/// <summary>
/// Ganancia neta mes a mes de un año calendario, para el gráfico de "ganancias anuales"
/// de Admin. Se calcula en el momento a partir de <see cref="Domain.Entities.Cobro"/> y
/// <see cref="Domain.Entities.MovimientoFinanciero"/> (no de <see cref="Domain.Entities.ResumenDiario"/>,
/// que solo guarda el neto y no distingue ingreso de gasto).
/// </summary>
public sealed class ObtenerResumenAnualUseCase(
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository) : IObtenerResumenAnualUseCase
{
    public async Task<IReadOnlyList<ResumenMensualDto>> EjecutarAsync(int anio, CancellationToken cancellationToken)
    {
        var desde = new DateTimeOffset(new DateTime(anio, 1, 1), TimeSpan.Zero);
        var hasta = desde.AddYears(1);

        var cobros = await cobroRepository.ObtenerPorRangoAsync(desde, hasta, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorRangoAsync(desde, hasta, cancellationToken);

        var ingresosPorMes = new decimal[12];
        var gastosPorMes = new decimal[12];

        foreach (var cobro in cobros)
        {
            ingresosPorMes[cobro.RegistradoEn.Month - 1] += cobro.MontoPagado;
        }

        foreach (var movimiento in movimientos)
        {
            if (movimiento.Tipo == TipoMovimientoFinanciero.Ingreso)
            {
                ingresosPorMes[movimiento.RegistradoEn.Month - 1] += movimiento.Monto;
            }
            else
            {
                gastosPorMes[movimiento.RegistradoEn.Month - 1] += movimiento.Monto;
            }
        }

        return Enumerable.Range(1, 12)
            .Select(mes => new ResumenMensualDto(
                mes, ingresosPorMes[mes - 1], gastosPorMes[mes - 1], ingresosPorMes[mes - 1] - gastosPorMes[mes - 1]))
            .ToList();
    }
}
