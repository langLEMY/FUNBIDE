using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IMovimientoFinancieroRepository
{
    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorTurnoAsync(Guid turnoCajaId, CancellationToken cancellationToken);

    /// <summary>Para el panel de Finanzas de Admin: todos los movimientos entre dos fechas (incluye Ingreso y Egreso, de cualquier turno).</summary>
    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken);

    Task RegistrarAsync(MovimientoFinanciero movimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
