using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Finanzas;

public interface IListarMovimientosFinancierosUseCase : IUseCase<IReadOnlyList<MovimientoFinancieroDto>>
{
}

public sealed class ListarMovimientosFinancierosUseCase(
    IMovimientoFinancieroRepository movimientoRepository) : IListarMovimientosFinancierosUseCase
{
    public async Task<IReadOnlyList<MovimientoFinancieroDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var movimientos = await movimientoRepository.ObtenerTodosAsync(cancellationToken);

        return movimientos
            .Select(m => new MovimientoFinancieroDto(m.Id, m.Tipo, m.Monto, m.Concepto, m.CitaId, m.RegistradoEn))
            .ToList();
    }
}
