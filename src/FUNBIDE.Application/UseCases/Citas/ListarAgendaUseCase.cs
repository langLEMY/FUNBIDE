using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IListarAgendaUseCase : IUseCase<ListarAgendaRequest, IReadOnlyList<CitaAgendaDto>>
{
}

/// <summary>Todas las citas (cualquier doctor), filtrables por fecha, doctor y/o estado — tabla de Agenda de Recepción.</summary>
public sealed class ListarAgendaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository,
    ICobroRepository cobroRepository) : IListarAgendaUseCase
{
    // Mismo patrón que ObtenerLogsAuditoriaUseCase: sin Pagina/TamanoPagina explícitos se
    // sigue devolviendo todo (comportamiento de hoy), con un techo duro razonable en vez
    // de un límite realista de "página" — Agenda sin fecha ni doctor puede legítimamente
    // querer ver bastante de una sola vez.
    private const int TamanoSinPaginacionExplicita = 10_000;
    private const int TamanoPaginaPorDefecto = 50;
    private const int TamanoPaginaMaximo = 100;

    public async Task<IReadOnlyList<CitaAgendaDto>> EjecutarAsync(ListarAgendaRequest request, CancellationToken cancellationToken)
    {
        int pagina;
        int tamanoPagina;
        if (request.Pagina is null && request.TamanoPagina is null)
        {
            pagina = 1;
            tamanoPagina = TamanoSinPaginacionExplicita;
        }
        else
        {
            pagina = Math.Clamp(request.Pagina ?? 1, 1, 1_000_000);
            tamanoPagina = request.TamanoPagina is null or < 1
                ? TamanoPaginaPorDefecto
                : Math.Min(request.TamanoPagina.Value, TamanoPaginaMaximo);
        }

        var (citas, _) = await citaRepository.ObtenerPaginadoAsync(
            request.Fecha, request.DoctorId, request.Estado, pagina, tamanoPagina, cancellationToken);

        return await CitaAgendaMapper.EnriquecerAsync(citas, pacienteRepository, usuarioRepository, cobroRepository, cancellationToken);
    }
}
