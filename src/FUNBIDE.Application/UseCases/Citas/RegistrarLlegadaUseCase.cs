using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IRegistrarLlegadaUseCase : IUseCase<RegistrarLlegadaRequest, CitaDto>
{
}

/// <summary>Check-in de Recepción: el paciente de una cita agendada llegó y pasa a la sala de espera.</summary>
public sealed class RegistrarLlegadaUseCase(ICitaRepository citaRepository) : IRegistrarLlegadaUseCase
{
    public async Task<CitaDto> EjecutarAsync(RegistrarLlegadaRequest request, CancellationToken cancellationToken)
    {
        var cita = await citaRepository.ObtenerPorIdAsync(request.CitaId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Cita), request.CitaId);

        cita.RegistrarLlegada();
        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(
            cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
            cita.Intervalo?.Inicio, cita.Intervalo?.Fin, cita.NotasCierre);
    }
}
