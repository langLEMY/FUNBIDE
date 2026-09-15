using FUNBIDE.Launcher.Actualizacion;

namespace FUNBIDE.Launcher.Actualizacion.Tests;

public class ComparadorVersionesTests
{
    [Theory]
    [InlineData("v1.5.0", "1.4.0", true)]
    [InlineData("1.5.0", "1.4.0", true)]
    [InlineData("V2.0.0", "1.4.0", true)]
    [InlineData("v1.4.0", "1.4.0", false)]
    [InlineData("v1.3.9", "1.4.0", false)]
    [InlineData("v1.4.0.1", "1.4.0", true)]
    public void HayVersionMasNueva_ComparaCorrectamente(string tagRemoto, string versionLocalTexto, bool esperado)
    {
        var versionLocal = Version.Parse(versionLocalTexto);

        var resultado = ComparadorVersiones.HayVersionMasNueva(tagRemoto, versionLocal);

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("v")]
    [InlineData("release-experimental")]
    [InlineData("v1.x.y")]
    public void HayVersionMasNueva_TagNoParseable_DevuelveFalseSinLanzar(string tagRemoto)
    {
        var resultado = ComparadorVersiones.HayVersionMasNueva(tagRemoto, new Version(1, 0, 0));

        Assert.False(resultado);
    }
}
