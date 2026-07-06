using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IMovimientoFinancieroRepository
{
    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task RegistrarAsync(MovimientoFinanciero movimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
