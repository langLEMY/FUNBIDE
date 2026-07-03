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
            var hoy = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.RegistrarPacienteAtendido();
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            return new CitaDto(
                cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
                cita.Intervalo?.Inicio, cita.Intervalo?.Fin, cita.NotasCierre);
        }, cancellationToken);
    }
}
