using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.FinanzasAdmin;

public interface IObtenerResumenPorPeriodoUseCase : IUseCase<ObtenerResumenPorPeriodoRequest, IReadOnlyList<ResumenPeriodoDto>>
{
}

/// <summary>
/// Hermano de <see cref="ObtenerResumenAnualUseCase"/> con granularidad diaria o semanal en
/// vez de mensual, para el selector de "tendencia" del gráfico de Finanzas — mismo cálculo
/// (ingresos/gastos/ganancia/fondo a partir de Cobro y MovimientoFinanciero en crudo, nunca
/// de ResumenDiario, que no distingue ingreso de gasto), agrupado por día o por semana en vez
/// de por mes. Zero-rellena cada período del rango aunque no tenga movimientos, igual que el
/// anual devuelve los 12 meses siempre — así el gráfico no tiene huecos.
/// </summary>
public sealed class ObtenerResumenPorPeriodoUseCase(
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository) : IObtenerResumenPorPeriodoUseCase
{
    public async Task<IReadOnlyList<ResumenPeriodoDto>> EjecutarAsync(
        ObtenerResumenPorPeriodoRequest request, CancellationToken cancellationToken)
    {
        var cobros = await cobroRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);

        var ingresosPorPeriodo = new Dictionary<DateOnly, decimal>();
        var gastosPorPeriodo = new Dictionary<DateOnly, decimal>();
        var fondoPorPeriodo = new Dictionary<DateOnly, decimal>();

        DateOnly ClavePeriodo(DateTimeOffset fecha)
        {
            var dia = DateOnly.FromDateTime(fecha.UtcDateTime);
            return request.Granularidad == GranularidadResumen.Semanal ? InicioDeSemana(dia) : dia;
        }

        static void Sumar(Dictionary<DateOnly, decimal> mapa, DateOnly clave, decimal monto) =>
            mapa[clave] = mapa.GetValueOrDefault(clave) + monto;

        foreach (var cobro in cobros)
        {
            var clave = ClavePeriodo(cobro.RegistradoEn);
            Sumar(ingresosPorPeriodo, clave, cobro.MontoPagado);

            // Ver ObtenerResumenAnualUseCase: el fondo es un desglose de Ingresos, no un
            // ingreso aparte, así que no se vuelve a sumar en ingresosPorPeriodo acá.
            if (cobro.MontoFondo is > 0)
            {
                Sumar(fondoPorPeriodo, clave, cobro.MontoFondo.Value);
            }
        }

        foreach (var movimiento in movimientos)
        {
            var clave = ClavePeriodo(movimiento.RegistradoEn);
            if (movimiento.Tipo == TipoMovimientoFinanciero.Ingreso)
            {
                Sumar(ingresosPorPeriodo, clave, movimiento.Monto);
                if (GananciaDeLaFundacionHelper.EsGananciaDeLaFundacion(movimiento.Concepto))
                {
                    Sumar(fondoPorPeriodo, clave, movimiento.Monto);
                }
            }
            else
            {
                Sumar(gastosPorPeriodo, clave, movimiento.Monto);
            }
        }

        return EnumerarPeriodos(request.Desde, request.Hasta, request.Granularidad)
            .Select(periodo =>
            {
                var ingresos = ingresosPorPeriodo.GetValueOrDefault(periodo);
                var gastos = gastosPorPeriodo.GetValueOrDefault(periodo);
                return new ResumenPeriodoDto(periodo, ingresos, gastos, ingresos - gastos, fondoPorPeriodo.GetValueOrDefault(periodo));
            })
            .ToList();
    }

    /// <summary>Lunes como inicio de semana (ISO 8601), igual que el resto de la app.</summary>
    private static DateOnly InicioDeSemana(DateOnly fecha)
    {
        var diasDesdeElLunes = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.AddDays(-diasDesdeElLunes);
    }

    private static List<DateOnly> EnumerarPeriodos(DateTimeOffset desde, DateTimeOffset hasta, GranularidadResumen granularidad)
    {
        var inicio = DateOnly.FromDateTime(desde.UtcDateTime);
        // hasta es exclusivo, igual que el resto de los rangos de fecha de la app (ver
        // ICobroRepository.ObtenerPorRangoAsync).
        var fin = DateOnly.FromDateTime(hasta.UtcDateTime.AddDays(-1));

        var periodos = new List<DateOnly>();

        if (granularidad == GranularidadResumen.Semanal)
        {
            var cursor = InicioDeSemana(inicio);
            var finSemana = InicioDeSemana(fin);
            while (cursor <= finSemana)
            {
                periodos.Add(cursor);
                cursor = cursor.AddDays(7);
            }
            return periodos;
        }

        for (var cursor = inicio; cursor <= fin; cursor = cursor.AddDays(1))
        {
            periodos.Add(cursor);
        }
        return periodos;
    }
}
