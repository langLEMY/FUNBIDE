using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class MovimientoFinancieroTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_MontoNoPositivo_LanzaArgumentOutOfRangeException(decimal monto)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MovimientoFinanciero(TipoMovimientoFinanciero.Ingreso, monto, "Donación", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_ConceptoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new MovimientoFinanciero(TipoMovimientoFinanciero.Ingreso, 100m, "   ", Guid.NewGuid()));
    }

    [Fact]
    public void MontoConSigno_Ingreso_EsPositivo()
    {
        var movimiento = new MovimientoFinanciero(TipoMovimientoFinanciero.Ingreso, 500m, "Donación", Guid.NewGuid());

        Assert.Equal(500m, movimiento.MontoConSigno);
    }

    [Fact]
    public void MontoConSigno_Egreso_EsNegativo()
    {
        var movimiento = new MovimientoFinanciero(TipoMovimientoFinanciero.Egreso, 500m, "Compra de insumos", Guid.NewGuid());

        Assert.Equal(-500m, movimiento.MontoConSigno);
    }
}
