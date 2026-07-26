using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public sealed record ListarTurnosCajaRequest(DateTimeOffset Desde, DateTimeOffset Hasta);

public interface IListarTurnosCajaUseCase : IUseCase<ListarTurnosCajaRequest, IReadOnlyList<TurnoCajaAdminDto>>
{
}

/// <summary>
/// Historial de turnos de caja (apertura/cierre, montos, diferencia de arqueo) para la
/// vista de supervisión de Admin — Admin no puede abrir/cerrar turnos (eso sigue siendo
/// exclusivo de Fondos), solo ver qué pasó.
/// </summary>
public sealed class ListarTurnosCajaUseCase(
    ITurnoCajaRepository turnoCajaRepository,
    IUsuarioRepository usuarioRepository) : IListarTurnosCajaUseCase
{
    public async Task<IReadOnlyList<TurnoCajaAdminDto>> EjecutarAsync(
        ListarTurnosCajaRequest request, CancellationToken cancellationToken)
    {
        var turnos = await turnoCajaRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);

        var idsUsuarios = turnos
            .Select(t => t.UsuarioAperturaId)
            .Concat(turnos.Where(t => t.UsuarioCierreId.HasValue).Select(t => t.UsuarioCierreId!.Value))
            .Distinct()
            .ToList();
        var nombres = await usuarioRepository.ObtenerNombresPorIdsAsync(idsUsuarios, cancellationToken);

        return turnos
            .Select(t => new TurnoCajaAdminDto(
                t.Id,
                nombres.GetValueOrDefault(t.UsuarioAperturaId, "—"),
                t.MontoInicial,
                t.AbiertoEn,
                t.Estado,
                t.UsuarioCierreId.HasValue ? nombres.GetValueOrDefault(t.UsuarioCierreId.Value, "—") : null,
                t.MontoFinalContado,
                t.MontoEsperado,
                t.Diferencia,
                t.Notas,
                t.CerradoEn))
            .ToList();
    }
}
