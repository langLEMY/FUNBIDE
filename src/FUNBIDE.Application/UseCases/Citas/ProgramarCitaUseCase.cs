using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IProgramarCitaUseCase : IUseCase<ProgramarCitaRequest, CitaDto>
{
}

public sealed class ProgramarCitaUseCase(
    ICitaRepository citaRepository,
    ICurrentUserService currentUser) : IProgramarCitaUseCase
{
    public async Task<CitaDto> EjecutarAsync(ProgramarCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await citaRepository.ObtenerPorIdAsync(request.CitaId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Cita), request.CitaId);

        if (cita.DoctorId != currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException($"La cita '{cita.Id}' no pertenece al doctor autenticado.");
        }

        var hayChoque = await citaRepository.TieneChoqueDeHorarioAsync(
            cita.DoctorId, request.Inicio, request.Fin, excluirCitaId: cita.Id, cancellationToken);
        if (hayChoque)
        {
            throw new HorarioNoDisponibleException(cita.DoctorId, request.Inicio, request.Fin);
        }

        var intervalo = IntervaloCita.Crear(request.Inicio, request.Fin);
        cita.Programar(intervalo);

        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(
            cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
            cita.Intervalo!.Inicio, cita.Intervalo.Fin, cita.NotasCierre);
    }
}
