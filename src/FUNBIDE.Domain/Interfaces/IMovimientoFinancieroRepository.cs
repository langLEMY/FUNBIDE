using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IMovimientoFinancieroRepository
{
    Task RegistrarAsync(MovimientoFinanciero movimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
