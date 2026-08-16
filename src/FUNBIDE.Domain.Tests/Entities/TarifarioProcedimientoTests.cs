using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class TarifarioProcedimientoTests
{
    private static TarifarioProcedimiento Crear(
        decimal montoSeguro = 690m, decimal montoPaciente = 100m, decimal montoTotal = 790m) =>
        new(Guid.NewGuid(), PlanSenasa.Contributivo, "Consulta odontológica general", montoSeguro, montoPaciente, montoTotal);

    [Fact]
    public void Constructor_DatosValidos_QuedaActivoConLosMontosDados()
    {
        var tarifario = Crear();

        Assert.True(tarifario.Activo);
        Assert.Equal(690m, tarifario.MontoSeguro);
        Assert.Equal(100m, tarifario.MontoPaciente);
        Assert.Equal(790m, tarifario.MontoTotal);
    }

    [Fact]
    public void Constructor_ProcedimientoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new TarifarioProcedimiento(Guid.NewGuid(), PlanSenasa.Contributivo, "  ", 690m, 100m, 790m));
    }

    [Fact]
    public void Constructor_MontoTotalNoPositivo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Crear(montoTotal: 0m));
    }

    [Fact]
    public void Constructor_MontoSeguroNegativo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Crear(montoSeguro: -1m));
    }

    [Fact]
    public void ActualizarMontos_CambiaLosTresMontos()
    {
        var tarifario = Crear();

        tarifario.ActualizarMontos(400m, 200m, 600m);

        Assert.Equal(400m, tarifario.MontoSeguro);
        Assert.Equal(200m, tarifario.MontoPaciente);
        Assert.Equal(600m, tarifario.MontoTotal);
    }

    [Fact]
    public void Desactivar_QuedaInactivo_YReactivarLoVuelveAActivar()
    {
        var tarifario = Crear();

        tarifario.Desactivar();
        Assert.False(tarifario.Activo);

        tarifario.Reactivar();
        Assert.True(tarifario.Activo);
    }
}
