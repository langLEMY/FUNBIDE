using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface ICancelarCitaUseCase : IUseCase<CancelarCitaRequest, CitaDto>
{
}

/// <summary>
/// Cancela una cita. El endpoint acepta tanto a Fondos (Recepción, cancela cualquier cita
/// de cualquier doctor) como a Doctor (solo puede cancelar sus propias citas) — a
/// diferencia de <see cref="ProgramarCitaUseCase"/>/<see cref="CompletarCitaUseCase"/>, que
/// son exclusivamente del Doctor, acá el chequeo de dueño solo aplica cuando quien llama
/// tiene rol Doctor.
/// </summary>
public sealed class CancelarCitaUseCase(
    ICitaRepository citaRepository, ICurrentUserService currentUser) : ICancelarCitaUseCase
{
    public async Task<CitaDto> EjecutarAsync(CancelarCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await citaRepository.ObtenerPorIdAsync(request.CitaId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Cita), request.CitaId);

        if (currentUser.Rol == RolUsuario.Doctor && cita.DoctorId != currentUser.UsuarioId)
        {
            throw new OperacionNoPermitidaException($"La cita '{cita.Id}' no pertenece al doctor autenticado.");
        }

        cita.Cancelar();
        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(
            cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
            cita.Intervalo?.Inicio, cita.Intervalo?.Fin, cita.NotasCierre);
    }
}
