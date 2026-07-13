using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class CobroTests
{
    private static Cobro CrearCobroSimple(decimal montoTotal = 1000m, decimal montoPagado = 1000m) => new(
        Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", montoTotal, MetodoPago.Efectivo, montoPagado);

    [Fact]
    public void Constructor_MontoTotalNoPositivo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CrearCobroSimple(0m, 0m));
    }

    [Fact]
    public void Constructor_ConceptoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "  ", 100m, MetodoPago.Efectivo, 100m));
    }

    [Fact]
    public void Constructor_SinSeguro_MontoACargoPacienteEsElMontoTotal()
    {
        var cobro = CrearCobroSimple(1000m, 1000m);

        Assert.Equal(1000m, cobro.MontoACargoPaciente);
        Assert.Equal(0m, cobro.MontoPendiente);
        Assert.Null(cobro.MontoCobertura);
    }

    [Fact]
    public void Constructor_MontoPagadoMenorAlDebido_DejaSaldoPendiente()
    {
        var cobro = CrearCobroSimple(1000m, 600m);

        Assert.Equal(400m, cobro.MontoPendiente);
    }

    [Fact]
    public void Constructor_MontoPagadoMayorAlDebido_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CrearCobroSimple(1000m, 1000.01m));
    }

    [Fact]
    public void Constructor_ConSeguro_CalculaCoberturaYCopagoAutomaticamente()
    {
        var cobro = new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m,
            MetodoPago.Efectivo, montoPagado: 700m, seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: 30m, codigoAutorizacion: "AUTH-123");

        Assert.Equal(300m, cobro.MontoCobertura);
        Assert.Equal(700m, cobro.MontoACargoPaciente);
        Assert.Equal(0m, cobro.MontoPendiente);
    }

    [Fact]
    public void Constructor_ConSeguroSinCodigoAutorizacion_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m,
            MetodoPago.Efectivo, montoPagado: 700m, seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: 30m, codigoAutorizacion: "  "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Constructor_ConSeguroPorcentajeFueraDeRango_LanzaArgumentOutOfRangeException(decimal porcentaje)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m,
            MetodoPago.Efectivo, montoPagado: 0m, seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: porcentaje, codigoAutorizacion: "AUTH-123"));
    }
}
