using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Caja;

public class CerrarTurnoCajaUseCaseTests
{
    private readonly ITurnoCajaRepository _turnoCajaRepository = Substitute.For<ITurnoCajaRepository>();
    private readonly ICobroRepository _cobroRepository = Substitute.For<ICobroRepository>();
    private readonly IMovimientoFinancieroRepository _movimientoRepository = Substitute.For<IMovimientoFinancieroRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public CerrarTurnoCajaUseCaseTests()
    {
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    private CerrarTurnoCajaUseCase CrearCasoDeUso() => new(
        _turnoCajaRepository, _cobroRepository, _movimientoRepository, _currentUser, _dateTimeProvider, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_SinTurnoAbierto_LanzaInvalidOperationException()
    {
        _turnoCajaRepository.ObtenerAbiertoAsync(Arg.Any<CancellationToken>()).Returns((TurnoCaja?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(new CerrarTurnoCajaRequest(1000m, null), CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_Exitoso_CalculaMontoEsperadoConLaFormulaCompleta()
    {
        var turno = new TurnoCaja(Guid.NewGuid(), 1000m, DateTimeOffset.UtcNow);
        _turnoCajaRepository.ObtenerAbiertoAsync(Arg.Any<CancellationToken>()).Returns(turno);

        var paciente1 = Guid.NewGuid();
        var cobros = new List<Cobro>
        {
            // Efectivo: cuenta para el esperado. Tarjeta: NO debe contarse como efectivo en caja.
            new(paciente1, null, turno.Id, Guid.NewGuid(), "Consulta", 500m, MetodoPago.Efectivo, 500m),
            new(paciente1, null, turno.Id, Guid.NewGuid(), "Consulta", 300m, MetodoPago.Tarjeta, 300m),
        };
        _cobroRepository.ObtenerPorTurnoAsync(turno.Id, Arg.Any<CancellationToken>()).Returns(cobros);

        var movimientos = new List<MovimientoFinanciero>
        {
            new(TipoMovimientoFinanciero.Ingreso, 200m, "Donación en efectivo", Guid.NewGuid(), turnoCajaId: turno.Id),
            new(TipoMovimientoFinanciero.Egreso, 150m, "Compra de insumos", Guid.NewGuid(), turnoCajaId: turno.Id),
        };
        _movimientoRepository.ObtenerPorTurnoAsync(turno.Id, Arg.Any<CancellationToken>()).Returns(movimientos);

        // Esperado = inicial(1000) + efectivo cobrado(500) + ingresos manuales(200) - egresos(150) = 1550
        var resultado = await CrearCasoDeUso().EjecutarAsync(new CerrarTurnoCajaRequest(1550m, "Cuadra exacto"), CancellationToken.None);

        Assert.Equal(1550m, resultado.MontoEsperado);
        Assert.Equal(1550m, resultado.MontoFinalContado);
        Assert.Equal(0m, resultado.Diferencia);
        Assert.Equal(EstadoTurnoCaja.Cerrado, resultado.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ContadoDistintoDelEsperado_RegistraLaDiferencia()
    {
        var turno = new TurnoCaja(Guid.NewGuid(), 0m, DateTimeOffset.UtcNow);
        _turnoCajaRepository.ObtenerAbiertoAsync(Arg.Any<CancellationToken>()).Returns(turno);
        _cobroRepository.ObtenerPorTurnoAsync(turno.Id, Arg.Any<CancellationToken>()).Returns(new List<Cobro>());
        _movimientoRepository.ObtenerPorTurnoAsync(turno.Id, Arg.Any<CancellationToken>()).Returns(new List<MovimientoFinanciero>());

        // Esperado = 0, pero contaron 50 de mas: Diferencia positiva = sobrante (ver TurnoCaja.Diferencia).
        var resultado = await CrearCasoDeUso().EjecutarAsync(new CerrarTurnoCajaRequest(50m, "Sobrante"), CancellationToken.None);

        Assert.Equal(0m, resultado.MontoEsperado);
        Assert.Equal(50m, resultado.Diferencia);
    }
}
