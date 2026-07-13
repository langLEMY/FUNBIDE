using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IListarSalaDeEsperaUseCase : IUseCase<IReadOnlyList<CitaAgendaDto>>
{
}

/// <summary>Citas de hoy en Programada o EnEspera — tabla de Recepción y badge del sidebar.</summary>
public sealed class ListarSalaDeEsperaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository,
    ICobroRepository cobroRepository,
    IDateTimeProvider dateTimeProvider) : IListarSalaDeEsperaUseCase
{
    public async Task<IReadOnlyList<CitaAgendaDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
        var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
        var citas = await citaRepository.ObtenerSalaDeEsperaAsync(hoy, cancellationToken);

        return await CitaAgendaMapper.EnriquecerAsync(citas, pacienteRepository, usuarioRepository, cobroRepository, cancellationToken);
    }
}
