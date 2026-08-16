using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Sistema;

public class VerificarEspacioDiscoUseCaseTests
{
    [Fact]
    public async Task EjecutarAsync_DevuelveElEstadoQueReportaElServicio()
    {
        var espacioDisco = Substitute.For<IEspacioDiscoService>();
        var estadoEsperado = new EstadoDiscoDto("C:\\", 15.4, 250.0, false);
        espacioDisco.ObtenerEstadoAsync(Arg.Any<CancellationToken>()).Returns(estadoEsperado);

        var resultado = await new VerificarEspacioDiscoUseCase(espacioDisco).EjecutarAsync(CancellationToken.None);

        Assert.Equal(estadoEsperado, resultado);
    }
}
