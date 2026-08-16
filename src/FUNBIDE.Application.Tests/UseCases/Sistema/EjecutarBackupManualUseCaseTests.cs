using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.UseCases.Sistema;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FUNBIDE.Application.Tests.UseCases.Sistema;

public class EjecutarBackupManualUseCaseTests
{
    private readonly IBackupEjecutorService _backupEjecutor = Substitute.For<IBackupEjecutorService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public EjecutarBackupManualUseCaseTests()
    {
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    private EjecutarBackupManualUseCase CrearCasoDeUso() =>
        new(_backupEjecutor, _currentUser, _dateTimeProvider, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_BackupExitoso_DevuelveExitosoYAudita200()
    {
        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Null(resultado.Mensaje);
        await _backupEjecutor.Received(1).EjecutarAsync(Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "sistema.backup-manual", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 200, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_BackupFalla_DevuelveNoExitosoConMensajeYAudita500()
    {
        _backupEjecutor.EjecutarAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("pg_dump no encontrado"));

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.False(resultado.Exitoso);
        Assert.Equal("pg_dump no encontrado", resultado.Mensaje);
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "sistema.backup-manual", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 500, Arg.Any<CancellationToken>());
    }
}
