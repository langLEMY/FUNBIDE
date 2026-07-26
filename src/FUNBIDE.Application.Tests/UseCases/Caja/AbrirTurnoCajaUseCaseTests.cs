using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Caja;

public class AbrirTurnoCajaUseCaseTests
{
    private readonly ITurnoCajaRepository _turnoCajaRepository = Substitute.For<ITurnoCajaRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public AbrirTurnoCajaUseCaseTests()
    {
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    private AbrirTurnoCajaUseCase CrearCasoDeUso() =>
        new(_turnoCajaRepository, _currentUser, _dateTimeProvider, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_YaHayTurnoAbierto_LanzaInvalidOperationExceptionYNoAbreOtro()
    {
        _turnoCajaRepository.ObtenerAbiertoAsync(Arg.Any<CancellationToken>())
            .Returns(new TurnoCaja(Guid.NewGuid(), 500m, DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(new AbrirTurnoCajaRequest(1000m), CancellationToken.None));

        await _turnoCajaRepository.DidNotReceive().AgregarAsync(Arg.Any<TurnoCaja>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_SinTurnoAbierto_AbreUnoNuevoConElMontoInicial()
    {
        _turnoCajaRepository.ObtenerAbiertoAsync(Arg.Any<CancellationToken>()).Returns((TurnoCaja?)null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(new AbrirTurnoCajaRequest(1500m), CancellationToken.None);

        Assert.Equal(1500m, resultado.MontoInicial);
        Assert.Equal(Domain.Enums.EstadoTurnoCaja.Abierto, resultado.Estado);
        await _turnoCajaRepository.Received(1).AgregarAsync(Arg.Any<TurnoCaja>(), Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "caja.abrir-turno", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 201, Arg.Any<CancellationToken>());
    }
}
