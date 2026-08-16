using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface ICompletarCitaUseCase : IUseCase<CompletarCitaRequest, CitaDto>
{
}

public sealed class CompletarCitaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    ICurrentUserService currentUser,
    IResumenDiarioRepository resumenDiarioRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICompletarCitaUseCase
{
    public Task<CitaDto> EjecutarAsync(CompletarCitaRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var cita = await citaRepository.ObtenerPorIdAsync(request.CitaId, ct)
                ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Cita), request.CitaId);

            if (cita.DoctorId != currentUser.UsuarioId)
            {
                throw new OperacionNoPermitidaException($"La cita '{cita.Id}' no pertenece al doctor autenticado.");
            }

            cita.Completar(request.NotasCierre);
            await citaRepository.GuardarCambiosAsync(ct);

            // Una cita completada cuenta como un paciente atendido ese día para el
            // dashboard de ADMIN (tarjeta "Pacientes atendidos hoy" y gráfico "Vista del mes").
            var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
            var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.RegistrarPacienteAtendido();
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            // Deja "última visita" del paciente al día de la cita completada, en vez de
            // depender de que alguien lo actualice a mano.
            var paciente = await pacienteRepository.ObtenerPorIdAsync(cita.PacienteId, ct);
            if (paciente is not null)
            {
                paciente.RegistrarVisita(hoy);
                await pacienteRepository.GuardarCambiosAsync(ct);
            }

            return new CitaDto(
                cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
                cita.Intervalo?.Inicio, cita.Intervalo?.Fin, cita.NotasCierre);
        }, cancellationToken);
    }
}
