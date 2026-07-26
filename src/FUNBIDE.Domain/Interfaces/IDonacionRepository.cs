using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IDonacionRepository
{
    Task<IReadOnlyList<Donacion>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken);

    Task AgregarAsync(Donacion donacion, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
