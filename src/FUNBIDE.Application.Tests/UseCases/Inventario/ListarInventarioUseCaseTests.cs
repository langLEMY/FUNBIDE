using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Inventario;

public class ListarInventarioUseCaseTests
{
    private readonly IInventarioRepository _inventarioRepository = Substitute.For<IInventarioRepository>();

    private ListarInventarioUseCase CrearCasoDeUso() => new(_inventarioRepository);

    public ListarInventarioUseCaseTests()
    {
        _inventarioRepository
            .ObtenerPaginadoAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InventarioItem>(), 0));
    }

    [Fact]
    public async Task EjecutarAsync_SinNada_TraeTodoComoAntes()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarInventarioRequest(), CancellationToken.None);

        await _inventarioRepository.Received(1).ObtenerPaginadoAsync(null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_BusquedaDeUnCaracter_LaIgnora()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarInventarioRequest(Busqueda: "a"), CancellationToken.None);

        await _inventarioRepository.Received(1).ObtenerPaginadoAsync(null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConPaginaExplicita_ClampeaTamanoPaginaAlMaximo()
    {
        await CrearCasoDeUso().EjecutarAsync(
            new ListarInventarioRequest(Pagina: 1, TamanoPagina: 5000), CancellationToken.None);

        await _inventarioRepository.Received(1).ObtenerPaginadoAsync(null, 1, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_MapeaLosItemsADto()
    {
        var item = new InventarioItem("AMX-500", "Amoxicilina 500mg", 40, CategoriaInventario.Medicamento, 10);
        _inventarioRepository
            .ObtenerPaginadoAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new[] { item }, 1));

        var resultado = await CrearCasoDeUso().EjecutarAsync(new ListarInventarioRequest(), CancellationToken.None);

        var dto = Assert.Single(resultado);
        Assert.Equal(item.Id, dto.Id);
        Assert.Equal("AMX-500", dto.Codigo);
        Assert.Equal(40, dto.StockActual);
    }
}
