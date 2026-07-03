using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.HistorialClinico;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.HistorialClinico;

public interface IObtenerHistorialPorPacienteUseCase : IUseCase<Guid, IReadOnlyList<EntradaHistorialDto>>
{
}

public sealed class ObtenerHistorialPorPacienteUseCase(
    IHistorialClinicoRepository historialRepository) : IObtenerHistorialPorPacienteUseCase
{
    public async Task<IReadOnlyList<EntradaHistorialDto>> EjecutarAsync(
        Guid pacienteId, CancellationToken cancellationToken)
    {
        var entradas = await historialRepository.ObtenerPorPacienteAsync(pacienteId, cancellationToken);

        return entradas
            .OrderByDescending(e => e.RegistradoEn)
            .Select(e => new EntradaHistorialDto(
                e.Id, e.PacienteId, e.DoctorId, e.CitaId, e.Contenido.Valor, e.RegistradoEn))
            .ToList();
    }
}
