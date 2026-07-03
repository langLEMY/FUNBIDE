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
        var hoy = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        var resumen = await resumenDiarioRepository.ObtenerPorFechaAsync(hoy, cancellationToken);

        return resumen is null
            ? new ResumenDiarioDto(hoy, 0, 0m)
            : new ResumenDiarioDto(resumen.Fecha, resumen.PacientesAtendidos, resumen.DineroMovido);
    }
}
