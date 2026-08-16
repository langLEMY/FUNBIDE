using FUNBIDE.Application.UseCases.Dashboard;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Dashboard;

public class ObtenerAlertasAdminUseCaseTests
{
    private readonly IInventarioRepository _inventarioRepository = Substitute.For<IInventarioRepository>();
    private readonly ICobroRepository _cobroRepository = Substitute.For<ICobroRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();

    private ObtenerAlertasAdminUseCase CrearCasoDeUso() =>
        new(_inventarioRepository, _cobroRepository, _pacienteRepository);

    public ObtenerAlertasAdminUseCaseTests()
    {
        _cobroRepository.ObtenerTodosConDeudaAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, decimal>());
        _pacienteRepository.ObtenerNombresPorIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());
    }

    [Fact]
    public async Task EjecutarAsync_StockPorDebajoDelMinimo_SeIncluyeOrdenadoPorMasUrgentePrimero()
    {
        var pocoUrgente = new InventarioItem("A001", "Guantes", stockInicial: 9, CategoriaInventario.Insumo, stockMinimo: 10);
        var muyUrgente = new InventarioItem("A002", "Alcohol", stockInicial: 0, CategoriaInventario.Insumo, stockMinimo: 10);
        _inventarioRepository.ObtenerTodosAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { pocoUrgente, muyUrgente });

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.Equal(2, resultado.StockBajo.Count);
        Assert.Equal(muyUrgente.Id, resultado.StockBajo[0].Id);
        Assert.Equal(pocoUrgente.Id, resultado.StockBajo[1].Id);
    }

    [Fact]
    public async Task EjecutarAsync_StockExactoAlMinimo_NoSeConsideraBajo()
    {
        var enElMinimo = new InventarioItem("A003", "Jeringas", stockInicial: 10, CategoriaInventario.Insumo, stockMinimo: 10);
        _inventarioRepository.ObtenerTodosAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { enElMinimo });

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.Empty(resultado.StockBajo);
    }

    [Fact]
    public async Task EjecutarAsync_PacienteConDeudaSinNombreResuelto_UsaNombreDesconocidoYOrdenaPorMontoDescendente()
    {
        _inventarioRepository.ObtenerTodosAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<InventarioItem>());

        var pacienteConNombre = Guid.NewGuid();
        var pacienteSinNombre = Guid.NewGuid();
        _cobroRepository.ObtenerTodosConDeudaAsync(Arg.Any<CancellationToken>()).Returns(
            new Dictionary<Guid, decimal> { [pacienteConNombre] = 500m, [pacienteSinNombre] = 1500m });
        _pacienteRepository.ObtenerNombresPorIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [pacienteConNombre] = "Juana Pérez" });

        var resultado = await CrearCasoDeUso().EjecutarAsync(CancellationToken.None);

        Assert.Equal(2, resultado.PacientesConDeuda.Count);
        Assert.Equal(pacienteSinNombre, resultado.PacientesConDeuda[0].PacienteId);
        Assert.Equal("Paciente desconocido", resultado.PacientesConDeuda[0].PacienteNombre);
        Assert.Equal("Juana Pérez", resultado.PacientesConDeuda[1].PacienteNombre);
    }
}
