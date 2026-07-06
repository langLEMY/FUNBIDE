using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Inventario;

public class DescargarInventarioUseCaseTests
{
    private readonly IInventarioRepository _inventarioRepository = Substitute.For<IInventarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public DescargarInventarioUseCaseTests()
    {
        // La unidad de trabajo real abre una transacción; en el test simplemente
        // ejecuta la operación recibida para que el caso de uso corra igual.
        _unitOfWork
            .EjecutarEnTransaccionAsync(Arg.Any<Func<CancellationToken, Task<MovimientoInventarioDto>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<MovimientoInventarioDto>>>()(CancellationToken.None));

        _currentUser.UsuarioId.Returns(Guid.NewGuid());
    }

    private DescargarInventarioUseCase CrearCasoDeUso() =>
        new(_inventarioRepository, _unitOfWork, _currentUser, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_StockInsuficiente_LanzaStockInsuficienteExceptionYNoRegistraMovimiento()
    {
        var item = new InventarioItem("MED-003", "Amoxicilina", 2, CategoriaInventario.Medicamento, 1);
        _inventarioRepository.ObtenerConBloqueoAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var request = new DescargarInventarioRequest(item.Id, 5, null);

        await Assert.ThrowsAsync<StockInsuficienteException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _inventarioRepository.DidNotReceive().RegistrarMovimientoAsync(Arg.Any<MovimientoInventario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ItemNoExiste_LanzaRecursoNoEncontradoException()
    {
        var idInexistente = Guid.NewGuid();
        _inventarioRepository.ObtenerConBloqueoAsync(idInexistente, Arg.Any<CancellationToken>()).Returns((InventarioItem?)null);
        var request = new DescargarInventarioRequest(idInexistente, 1, null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_StockSuficiente_DescargaYRegistraMovimiento()
    {
        var item = new InventarioItem("MED-004", "Loratadina", 10, CategoriaInventario.Medicamento, 1);
        _inventarioRepository.ObtenerConBloqueoAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var request = new DescargarInventarioRequest(item.Id, 4, "Consulta 123");

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(6, resultado.StockResultante);
        Assert.Equal(6, item.StockActual);
        await _inventarioRepository.Received(1).RegistrarMovimientoAsync(Arg.Any<MovimientoInventario>(), Arg.Any<CancellationToken>());
    }
}
