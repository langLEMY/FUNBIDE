using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class TurnoCajaTests
{
    private static TurnoCaja CrearTurno(decimal montoInicial = 1000m) =>
        new(Guid.NewGuid(), montoInicial, DateTimeOffset.UtcNow);

    [Fact]
    public void Constructor_MontoInicialNegativo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TurnoCaja(Guid.NewGuid(), -1m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_TurnoNuevo_QuedaAbierto()
    {
        var turno = CrearTurno();

        Assert.Equal(EstadoTurnoCaja.Abierto, turno.Estado);
    }

    [Fact]
    public void Cerrar_TurnoAbierto_CambiaEstadoACerradoYGuardaDatosDeCierre()
    {
        var turno = CrearTurno();
        var usuarioCierre = Guid.NewGuid();

        turno.Cerrar(usuarioCierre, 1500m, 1500m, "Cuadra sin diferencias", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoTurnoCaja.Cerrado, turno.Estado);
        Assert.Equal(usuarioCierre, turno.UsuarioCierreId);
        Assert.Equal(1500m, turno.MontoFinalContado);
        Assert.Equal(1500m, turno.MontoEsperado);
        Assert.Equal(0m, turno.Diferencia);
        Assert.NotNull(turno.CerradoEn);
    }

    [Theory]
    [InlineData(1500, 1500, 0)]
    [InlineData(1600, 1500, 100)]
    [InlineData(1400, 1500, -100)]
    public void Cerrar_CalculaDiferenciaComoContadoMenosEsperado(decimal contado, decimal esperado, decimal diferenciaEsperada)
    {
        var turno = CrearTurno();

        turno.Cerrar(Guid.NewGuid(), contado, esperado, null, DateTimeOffset.UtcNow);

        Assert.Equal(diferenciaEsperada, turno.Diferencia);
    }

    [Fact]
    public void Cerrar_TurnoYaCerrado_LanzaInvalidOperationException()
    {
        var turno = CrearTurno();
        turno.Cerrar(Guid.NewGuid(), 1500m, 1500m, null, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            turno.Cerrar(Guid.NewGuid(), 1500m, 1500m, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cerrar_MontoContadoNegativo_LanzaArgumentOutOfRangeException()
    {
        var turno = CrearTurno();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            turno.Cerrar(Guid.NewGuid(), -1m, 1000m, null, DateTimeOffset.UtcNow));
    }
}
