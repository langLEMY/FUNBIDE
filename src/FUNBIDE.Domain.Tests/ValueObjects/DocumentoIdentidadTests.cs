using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Tests.ValueObjects;

public class DocumentoIdentidadTests
{
    [Fact]
    public void Crear_ValorVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DocumentoIdentidad.Crear("   "));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012345678901")]
    public void Crear_LongitudFueraDeRango_LanzaArgumentException(string valor)
    {
        Assert.Throws<ArgumentException>(() => DocumentoIdentidad.Crear(valor));
    }

    [Fact]
    public void Crear_ValorConEspacios_RecortaAlAlmacenar()
    {
        var documento = DocumentoIdentidad.Crear("  00112345678  ");

        Assert.Equal("00112345678", documento.Valor);
    }

    [Fact]
    public void Igualdad_MismoValor_SonIguales()
    {
        var a = DocumentoIdentidad.Crear("00112345678");
        var b = DocumentoIdentidad.Crear("00112345678");

        Assert.Equal(a, b);
    }
}
