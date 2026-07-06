using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Inventario;

public class CrearInventarioItemUseCaseTests
{
    private readonly IInventarioRepository _inventarioRepository = Substitute.For<IInventarioRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    private CrearInventarioItemUseCase CrearCasoDeUso() =>
        new(_inventarioRepository, _currentUser, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_CodigoYaExiste_LanzaCodigoInventarioEnUsoException()
    {
        _inventarioRepository.ExisteCodigoAsync("MED-001", Arg.Any<CancellationToken>()).Returns(true);
        var request = new CrearInventarioItemRequest("MED-001", "Paracetamol", 10, CategoriaInventario.Medicamento, 2);

        await Assert.ThrowsAsync<CodigoInventarioEnUsoException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _inventarioRepository.DidNotReceive().AgregarAsync(Arg.Any<Domain.Entities.InventarioItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_CodigoDisponible_AgregaYRetornaDto()
    {
        _inventarioRepository.ExisteCodigoAsync("MED-002", Arg.Any<CancellationToken>()).Returns(false);
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        var request = new CrearInventarioItemRequest("MED-002", "Ibuprofeno", 20, CategoriaInventario.Medicamento, 5);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal("MED-002", resultado.Codigo);
        Assert.Equal(20, resultado.StockActual);
        await _inventarioRepository.Received(1).AgregarAsync(Arg.Any<Domain.Entities.InventarioItem>(), Arg.Any<CancellationToken>());
        await _inventarioRepository.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "inventario.creacion", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 201, Arg.Any<CancellationToken>());
    }
}
