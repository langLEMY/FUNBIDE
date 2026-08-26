using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class TarifarioProcedimientoTests
{
    private static TarifarioProcedimiento Crear(
        decimal montoSeguro = 690m, decimal montoPaciente = 100m, decimal montoTotal = 790m,
        decimal? montoFondo = null, EspecialidadMedica? especialidad = null) =>
        new(Guid.NewGuid(), PlanAseguradora.Contributivo, "Consulta odontológica general",
            montoSeguro, montoPaciente, montoTotal, montoFondo, especialidad);

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
            () => new TarifarioProcedimiento(Guid.NewGuid(), PlanAseguradora.Contributivo, "  ", 690m, 100m, 790m));
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
    public void Constructor_MontoFondoNegativo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Crear(montoFondo: -1m));
    }

    [Fact]
    public void Constructor_SinMontoFondo_QuedaNulo()
    {
        var tarifario = Crear();

        Assert.Null(tarifario.MontoFondo);
    }

    [Fact]
    public void Constructor_ConMontoFondoYEspecialidad_LosGuardaTalCual()
    {
        var tarifario = Crear(montoFondo: 250m, especialidad: EspecialidadMedica.Odontologia);

        Assert.Equal(250m, tarifario.MontoFondo);
        Assert.Equal(EspecialidadMedica.Odontologia, tarifario.Especialidad);
    }

    [Fact]
    public void AsignarEspecialidad_CambiaLaEspecialidad()
    {
        var tarifario = Crear();

        tarifario.AsignarEspecialidad(EspecialidadMedica.Pediatria);

        Assert.Equal(EspecialidadMedica.Pediatria, tarifario.Especialidad);
    }

    [Fact]
    public void ActualizarMontos_CambiaLosTresMontos()
    {
        var tarifario = Crear();

        tarifario.ActualizarMontos(400m, 200m, 600m);

        Assert.Equal(400m, tarifario.MontoSeguro);
        Assert.Equal(200m, tarifario.MontoPaciente);
        Assert.Equal(600m, tarifario.MontoTotal);
        Assert.Null(tarifario.MontoFondo);
    }

    [Fact]
    public void ActualizarMontos_ConMontoFondo_LoActualizaTambien()
    {
        var tarifario = Crear(montoFondo: 100m);

        tarifario.ActualizarMontos(500m, 100m, 600m, 250m);

        Assert.Equal(250m, tarifario.MontoFondo);
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
