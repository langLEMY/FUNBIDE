using FUNBIDE.Application.DTOs.Auditoria;
using FUNBIDE.Application.UseCases.Auditoria;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Auditoria;

public class ObtenerLogsAuditoriaUseCaseTests
{
    private readonly IAuditoriaLogRepository _auditoriaLogRepository = Substitute.For<IAuditoriaLogRepository>();

    private ObtenerLogsAuditoriaUseCase CrearCasoDeUso() => new(_auditoriaLogRepository);

    public ObtenerLogsAuditoriaUseCaseTests()
    {
        _auditoriaLogRepository
            .ObtenerPaginadoAsync(
                Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<AuditoriaLog>(), 0));
    }

    [Fact]
    public async Task EjecutarAsync_SinDesdeNiHasta_UsaUltimos30DiasHastaAhora()
    {
        // Sin esto, un caller que no manda ninguna fecha (a diferencia del frontend actual,
        // que siempre manda ambas) traería TODA la tabla de auditoría de una sola vez.
        await CrearCasoDeUso().EjecutarAsync(new ConsultarAuditoriaRequest(null, null, null), CancellationToken.None);

        await _auditoriaLogRepository.Received(1).ObtenerPaginadoAsync(
            Arg.Is<DateTimeOffset>(d => d <= DateTimeOffset.UtcNow.AddDays(-29) && d >= DateTimeOffset.UtcNow.AddDays(-31)),
            Arg.Is<DateTimeOffset>(h => h <= DateTimeOffset.UtcNow.AddMinutes(1) && h >= DateTimeOffset.UtcNow.AddMinutes(-1)),
            null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_RangoMasLargoQueElSpanMaximo_LoRecortaAlSpanMaximo()
    {
        var hasta = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var desde = hasta.AddYears(-5);

        await CrearCasoDeUso().EjecutarAsync(new ConsultarAuditoriaRequest(desde, hasta, null), CancellationToken.None);

        await _auditoriaLogRepository.Received(1).ObtenerPaginadoAsync(
            hasta.AddDays(-366), hasta, null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConDesdeYHastaExplicitos_NoLosToca()
    {
        var desde = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        await CrearCasoDeUso().EjecutarAsync(new ConsultarAuditoriaRequest(desde, hasta, "pacientes"), CancellationToken.None);

        await _auditoriaLogRepository.Received(1).ObtenerPaginadoAsync(
            desde, hasta, "pacientes", 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_SinPaginaNiTamanoPagina_PideElTechoSinLimiteExplicito()
    {
        // Sigue devolviendo todo el rango de una sola vez (comportamiento de hoy en
        // ActividadPage.tsx/MiPerfilPage.tsx), no una página de 50.
        await CrearCasoDeUso().EjecutarAsync(new ConsultarAuditoriaRequest(null, null, null), CancellationToken.None);

        await _auditoriaLogRepository.Received(1).ObtenerPaginadoAsync(
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), null, 1, 10_000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConPaginaExplicita_ClampeaTamanoPaginaAlMaximo()
    {
        await CrearCasoDeUso().EjecutarAsync(
            new ConsultarAuditoriaRequest(null, null, null, Pagina: 2, TamanoPagina: 5000), CancellationToken.None);

        await _auditoriaLogRepository.Received(1).ObtenerPaginadoAsync(
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), null, 2, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_MapeaLosLogsADto()
    {
        var log = new AuditoriaLog("pacientes.crear", "pacientes", DocumentoJson.Crear("{}"), Guid.NewGuid(), 201);
        _auditoriaLogRepository
            .ObtenerPaginadoAsync(
                Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new[] { log }, 1));

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ConsultarAuditoriaRequest(null, null, null), CancellationToken.None);

        var dto = Assert.Single(resultado);
        Assert.Equal(log.Id, dto.Id);
        Assert.Equal("pacientes.crear", dto.Accion);
    }
}
