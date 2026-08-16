using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Sistema;

public class VerificarEstadoSistemaUseCaseTests
{
    private readonly IEstadoBaseDeDatosService _estadoBaseDeDatos = Substitute.For<IEstadoBaseDeDatosService>();
    private readonly IEstadoBackupService _estadoBackup = Substitute.For<IEstadoBackupService>();
    private readonly ISupabaseStorageService _almacenamiento = Substitute.For<ISupabaseStorageService>();
    private readonly IConfiguracionSistemaRepository _configuracionRepository = Substitute.For<IConfiguracionSistemaRepository>();

    private VerificarEstadoSistemaUseCase CrearCasoDeUso() =>
        new(_estadoBaseDeDatos, _estadoBackup, _almacenamiento, _configuracionRepository);

    [Fact]
    public async Task EjecutarAsync_ModoMantenimientoActivo_LoReflejaEnElDto()
    {
        _estadoBaseDeDatos.VerificarConexionAsync(Arg.Any<CancellationToken>()).Returns(true);
        _almacenamiento.VerificarConexionAsync(Arg.Any<CancellationToken>()).Returns(true);
        _estadoBackup.ObtenerUltimoEstadoAsync(Arg.Any<CancellationToken>())
            .Returns(new EstadoBackupInfo(new DateTimeOffset(2026, 7, 26, 3, 0, 0, TimeSpan.Zero), true));

        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        configuracion.CambiarModoMantenimiento(true, "Mantenimiento programado", Guid.NewGuid(), DateTimeOffset.UtcNow);
        _configuracionRepository.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(configuracion);

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.True(resultado.BaseDeDatosOperativa);
        Assert.True(resultado.AlmacenamientoOperativo);
        Assert.True(resultado.UltimoBackupExitoso);
        Assert.True(resultado.ModoMantenimientoActivo);
        Assert.Equal("Mantenimiento programado", resultado.ModoMantenimientoMensaje);
    }

    [Fact]
    public async Task EjecutarAsync_SinConfiguracionNiBackupTodavia_UsaValoresPorDefectoSinLanzar()
    {
        _estadoBaseDeDatos.VerificarConexionAsync(Arg.Any<CancellationToken>()).Returns(true);
        _almacenamiento.VerificarConexionAsync(Arg.Any<CancellationToken>()).Returns(true);
        _estadoBackup.ObtenerUltimoEstadoAsync(Arg.Any<CancellationToken>()).Returns((EstadoBackupInfo?)null);
        _configuracionRepository.ObtenerAsync(Arg.Any<CancellationToken>()).Returns((ConfiguracionSistema?)null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.Null(resultado.UltimoBackupUtc);
        Assert.Null(resultado.UltimoBackupExitoso);
        Assert.False(resultado.ModoMantenimientoActivo);
        Assert.Null(resultado.ModoMantenimientoMensaje);
    }
}
