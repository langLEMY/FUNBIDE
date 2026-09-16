using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.UseCases.Citas;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Citas;

public class ListarAgendaUseCaseTests
{
    private readonly ICitaRepository _citaRepository = Substitute.For<ICitaRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICobroRepository _cobroRepository = Substitute.For<ICobroRepository>();

    private ListarAgendaUseCase CrearCasoDeUso() => new(_citaRepository, _pacienteRepository, _usuarioRepository, _cobroRepository);

    public ListarAgendaUseCaseTests()
    {
        _citaRepository
            .ObtenerPaginadoAsync(
                Arg.Any<DateOnly?>(), Arg.Any<Guid?>(), Arg.Any<EstadoCita?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Cita>(), 0));
    }

    [Fact]
    public async Task EjecutarAsync_SinPaginaNiTamanoPagina_PideElTechoSinLimiteExplicito()
    {
        // Comportamiento de hoy en AgendaPage.tsx: sin fecha ni doctor (filtro "Todos"),
        // sigue trayendo todo de una sola vez, no una página de 50.
        await CrearCasoDeUso().EjecutarAsync(new ListarAgendaRequest(null, null), CancellationToken.None);

        await _citaRepository.Received(1).ObtenerPaginadoAsync(
            null, null, null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConPaginaExplicita_ClampeaTamanoPaginaAlMaximo()
    {
        await CrearCasoDeUso().EjecutarAsync(
            new ListarAgendaRequest(null, null, Pagina: 1, TamanoPagina: 5000), CancellationToken.None);

        await _citaRepository.Received(1).ObtenerPaginadoAsync(
            null, null, null, 1, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConEstado_LoPropagaAlRepositorio()
    {
        await CrearCasoDeUso().EjecutarAsync(
            new ListarAgendaRequest(null, null, EstadoCita.Programada), CancellationToken.None);

        await _citaRepository.Received(1).ObtenerPaginadoAsync(
            null, null, EstadoCita.Programada, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_SinCitas_DevuelveListaVaciaSinConsultarLosDemasRepositorios()
    {
        var resultado = await CrearCasoDeUso().EjecutarAsync(new ListarAgendaRequest(null, null), CancellationToken.None);

        Assert.Empty(resultado);
        await _pacienteRepository.DidNotReceive().ObtenerNombresPorIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }
}
