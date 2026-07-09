using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;

namespace FUNBIDE.Domain.Tests.Entities;

public class InventarioItemTests
{
    private static InventarioItem CrearItem(int stockInicial = 10, int stockMinimo = 2) =>
        new("MED-001", "Paracetamol", stockInicial, CategoriaInventario.Medicamento, stockMinimo);

    [Fact]
    public void Constructor_CodigoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new InventarioItem("   ", "Nombre", 10, CategoriaInventario.Medicamento, 0));
    }

    [Fact]
    public void Constructor_StockInicialNegativo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InventarioItem("MED-001", "Nombre", -1, CategoriaInventario.Medicamento, 0));
    }

    [Fact]
    public void DescargarStock_CantidadValida_DisminuyeStock()
    {
        var item = CrearItem(stockInicial: 10);

        item.DescargarStock(4);

        Assert.Equal(6, item.StockActual);
    }

    [Fact]
    public void DescargarStock_CantidadMayorQueStockDisponible_LanzaStockInsuficienteException()
    {
        var item = CrearItem(stockInicial: 3);

        var ex = Assert.Throws<StockInsuficienteException>(() => item.DescargarStock(4));

        Assert.Equal(3, ex.StockActual);
        Assert.Equal(4, ex.CantidadSolicitada);
        Assert.Equal(3, item.StockActual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DescargarStock_CantidadNoPositiva_LanzaArgumentOutOfRangeException(int cantidad)
    {
        var item = CrearItem();

        Assert.Throws<ArgumentOutOfRangeException>(() => item.DescargarStock(cantidad));
    }

    [Fact]
    public void Reingresar_CantidadValida_AumentaStock()
    {
        var item = CrearItem(stockInicial: 5);

        item.Reingresar(3);

        Assert.Equal(8, item.StockActual);
    }

    [Fact]
    public void Reingresar_CantidadNoPositiva_LanzaArgumentOutOfRangeException()
    {
        var item = CrearItem();

        Assert.Throws<ArgumentOutOfRangeException>(() => item.Reingresar(0));
    }

    [Fact]
    public void AjustarStock_ValorValido_FijaElStockExacto()
    {
        var item = CrearItem(stockInicial: 10);

        item.AjustarStock(3);

        Assert.Equal(3, item.StockActual);
    }

    [Fact]
    public void AjustarStock_ValorNegativo_LanzaArgumentOutOfRangeException()
    {
        var item = CrearItem();

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AjustarStock(-1));
    }

    [Fact]
    public void ActualizarDatos_ValoresValidos_ActualizaNombreYStockMinimo()
    {
        var item = CrearItem();

        item.ActualizarDatos("Acetaminofén", 5);

        Assert.Equal("Acetaminofén", item.Nombre);
        Assert.Equal(5, item.StockMinimo);
    }

    [Fact]
    public void ActualizarDatos_NombreVacio_LanzaArgumentException()
    {
        var item = CrearItem();

        Assert.Throws<ArgumentException>(() => item.ActualizarDatos("  ", 5));
    }
}
