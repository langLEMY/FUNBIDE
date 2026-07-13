using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IListarAgendaUseCase : IUseCase<ListarAgendaRequest, IReadOnlyList<CitaAgendaDto>>
{
}

/// <summary>Todas las citas (cualquier doctor), filtrables por fecha y/o doctor — tabla de Agenda de Recepción.</summary>
public sealed class ListarAgendaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository,
    ICobroRepository cobroRepository) : IListarAgendaUseCase
{
    public async Task<IReadOnlyList<CitaAgendaDto>> EjecutarAsync(ListarAgendaRequest request, CancellationToken cancellationToken)
    {
        var citas = await citaRepository.ObtenerPorFiltroAsync(request.Fecha, request.DoctorId, cancellationToken);

        return await CitaAgendaMapper.EnriquecerAsync(citas, pacienteRepository, usuarioRepository, cobroRepository, cancellationToken);
    }
}
