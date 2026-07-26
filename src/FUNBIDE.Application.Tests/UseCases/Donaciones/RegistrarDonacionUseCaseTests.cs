using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Donaciones;
using FUNBIDE.Application.UseCases.Donaciones;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Donaciones;

public class RegistrarDonacionUseCaseTests
{
    private readonly IDonacionRepository _donacionRepository = Substitute.For<IDonacionRepository>();
    private readonly IMovimientoFinancieroRepository _movimientoRepository = Substitute.For<IMovimientoFinancieroRepository>();
    private readonly IResumenDiarioRepository _resumenDiarioRepository = Substitute.For<IResumenDiarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public RegistrarDonacionUseCaseTests()
    {
        _unitOfWork
            .EjecutarEnTransaccionAsync(Arg.Any<Func<CancellationToken, Task<DonacionDto>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<DonacionDto>>>()(CancellationToken.None));

        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    private RegistrarDonacionUseCase CrearCasoDeUso() => new(
        _donacionRepository, _movimientoRepository, _resumenDiarioRepository, _unitOfWork, _currentUser, _dateTimeProvider, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_Exitoso_RegistraDonacionYMovimientoFinancieroPorElMismoMonto()
    {
        var resumen = new ResumenDiario(DateOnly.FromDateTime(DateTime.UtcNow));
        _resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(resumen);

        var request = new RegistrarDonacionRequest("Juan Donante", "8091112222", 5000m, "Donación mensual");

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal("Juan Donante", resultado.DonanteNombre);
        Assert.Equal(5000m, resultado.Monto);
        await _donacionRepository.Received(1).AgregarAsync(Arg.Any<Donacion>(), Arg.Any<CancellationToken>());
        await _movimientoRepository.Received(1).RegistrarAsync(
            Arg.Is<MovimientoFinanciero>(m => m.Monto == 5000m && m.Tipo == Domain.Enums.TipoMovimientoFinanciero.Ingreso),
            Arg.Any<CancellationToken>());

        // Una donación es un ingreso: debe sumar (signo positivo) en el resumen del dashboard, no restar.
        Assert.Equal(5000m, resumen.DineroMovido);

        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "donaciones.registrar", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 201, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_MontoNoPositivo_LanzaArgumentOutOfRangeExceptionYNoRegistraNada()
    {
        var request = new RegistrarDonacionRequest("Juan Donante", null, 0m, "Donación");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _donacionRepository.DidNotReceive().AgregarAsync(Arg.Any<Donacion>(), Arg.Any<CancellationToken>());
        await _movimientoRepository.DidNotReceive().RegistrarAsync(Arg.Any<MovimientoFinanciero>(), Arg.Any<CancellationToken>());
    }
}
