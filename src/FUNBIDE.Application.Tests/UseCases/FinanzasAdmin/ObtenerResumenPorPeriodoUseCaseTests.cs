using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Application.UseCases.FinanzasAdmin;
using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.FinanzasAdmin;

public class ObtenerResumenPorPeriodoUseCaseTests
{
    private readonly ICobroRepository _cobroRepository = Substitute.For<ICobroRepository>();
    private readonly IMovimientoFinancieroRepository _movimientoRepository = Substitute.For<IMovimientoFinancieroRepository>();

    private ObtenerResumenPorPeriodoUseCase CrearCasoDeUso() => new(_cobroRepository, _movimientoRepository);

    // RegistradoEn es "protected init" (ver AppendOnlyEntity) — no hay forma pública de
    // fijarlo a una fecha concreta para el test, así que se pisa por reflexión. Es lo mismo
    // que hacer que el reloj del sistema estuviera en esa fecha al construir la entidad.
    private static void FijarRegistradoEn(AppendOnlyEntity entidad, DateTimeOffset fecha) =>
        typeof(AppendOnlyEntity).GetProperty(nameof(AppendOnlyEntity.RegistradoEn))!.SetValue(entidad, fecha);

    private static Cobro CrearCobro(DateTimeOffset fecha, decimal montoPagado)
    {
        var cobro = new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", montoPagado,
            [new PagoRecibido(MetodoPago.Efectivo, montoPagado)]);
        FijarRegistradoEn(cobro, fecha);
        return cobro;
    }

    private static MovimientoFinanciero CrearMovimiento(DateTimeOffset fecha, TipoMovimientoFinanciero tipo, decimal monto, string concepto = "Gasto operativo")
    {
        var movimiento = new MovimientoFinanciero(tipo, monto, concepto, Guid.NewGuid());
        FijarRegistradoEn(movimiento, fecha);
        return movimiento;
    }

    [Fact]
    public async Task EjecutarAsync_GranularidadDiaria_AgrupaPorDiaYZeroRellenaLosDiasSinMovimientos()
    {
        var desde = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero); // 3 días: 1, 2, 3

        _cobroRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Cobro>)[CrearCobro(new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero), 500m)]);
        _movimientoRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<MovimientoFinanciero>)[
                CrearMovimiento(new DateTimeOffset(2026, 9, 3, 8, 0, 0, TimeSpan.Zero), TipoMovimientoFinanciero.Egreso, 100m),
            ]);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ObtenerResumenPorPeriodoRequest(desde, hasta, GranularidadResumen.Diaria), CancellationToken.None);

        Assert.Equal(3, resultado.Count);
        Assert.Equal(new DateOnly(2026, 9, 1), resultado[0].Periodo);
        Assert.Equal(500m, resultado[0].Ingresos);
        Assert.Equal(500m, resultado[0].Ganancia);

        Assert.Equal(new DateOnly(2026, 9, 2), resultado[1].Periodo);
        Assert.Equal(0m, resultado[1].Ingresos);
        Assert.Equal(0m, resultado[1].Gastos);

        Assert.Equal(new DateOnly(2026, 9, 3), resultado[2].Periodo);
        Assert.Equal(100m, resultado[2].Gastos);
        Assert.Equal(-100m, resultado[2].Ganancia);
    }

    [Fact]
    public async Task EjecutarAsync_GranularidadSemanal_AgrupaTodaLaSemanaEnElLunesDeInicio()
    {
        // Martes 1 y viernes 4 de sept. de 2026 caen en la misma semana ISO (lunes 31 de
        // agosto a domingo 6 de septiembre) -- deben sumarse en un solo punto.
        var desde = new DateTimeOffset(2026, 8, 31, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero);

        _cobroRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Cobro>)[
                CrearCobro(new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero), 300m),
                CrearCobro(new DateTimeOffset(2026, 9, 4, 9, 0, 0, TimeSpan.Zero), 200m),
            ]);
        _movimientoRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<MovimientoFinanciero>)[]);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ObtenerResumenPorPeriodoRequest(desde, hasta, GranularidadResumen.Semanal), CancellationToken.None);

        var semana = Assert.Single(resultado);
        Assert.Equal(new DateOnly(2026, 8, 31), semana.Periodo); // lunes
        Assert.Equal(500m, semana.Ingresos);
    }

    [Fact]
    public async Task EjecutarAsync_CobroConMontoFondo_LoSumaAlDesgloseDeFondoSinDuplicarIngresos()
    {
        var desde = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero);

        var cobro = new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 600m,
            [new PagoRecibido(MetodoPago.Efectivo, 100m)], seguroMedicoId: Guid.NewGuid(),
            codigoAutorizacion: "AUTH-1", tarifarioProcedimientoId: Guid.NewGuid(),
            montoCoberturaExacto: 500m, montoFondoExacto: 250m);
        FijarRegistradoEn(cobro, new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero));

        _cobroRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Cobro>)[cobro]);
        _movimientoRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<MovimientoFinanciero>)[]);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ObtenerResumenPorPeriodoRequest(desde, hasta, GranularidadResumen.Diaria), CancellationToken.None);

        var dia = Assert.Single(resultado);
        // MontoPagado (lo cobrado en caja), no MontoTotal -- igual que ObtenerResumenAnualUseCase.
        Assert.Equal(100m, dia.Ingresos);
        Assert.Equal(250m, dia.FondoGanancias);
    }
}
