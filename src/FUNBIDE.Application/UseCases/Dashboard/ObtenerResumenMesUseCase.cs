using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Dashboard;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Dashboard;

public interface IObtenerResumenMesUseCase : IUseCase<IReadOnlyList<ResumenDiarioDto>>
{
}

/// <summary>
/// Serie diaria del mes calendario en curso para el gráfico "Vista del mes". Al
/// filtrar siempre por el mes actual, el gráfico "se reinicia" solo el día 1 de cada
/// mes sin borrar nada: los meses anteriores quedan en el historial, simplemente no
/// se incluyen en esta consulta.
/// </summary>
public sealed class ObtenerResumenMesUseCase(
    IResumenDiarioRepository resumenDiarioRepository,
    IDateTimeProvider dateTimeProvider) : IObtenerResumenMesUseCase
{
    public async Task<IReadOnlyList<ResumenDiarioDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
        var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
        var resumenes = await resumenDiarioRepository.ObtenerPorMesAsync(hoy.Year, hoy.Month, cancellationToken);

        return resumenes
            .Select(r => new ResumenDiarioDto(
                r.Fecha, r.PacientesAtendidos, r.DineroMovido, r.DineroEfectivo, r.DineroTarjeta, r.DineroTransferencia))
            .ToList();
    }
}
