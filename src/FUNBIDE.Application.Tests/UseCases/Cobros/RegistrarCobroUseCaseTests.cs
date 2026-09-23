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
    private readonly IMovimientoFinancieroRepository _movimientoFinancieroRepository = Substitute.For<IMovimientoFinancieroRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly IResumenDiarioRepository _resumenDiarioRepository = Substitute.For<IResumenDiarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();
    private readonly INotificadorTiempoRealService _notificadorTiempoReal = Substitute.For<INotificadorTiempoRealService>();

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
        _usuarioRepository.ObtenerNombresPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());
    }

    private RegistrarCobroUseCase CrearCasoDeUso() => new(
        _cobroRepository, _turnoCajaRepository, _seguroMedicoRepository, _tarifarioRepository, _movimientoFinancieroRepository,
        _pacienteRepository, _usuarioRepository, _resumenDiarioRepository, _unitOfWork, _currentUser, _dateTimeProvider,
        _auditoriaLogService, _notificadorTiempoReal);

    private static RegistrarCobroRequest CrearRequest(Guid pacienteId, Guid? seguroMedicoId = null, decimal montoPagado = 1000m) =>
        new(pacienteId, null, "Consulta general", 1000m, PagoEfectivo(montoPagado), seguroMedicoId, seguroMedicoId is null ? null : "AUTH-1");

    [Fact]
    public async Task EjecutarAsync_SinCajaAbierta_LaAbreSolaConElFondoFijoYRegistraElCobro()
    {
        // La caja ya no se abre a mano: si no hay turno abierto, ObtenerAbiertoConBloqueoOAbrirAsync
        // (ver TurnoCajaRepository) lo abre solo con TurnoCaja.FondoFijo y lo devuelve, en
        // vez de que este caso de uso falle con "no hay caja abierta".
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);
        var request = CrearRequest(paciente.Id);

        await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        await _cobroRepository.Received(1).AgregarAsync(Arg.Any<Domain.Entities.Cobro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_CitaYaCobrada_LanzaCitaYaCobradaException()
    {
        var citaId = Guid.NewGuid();
        var paciente = CrearPaciente();
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
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
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var pacienteId = Guid.NewGuid();
        _pacienteRepository.ObtenerPorIdAsync(pacienteId, Arg.Any<CancellationToken>()).Returns((Paciente?)null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => CrearCasoDeUso().EjecutarAsync(CrearRequest(pacienteId), CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_SeguroDesactivado_LanzaInvalidOperationException()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("ARS Humano", 30m);
        seguro.Desactivar();
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(CrearRequest(paciente.Id, seguro.Id), CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_ConSeguroActivoSinTarifario_LanzaArgumentOutOfRangeException()
    {
        // El cálculo automático por % de cobertura está desactivado (ver
        // RegistrarCobroRequestValidator, que en producción rechaza esto antes de
        // llegar acá): sin TarifarioProcedimientoId, Cobro ya no tiene de dónde sacar
        // un monto de cobertura válido, ni siquiera con un seguro activo.
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("ARS Humano", 40m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 1000m, PagoEfectivo(600m), seguro.Id, "AUTH-1");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));

        await _cobroRepository.DidNotReceive().AgregarAsync(Arg.Any<Domain.Entities.Cobro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_Exitoso_AcumulaEnResumenDiarioSoloLoPagadoNoElTotal()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
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
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
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
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanAseguradora.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
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
    public async Task EjecutarAsync_ConTarifarioConFondo_CreaMovimientoFinancieroYLoAcumulaEnElResumen()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("Renacer", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanAseguradora.Estandar, "Consulta general", 500m, 100m, 600m, montoFondo: 250m);
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var resumen = new ResumenDiario(DateOnly.FromDateTime(DateTime.UtcNow));
        _resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(resumen);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta general", 600m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(250m, resultado.MontoFondo);
        // El fondo no es dinero de caja: MontoPagado (100) es lo único que suma a
        // DineroMovido antes del fondo aparte, pero el fondo también se reconoce como
        // ingreso de la fundación en el mismo resumen — total 100 (paciente) + 250 (fondo).
        Assert.Equal(350m, resumen.DineroMovido);
        await _movimientoFinancieroRepository.Received(1).RegistrarAsync(
            Arg.Is<Domain.Entities.MovimientoFinanciero>(m =>
                m.Monto == 250m && m.Tipo == TipoMovimientoFinanciero.Ingreso && m.TurnoCajaId != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ConTarifarioSinFondo_NoCreaMovimientoFinanciero()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanAseguradora.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta odontológica general", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Null(resultado.MontoFondo);
        await _movimientoFinancieroRepository.DidNotReceive().RegistrarAsync(
            Arg.Any<Domain.Entities.MovimientoFinanciero>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioDeOtraAseguradora_LanzaInvalidOperationException()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            Guid.NewGuid(), PlanAseguradora.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioDesactivado_LanzaInvalidOperationException()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var seguro = new SeguroMedico("SENASA", 50m);
        _seguroMedicoRepository.ObtenerPorIdAsync(seguro.Id, Arg.Any<CancellationToken>()).Returns(seguro);

        var tarifario = new TarifarioProcedimiento(
            seguro.Id, PlanAseguradora.Contributivo, "Consulta odontológica general", 690m, 100m, 790m);
        tarifario.Desactivar();
        _tarifarioRepository.ObtenerPorIdAsync(tarifario.Id, Arg.Any<CancellationToken>()).Returns(tarifario);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta", 790m, PagoEfectivo(100m), seguro.Id, "AUTH-1", tarifario.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_ConDoctor_DevuelveDoctorIdYNombreResueltoEnElDto()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var doctorId = Guid.NewGuid();
        _usuarioRepository.ObtenerNombresPorIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(doctorId)), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [doctorId] = "Dr. Juan Gómez" });

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta general", 1000m, PagoEfectivo(1000m), null, null, DoctorId: doctorId);

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(doctorId, resultado.DoctorId);
        Assert.Equal("Dr. Juan Gómez", resultado.DoctorNombre);
    }

    [Fact]
    public async Task EjecutarAsync_TarifarioNoExiste_LanzaRecursoNoEncontradoException()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
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

    [Fact]
    public async Task EjecutarAsync_ClaveIdempotenciaYaUsada_DevuelveElCobroExistenteSinAbrirLaCajaNiCrearOtro()
    {
        // Simula un doble-click en "Registrar" o un reintento de red: la segunda petición
        // llega con la misma clave que la primera, que ya se procesó. No debe competir por
        // el bloqueo de la caja ni crear un segundo Cobro -- eso sería el propio bug que
        // esto previene.
        var paciente = CrearPaciente();
        var cobroExistente = new Cobro(
            paciente.Id, null, Guid.NewGuid(), Guid.NewGuid(), "Consulta general", 1000m,
            [new PagoRecibido(MetodoPago.Efectivo, 1000m)], claveIdempotencia: "clave-doble-submit-1");
        _cobroRepository
            .ObtenerPorClaveIdempotenciaAsync("clave-doble-submit-1", Arg.Any<CancellationToken>())
            .Returns(cobroExistente);
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta general", 1000m, PagoEfectivo(1000m), null, null,
            ClaveIdempotencia: "clave-doble-submit-1");

        var resultado = await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        Assert.Equal(cobroExistente.Id, resultado.Id);
        await _turnoCajaRepository.DidNotReceive().ObtenerAbiertoConBloqueoOAbrirAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _cobroRepository.DidNotReceive().AgregarAsync(Arg.Any<Cobro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ClaveIdempotenciaNueva_LaGuardaEnElCobroCreado()
    {
        _turnoCajaRepository
            .ObtenerAbiertoConBloqueoOAbrirAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(CrearTurnoAbierto());
        var paciente = CrearPaciente();
        _pacienteRepository.ObtenerPorIdAsync(paciente.Id, Arg.Any<CancellationToken>()).Returns(paciente);
        _cobroRepository
            .ObtenerPorClaveIdempotenciaAsync("clave-nueva", Arg.Any<CancellationToken>())
            .Returns((Cobro?)null);

        var request = new RegistrarCobroRequest(
            paciente.Id, null, "Consulta general", 1000m, PagoEfectivo(1000m), null, null,
            ClaveIdempotencia: "clave-nueva");

        await CrearCasoDeUso().EjecutarAsync(request, CancellationToken.None);

        await _cobroRepository.Received(1).AgregarAsync(
            Arg.Is<Cobro>(c => c.ClaveIdempotencia == "clave-nueva"), Arg.Any<CancellationToken>());
    }
}
