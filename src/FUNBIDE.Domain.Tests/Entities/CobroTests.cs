using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class CobroTests
{
    private static IReadOnlyList<PagoRecibido> PagoEfectivo(decimal monto) =>
        monto > 0 ? [new PagoRecibido(MetodoPago.Efectivo, monto)] : [];

    private static Cobro CrearCobroSimple(decimal montoTotal = 1000m, decimal montoPagado = 1000m) => new(
        Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", montoTotal, PagoEfectivo(montoPagado));

    [Fact]
    public void Constructor_MontoTotalNoPositivo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CrearCobroSimple(0m, 0m));
    }

    [Fact]
    public void Constructor_ConceptoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "  ", 100m, PagoEfectivo(100m)));
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
            PagoEfectivo(700m), seguroMedicoId: Guid.NewGuid(),
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
            PagoEfectivo(700m), seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: 30m, codigoAutorizacion: "  "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Constructor_ConSeguroPorcentajeFueraDeRango_LanzaArgumentOutOfRangeException(decimal porcentaje)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m,
            PagoEfectivo(0m), seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: porcentaje, codigoAutorizacion: "AUTH-123"));
    }

    [Fact]
    public void Constructor_ConMontoCoberturaExacto_LoUsaTalCualSinDerivarloDeUnPorcentaje()
    {
        var tarifarioId = Guid.NewGuid();
        var cobro = new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta odontológica general", 790m,
            PagoEfectivo(100m), seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: null, codigoAutorizacion: "AUTH-1",
            tarifarioProcedimientoId: tarifarioId, montoCoberturaExacto: 690m);

        Assert.Equal(690m, cobro.MontoCobertura);
        Assert.Equal(100m, cobro.MontoACargoPaciente);
        Assert.Null(cobro.PorcentajeCobertura);
        Assert.Equal(tarifarioId, cobro.TarifarioProcedimientoId);
    }

    [Fact]
    public void Constructor_ConMontoCoberturaExactoMayorAlTotal_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta", 500m,
            PagoEfectivo(0m), seguroMedicoId: Guid.NewGuid(),
            porcentajeCobertura: null, codigoAutorizacion: "AUTH-1", montoCoberturaExacto: 600m));
    }

    [Fact]
    public void Constructor_SinSeguroYSinTarifarioProcedimientoId_LoDejaNulo()
    {
        var cobro = CrearCobroSimple();

        Assert.Null(cobro.TarifarioProcedimientoId);
    }

    [Fact]
    public void Constructor_ConVariosMetodosDePago_SumaTodasLasLineasEnMontoPagado()
    {
        var pagos = new List<PagoRecibido> { new(MetodoPago.Tarjeta, 300m), new(MetodoPago.Efectivo, 200m) };

        var cobro = new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m, pagos);

        Assert.Equal(500m, cobro.MontoPagado);
        Assert.Equal(2, cobro.Pagos.Count);
        Assert.Equal(500m, cobro.MontoPendiente);
    }

    [Fact]
    public void Constructor_ConDosLineasDelMismoMetodo_LanzaArgumentException()
    {
        var pagos = new List<PagoRecibido> { new(MetodoPago.Efectivo, 300m), new(MetodoPago.Efectivo, 200m) };

        Assert.Throws<ArgumentException>(() => new Cobro(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m, pagos));
    }
}
