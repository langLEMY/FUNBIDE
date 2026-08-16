using FUNBIDE.Application.DTOs.Donaciones;
using FUNBIDE.Application.UseCases.Donaciones;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Donaciones;

public class ListarDonacionesUseCaseTests
{
    private readonly IDonacionRepository _donacionRepository = Substitute.For<IDonacionRepository>();

    private ListarDonacionesUseCase CrearCasoDeUso() => new(_donacionRepository);

    [Fact]
    public async Task EjecutarAsync_ConsultaElRangoPedidoYMapeaLasDonacionesADto()
    {
        var desde = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var donacion = new Donacion("Ana Ruiz", "809-555-0000", 2500m, "Compra de sillas de ruedas", Guid.NewGuid());
        _donacionRepository.ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>())
            .Returns(new[] { donacion });

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ListarDonacionesRequest(desde, hasta), CancellationToken.None);

        var dto = Assert.Single(resultado);
        Assert.Equal(donacion.Id, dto.Id);
        Assert.Equal("Ana Ruiz", dto.DonanteNombre);
        Assert.Equal("809-555-0000", dto.DonanteContacto);
        Assert.Equal(2500m, dto.Monto);
        Assert.Equal("Compra de sillas de ruedas", dto.Concepto);
        await _donacionRepository.Received(1).ObtenerPorRangoAsync(desde, hasta, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_SinDonacionesEnElRango_DevuelveListaVacia()
    {
        _donacionRepository.ObtenerPorRangoAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Donacion>());

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ListarDonacionesRequest(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.Empty(resultado);
    }
}
