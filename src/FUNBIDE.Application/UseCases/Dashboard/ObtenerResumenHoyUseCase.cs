using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Dashboard;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Dashboard;

public interface IObtenerResumenHoyUseCase : IUseCase<ResumenDiarioDto>
{
}

/// <summary>Alimenta las tarjetas "Pacientes atendidos hoy" / "Dinero movido hoy" del panel de ADMIN.</summary>
public sealed class ObtenerResumenHoyUseCase(
    IResumenDiarioRepository resumenDiarioRepository,
    IDateTimeProvider dateTimeProvider) : IObtenerResumenHoyUseCase
{
    public async Task<ResumenDiarioDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
        var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
        var resumen = await resumenDiarioRepository.ObtenerPorFechaAsync(hoy, cancellationToken);

        return resumen is null
            ? new ResumenDiarioDto(hoy, 0, 0m, 0m, 0m, 0m)
            : new ResumenDiarioDto(
                resumen.Fecha, resumen.PacientesAtendidos, resumen.DineroMovido,
                resumen.DineroEfectivo, resumen.DineroTarjeta, resumen.DineroTransferencia);
    }
}
