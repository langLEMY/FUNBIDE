using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Cobros;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Cobros;

public interface IObtenerDeudaPacienteUseCase : IUseCase<Guid, DeudaPacienteDto>
{
}

/// <summary>Suma de saldos pendientes del paciente, para el aviso de "deuda anterior" en Cobros/Recepción.</summary>
public sealed class ObtenerDeudaPacienteUseCase(ICobroRepository cobroRepository) : IObtenerDeudaPacienteUseCase
{
    public async Task<DeudaPacienteDto> EjecutarAsync(Guid pacienteId, CancellationToken cancellationToken)
    {
        var deuda = await cobroRepository.ObtenerDeudaTotalPorPacienteAsync(pacienteId, cancellationToken);
        return new DeudaPacienteDto(pacienteId, deuda);
    }
}
