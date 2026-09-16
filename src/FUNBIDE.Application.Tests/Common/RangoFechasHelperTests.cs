using FUNBIDE.Application.Common;

namespace FUNBIDE.Application.Tests.Common;

public class RangoFechasHelperTests
{
    [Fact]
    public void RecortarASpanMaximo_RangoDentroDelLimite_NoLoToca()
    {
        var desde = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = desde.AddDays(30);

        var (desdeResultado, hastaResultado) = RangoFechasHelper.RecortarASpanMaximo(desde, hasta);

        Assert.Equal(desde, desdeResultado);
        Assert.Equal(hasta, hastaResultado);
    }

    [Fact]
    public void RecortarASpanMaximo_RangoMasLargoQueElMaximo_RecortaDesdeManteniendoHasta()
    {
        var hasta = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var desde = hasta.AddYears(-10);

        var (desdeResultado, hastaResultado) = RangoFechasHelper.RecortarASpanMaximo(desde, hasta, spanMaximoDias: 366);

        Assert.Equal(hasta, hastaResultado);
        Assert.Equal(hasta.AddDays(-366), desdeResultado);
    }
}
