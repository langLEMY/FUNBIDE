using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Cobros;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Application.UseCases.Cobros;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Cobros;

public class RegistrarCobroUseCaseTests
{
    private readonly ICobroRepository _cobroRepository = Substitute.For<ICobroRepository>();
    private readonly ITurnoCajaRepository _turnoCajaRepository = Substitute.For<ITurnoCajaRepository>();
    private readonly ISeguroMedicoRepository _seguroMedicoRepository = Substitute.For<ISeguroMedicoRepository>();
    private readonly ITarifarioProcedimientoRepository _tarifarioRepository = Substitute.For<ITarifarioProcedimientoRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IResumenDiarioRepository _resumenDiarioRepository = Substitute.For<IResumenDiarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    private static Paciente CrearPaciente() =>
        new("Ana", "Pérez", DocumentoIdentidad.Crear("00112345678"), "8091234567");

    private static TurnoCaja CrearTurnoAbierto() => new(Guid.NewGuid(), 1000m, DateTimeOffset.UtcNow);

    private static IReadOnlyList<PagoDto> PagoEfectivo(decimal monto) =>
        monto > 0 ? [new PagoDto(MetodoPago.Efectivo, monto)] : [];

    public RegistrarCobroUseCaseTests()
    {
        _unitOfWork
            .EjecutarEnTransaccionAsync(Arg.Any<Func<CancellationToken, Task<CobroDto>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<CobroDto>>>()(CancellationToken.None));

        _currentUser.UsuarioId.Returns(Guid.NewGuid());
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
        _dateTimeProvider.ZonaHorariaClinica.Returns(TimeZoneInfo.Utc);
        _resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new ResumenDiario(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    private RegistrarCobroUseCase CrearCasoDeUso() => new(
        _cobroRepository, _turnoCajaRepository, _seguroMedicoRepository, _tarifarioRepository, _pacienteRepository,
        _resumenDiarioRepository, _unitOfWork, _currentUser, _dateTimeProvider, _auditoriaLogService);

    private static RegistrarCobroRequest CrearRequest(Guid pacienteId, Guid? seguroMedicoId = null, decimal montoPagado = 1000m) =>
        new(pacienteId, null, "Consulta general", 1000m, PagoEfectivo(montoPagado), seguroMedicoId, seguroMedicoId is null ? null : "AUTH-1");

    [Fact]
    public async Task EjecutarAsync_SinCajaAbierta_LanzaInvalidOperationExceptionYNoRegistraNada()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns((TurnoCaja?)null);
        var request = CrearRequest(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _cobroRepository.DidNotReceive().AgregarAsync(Arg.Any<Domain.Entities.Cobro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_CitaYaCobrada_LanzaCitaYaCobradaException()
    {
        var citaId = Guid.NewGuid();
        var paciente = CrearPaciente();
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);
        _cobroRepository.ExisteCobroParaCitaAsync(citaId, Arg.Any<CancellationToken>()).Returns(true);
        var request = new RegistrarCobroRequest(paciente.Id, citaId, "Consulta", 500m, PagoEfectivo(500m), null, null);

        await Assert.ThrowsAsync<CitaYaCobradaException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _cobroRepository.DidNotReceive().AgregarAsync(Arg.Any<Domain.Entities.Cobro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_PacienteNoExiste_LanzaRecursoNoEncontradoException()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var pacienteId = Guid.NewGuid();
        _pacienteRepository.ObtenerPorIdAsync(pacienteId, Arg.Any<CancellationToken>()).Returns((Paciente?)null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => CrearCasoDeUso().EjecutarAsync(CrearRequest(pacienteId), CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_SeguroDesactivado_LanzaInvalidOperationException()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("ARS Humano", 30m);
        seguro.Desactivar();
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(CrearRequest(paciente.Id, seguro.Id), CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_ConSeguroActivo_UsaElPorcentajeDelCatalogoNoDelCliente()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("ARS Humano", 40m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 1000m, PagoEfectivo(600m), seguro.Id, "AUTH-1");

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        // El 40% viene del catálogo (seguro.PorcentajeCobertura), la request no trae
        // ningún campo de porcentaje que el cliente pueda mandar.
        Assert.Equal(40m, resultado.PorcentajeCobertura);
        Assert.Equal(400m, resultado.MontoCobertura);
        Assert.Equal(600m, resultado.MontoACargoPaciente);
        Assert.Equal(0m, resultado.MontoPendiente);
    }

    [Fact]
    public async Task EjecutarAsync_Exitoso_AcumulaEnResumenDiarioSoloLoPagadoNoElTotal()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var resumen = new ResumenDiario(DateOnly.FromDateTime(DateTime.UtcNow));
        _resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(resumen);

        // MontoTotal 1000 pero MontoPagado 700 (con seguro cubriendo parte, o pago parcial):
        // el resumen del dashboard debe sumar 700, no 1000.
        var request = CrearRequest(paciente.Id, montoPagado: 700m);

        await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(700m, resumen.DineroMovido);
        Assert.Equal(700m, resumen.DineroEfectivo);
        await _cobroRepository.Received(1).AgregarAsync(Arg.Any<Domain.Entities.Cobro>(), Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "cobros.registrar", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 201, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConPagoDividido_SumaTodasLasLineasYLasDevuelveEnElDto()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var resumen = new ResumenDiario(DateOnly.FromDateTime(DateTime.UtcNow));
        _resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(resumen);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta general", 1000m,
            [new PagoDto(MetodoPago.Tarjeta, 300m), new PagoDto(MetodoPago.Efectivo, 200m)],
            null, null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(500m, resultado.MontoPagado);
        Assert.Equal(2, resultado.Pagos.Count);
        Assert.Equal(300m, resumen.DineroTarjeta);
        Assert.Equal(200m, resumen.DineroEfectivo);
        Assert.Equal(500m, resumen.DineroMovido);
    }

    [Fact]
    public async Task EjecutarAsync_ConTarifario_UsaMontosExactosDelTarifarioIgnorandoLoQueMandaElCliente()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanSenasa.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        // El cliente manda un montoTotal distinto (999) a propósito: debe ganar el del tarifario (790).
        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta odontológica general", 999m, PagoEfectivo(100m),
            seguro.Id, "AUTH-1", tarifario.Id);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(790m, resultado.MontoTotal);
        Assert.Equal(690m, resultado.MontoCobertura);
        Assert.Equal(100m, resultado.MontoACargoPaciente);
        Assert.Null(resultado.PorcentajeCobertura);
        Assert.Equal(tarifario.Id, resultado.TarifarioProcedimientoId);
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioDeOtraAseguradora_LanzaInvalidOperationException()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            Guid.NewGuid(), PlanSenasa.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioDesactivado_LanzaInvalidOperationException()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanSenasa.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        tarifario.Desactivar();
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioNoExiste_LanzaRecursoNoEncontradoException()
    {
        _turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(Arg.Any<CancellationToken>()).Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);
        _tarifarioRepository.ObtenerPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TarifarioProcedimiento?)null);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", Guid.NewGuid());

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }
}
