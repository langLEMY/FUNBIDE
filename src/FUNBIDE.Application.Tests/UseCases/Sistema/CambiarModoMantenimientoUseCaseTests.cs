using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Sistema;

public class CambiarModoMantenimientoUseCaseTests
{
    private readonly IConfiguracionSistemaRepository _configuracionRepository = Substitute.For<IConfiguracionSistemaRepository>();
    private readonly IConfiguracionSistemaCache _configuracionCache = Substitute.For<IConfiguracionSistemaCache>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public CambiarModoMantenimientoUseCaseTests()
    {
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    private CambiarModoMantenimientoUseCase CrearCasoDeUso() =>
        new(_configuracionRepository, _configuracionCache, _currentUser, _dateTimeProvider, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_NoExisteFilaTodavia_LaCreaYLaActiva()
    {
        _configuracionRepository.ObtenerAsync(Arg.Any<CancellationToken>()).Returns((ConfiguracionSistema?)null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new CambiarModoMantenimientoRequest(true, "Mantenimiento programado"), CancellationToken.None);

        Assert.True(resultado.Activo);
        Assert.Equal("Mantenimiento programado", resultado.Mensaje);
        await _configuracionRepository.Received(1).AgregarAsync(Arg.Any<ConfiguracionSistema>(), Arg.Any<CancellationToken>());
        await _configuracionRepository.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
        _configuracionCache.Received(1).Invalidar();
    }

    [Fact]
    public async Task EjecutarAsync_YaExisteFila_LaActualizaSinCrearOtra()
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _configuracionRepository.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(configuracion);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new CambiarModoMantenimientoRequest(false, null), CancellationToken.None);

        Assert.False(resultado.Activo);
        await _configuracionRepository.DidNotReceive().AgregarAsync(Arg.Any<ConfiguracionSistema>(), Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "sistema.modo-mantenimiento", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 200, Arg.Any<CancellationToken>());
    }
}
