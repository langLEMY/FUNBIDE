using System.Security.Cryptography;
using System.Text;
using FUNBIDE.Launcher.Actualizacion;

namespace FUNBIDE.Launcher.Actualizacion.Tests;

public class VerificadorHashTests
{
    [Fact]
    public void Coincide_HashCorrecto_DevuelveTrue()
    {
        var contenido = Encoding.UTF8.GetBytes("contenido de prueba");
        var hashReal = Convert.ToHexString(SHA256.HashData(contenido));

        Assert.True(VerificadorHash.Coincide(contenido, hashReal));
    }

    [Fact]
    public void Coincide_HashEnMinusculas_DevuelveTrue()
    {
        var contenido = Encoding.UTF8.GetBytes("contenido de prueba");
        var hashReal = Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant();

        Assert.True(VerificadorHash.Coincide(contenido, hashReal));
    }

    [Fact]
    public void Coincide_ConNombreDeArchivoDespuesDelHash_DevuelveTrue()
    {
        // Formato típico de sha256sum: "<hash>  nombre-del-archivo.exe".
        var contenido = Encoding.UTF8.GetBytes("contenido de prueba");
        var hashReal = Convert.ToHexString(SHA256.HashData(contenido));

        Assert.True(VerificadorHash.Coincide(contenido, $"{hashReal}  FUNBIDE-Setup-x64.exe\n"));
    }

    [Fact]
    public void Coincide_ContenidoAlterado_DevuelveFalse()
    {
        var contenido = Encoding.UTF8.GetBytes("contenido de prueba");
        var hashDeOtroContenido = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("otro contenido")));

        Assert.False(VerificadorHash.Coincide(contenido, hashDeOtroContenido));
    }

    [Fact]
    public void Coincide_HashVacio_DevuelveFalse()
    {
        var contenido = Encoding.UTF8.GetBytes("contenido de prueba");

        Assert.False(VerificadorHash.Coincide(contenido, string.Empty));
    }
}
