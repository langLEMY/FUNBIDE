using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IListarPendientesDeCobroUseCase : IUseCase<IReadOnlyList<CitaAgendaDto>>
{
}

/// <summary>
/// Consultas completadas sin cobro asociado: el aviso "doctor finalizó consulta — pasa a
/// caja". Sin acotar por fecha a propósito (ver <see cref="ICitaRepository.ObtenerPendientesDeCobroAsync"/>).
/// </summary>
public sealed class ListarPendientesDeCobroUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository,
    ICobroRepository cobroRepository) : IListarPendientesDeCobroUseCase
{
    public async Task<IReadOnlyList<CitaAgendaDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var citas = await citaRepository.ObtenerPendientesDeCobroAsync(cancellationToken);

        return await CitaAgendaMapper.EnriquecerAsync(citas, pacienteRepository, usuarioRepository, cobroRepository, cancellationToken);
    }
}
