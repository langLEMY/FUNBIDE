using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.UseCases.Pacientes;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Pacientes;

public class ListarPacientesUseCaseTests
{
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();

    private ListarPacientesUseCase CrearCasoDeUso() => new(_pacienteRepository);

    public ListarPacientesUseCaseTests()
    {
        _pacienteRepository
            .ObtenerPaginadoAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<Domain.Enums.EstadoPaciente?>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Paciente>(), 0));
    }

    [Fact]
    public async Task EjecutarAsync_BusquedaDeUnSoloCaracter_LaIgnoraYNoLaMandaAlRepositorio()
    {
        // Protege contra un debounce de frontend que fallara y mandara cada tecla: un
        // ILIKE '%x%' de un carácter es de los queries más pesados posibles.
        await CrearCasoDeUso().EjecutarAsync(new ListarPacientesRequest(1, 50, "a", null), CancellationToken.None);

        await _pacienteRepository.Received(1).ObtenerPaginadoAsync(
            1, 50, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_BusquedaDeDosCaracteres_LaManda()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarPacientesRequest(1, 50, "an", null), CancellationToken.None);

        await _pacienteRepository.Received(1).ObtenerPaginadoAsync(
            1, 50, "an", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_BusquedaSoloEspacios_LaIgnora()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarPacientesRequest(1, 50, "   ", null), CancellationToken.None);

        await _pacienteRepository.Received(1).ObtenerPaginadoAsync(
            1, 50, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_PaginaMenorAUno_SeClampeaAUno()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarPacientesRequest(0, 50, null, null), CancellationToken.None);

        await _pacienteRepository.Received(1).ObtenerPaginadoAsync(
            1, 50, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_TamanoPaginaMayorAlMaximo_SeClampeaAlMaximo()
    {
        await CrearCasoDeUso().EjecutarAsync(new ListarPacientesRequest(1, 5000, null, null), CancellationToken.None);

        await _pacienteRepository.Received(1).ObtenerPaginadoAsync(
            1, 100, null, null, Arg.Any<CancellationToken>());
    }
}
