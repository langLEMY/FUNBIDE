using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Finanzas;

public interface IListarMovimientosFinancierosUseCase : IUseCase<Guid?, IReadOnlyList<MovimientoFinancieroDto>>
{
}

/// <summary>
/// Lista movimientos financieros; si se pasa <paramref name="turnoCajaId"/> (ver
/// <see cref="EjecutarAsync"/>) los acota al turno actual, para el panel de Caja.
/// </summary>
public sealed class ListarMovimientosFinancierosUseCase(
    IMovimientoFinancieroRepository movimientoRepository) : IListarMovimientosFinancierosUseCase
{
    public async Task<IReadOnlyList<MovimientoFinancieroDto>> EjecutarAsync(Guid? turnoCajaId, CancellationToken cancellationToken)
    {
        var movimientos = turnoCajaId.HasValue
            ? await movimientoRepository.ObtenerPorTurnoAsync(turnoCajaId.Value, cancellationToken)
            : await movimientoRepository.ObtenerTodosAsync(cancellationToken);

        return movimientos
            .Select(m => new MovimientoFinancieroDto(m.Id, m.Tipo, m.Monto, m.Concepto, m.CitaId, m.TurnoCajaId, m.RegistradoEn))
            .ToList();
    }
}
