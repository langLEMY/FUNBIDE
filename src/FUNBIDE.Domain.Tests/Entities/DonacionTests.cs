using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Tests.Entities;

public class DonacionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_MontoNoPositivo_LanzaArgumentOutOfRangeException(decimal monto)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Donacion("Juan Pérez", null, monto, "Donación general", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_DonanteNombreVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Donacion("   ", null, 500m, "Donación general", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_ConceptoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Donacion("Juan Pérez", null, 500m, "   ", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_ContactoEnBlanco_QuedaComoNull()
    {
        var donacion = new Donacion("Juan Pérez", "   ", 500m, "Donación general", Guid.NewGuid());

        Assert.Null(donacion.DonanteContacto);
    }

    [Fact]
    public void Constructor_DatosValidos_AsignaCampos()
    {
        var usuarioId = Guid.NewGuid();
        var donacion = new Donacion(" Juan Pérez ", " juan@correo.com ", 1500m, " Campaña de invierno ", usuarioId);

        Assert.Equal("Juan Pérez", donacion.DonanteNombre);
        Assert.Equal("juan@correo.com", donacion.DonanteContacto);
        Assert.Equal(1500m, donacion.Monto);
        Assert.Equal("Campaña de invierno", donacion.Concepto);
        Assert.Equal(usuarioId, donacion.UsuarioId);
    }
}
