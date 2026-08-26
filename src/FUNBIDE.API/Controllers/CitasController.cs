using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.UseCases.Citas;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Citas médicas. El flujo Pendiente→Programada→Completada (crear/programar/completar/
/// pendientes-programadas-completadas) es del Doctor sobre sus propias citas. Las
/// acciones de Agenda/Recepción/Caja (agendar, registrar-llegada, llegada-directa,
/// cancelar, agenda, sala-espera, pendientes-de-cobro) son de Fondos y no están acotadas
/// a un solo doctor. Cada acción declara su propio <see cref="RequiereRolAttribute"/> en
/// vez de uno a nivel de clase, igual que <c>PacientesController</c>.
/// </summary>
[ApiController]
[Route("api/citas")]
[Authorize]
public sealed class CitasController(
    IObtenerCitasPorEstadoUseCase obtenerPorEstado,
    ICrearCitaUseCase crearCita,
    IProgramarCitaUseCase programarCita,
    ICompletarCitaUseCase completarCita,
    ICancelarCitaUseCase cancelarCita,
    IAgendarCitaUseCase agendarCita,
    IRegistrarLlegadaUseCase registrarLlegada,
    IRegistrarLlegadaSinCitaUseCase registrarLlegadaSinCita,
    IListarAgendaUseCase listarAgenda,
    IListarSalaDeEsperaUseCase listarSalaDeEspera,
    IListarPendientesDeCobroUseCase listarPendientesDeCobro,
    IListarPacientesDelDoctorUseCase listarPacientesDelDoctor) : ControllerBase
{
    [HttpGet("pendientes")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerPendientesAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Pendiente, cancellationToken));

    [HttpGet("programadas")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerProgramadasAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Programada, cancellationToken));

    [HttpGet("completadas")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerCompletadasAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Completada, cancellationToken));

    /// <summary>
    /// Citas del doctor cuyo paciente ya llegó (recepción llamó a registrar-llegada) y
    /// está en sala de espera. Sin esto, una cita que pasa de Programada a EnEspera
    /// desaparece de las tres pestañas de arriba y el doctor no tiene forma de
    /// encontrarla para completarla.
    /// </summary>
    [HttpGet("en-espera")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerEnEsperaAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.EnEspera, cancellationToken));

    /// <summary>Pacientes que alguna vez tuvieron una cita con el doctor autenticado, para su Dashboard.</summary>
    [HttpGet("pacientes")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<IReadOnlyList<PacienteDelDoctorDto>>> ObtenerPacientesAsync(CancellationToken cancellationToken) =>
        Ok(await listarPacientesDelDoctor.EjecutarAsync(cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<CitaDto>> CrearAsync(CrearCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await crearCita.EjecutarAsync(request, cancellationToken);
        return Created("api/citas/pendientes", cita);
    }

    [HttpPatch("programar")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<CitaDto>> ProgramarAsync(ProgramarCitaRequest request, CancellationToken cancellationToken) =>
        Ok(await programarCita.EjecutarAsync(request, cancellationToken));

    [HttpPatch("completar")]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<CitaDto>> CompletarAsync(CompletarCitaRequest request, CancellationToken cancellationToken) =>
        Ok(await completarCita.EjecutarAsync(request, cancellationToken));

    [HttpPatch("cancelar")]
    [RequiereRol(RolUsuario.Doctor, RolUsuario.Fondos)]
    [RequierePermiso(ModuloPermiso.Agenda)]
    public async Task<ActionResult<CitaDto>> CancelarAsync(CancelarCitaRequest request, CancellationToken cancellationToken) =>
        Ok(await cancelarCita.EjecutarAsync(request, cancellationToken));

    [HttpPost("agendar")]
    [RequiereRol(RolUsuario.Fondos)]
    [RequierePermiso(ModuloPermiso.Agenda)]
    public async Task<ActionResult<CitaDto>> AgendarAsync(AgendarCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await agendarCita.EjecutarAsync(request, cancellationToken);
        return Created("api/citas/agenda", cita);
    }

    [HttpPatch("registrar-llegada")]
    [RequiereRol(RolUsuario.Fondos)]
    [RequierePermiso(ModuloPermiso.Recepcion)]
    public async Task<ActionResult<CitaDto>> RegistrarLlegadaAsync(
        RegistrarLlegadaRequest request, CancellationToken cancellationToken) =>
        Ok(await registrarLlegada.EjecutarAsync(request, cancellationToken));

    [HttpPost("llegada-directa")]
    [RequiereRol(RolUsuario.Fondos)]
    [RequierePermiso(ModuloPermiso.Recepcion)]
    public async Task<ActionResult<CitaDto>> RegistrarLlegadaSinCitaAsync(
        RegistrarLlegadaSinCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await registrarLlegadaSinCita.EjecutarAsync(request, cancellationToken);
        return Created("api/citas/sala-espera", cita);
    }

    [HttpGet("agenda")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Agenda, ModuloPermiso.Operaciones)]
    public async Task<ActionResult<IReadOnlyList<CitaAgendaDto>>> ObtenerAgendaAsync(
        [FromQuery] DateOnly? fecha, [FromQuery] Guid? doctorId, CancellationToken cancellationToken) =>
        Ok(await listarAgenda.EjecutarAsync(new ListarAgendaRequest(fecha, doctorId), cancellationToken));

    [HttpGet("sala-espera")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Recepcion, ModuloPermiso.Operaciones)]
    public async Task<ActionResult<IReadOnlyList<CitaAgendaDto>>> ObtenerSalaDeEsperaAsync(CancellationToken cancellationToken) =>
        Ok(await listarSalaDeEspera.EjecutarAsync(cancellationToken));

    [HttpGet("pendientes-de-cobro")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Cobros)]
    public async Task<ActionResult<IReadOnlyList<CitaAgendaDto>>> ObtenerPendientesDeCobroAsync(CancellationToken cancellationToken) =>
        Ok(await listarPendientesDeCobro.EjecutarAsync(cancellationToken));
}
