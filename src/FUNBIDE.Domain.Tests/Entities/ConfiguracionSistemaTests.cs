using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Tests.Entities;

public class ConfiguracionSistemaTests
{
    [Fact]
    public void CambiarModoMantenimiento_Activar_GuardaElMensaje()
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var usuarioId = Guid.NewGuid();
        var ahora = DateTimeOffset.UtcNow;

        configuracion.CambiarModoMantenimiento(true, "Migrando la base de datos", usuarioId, ahora);

        Assert.True(configuracion.ModoMantenimientoActivo);
        Assert.Equal("Migrando la base de datos", configuracion.ModoMantenimientoMensaje);
        Assert.Equal(usuarioId, configuracion.ActualizadoPorUsuarioId);
        Assert.Equal(ahora, configuracion.ActualizadoEn);
    }

    [Fact]
    public void CambiarModoMantenimiento_Desactivar_LimpiaElMensajeAnterior()
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        configuracion.CambiarModoMantenimiento(true, "Migrando la base de datos", Guid.NewGuid(), DateTimeOffset.UtcNow);

        configuracion.CambiarModoMantenimiento(false, null, Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.False(configuracion.ModoMantenimientoActivo);
        Assert.Null(configuracion.ModoMantenimientoMensaje);
    }

    [Fact]
    public void CambiarModoMantenimiento_ActivarSinMensaje_QuedaEnNull()
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);

        configuracion.CambiarModoMantenimiento(true, "   ", Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Null(configuracion.ModoMantenimientoMensaje);
    }
}
