using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface ITurnoCajaRepository
{
    Task<TurnoCaja?> ObtenerAbiertoAsync(CancellationToken cancellationToken);

    Task<TurnoCaja?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TurnoCaja>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken);

    Task AgregarAsync(TurnoCaja turno, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
