using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface ITurnoCajaRepository
{
    Task<TurnoCaja?> ObtenerAbiertoAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Igual que <see cref="ObtenerAbiertoAsync"/> pero toma un bloqueo de fila (SELECT ...
    /// FOR UPDATE). Usado por <c>RegistrarCobroUseCase</c> y <c>CerrarTurnoCajaUseCase</c>
    /// para serializarse entre sí: sin este bloqueo compartido, un cobro registrado justo
    /// mientras se está cerrando la caja podía quedar fuera del cálculo de "efectivo
    /// esperado" del arqueo (el cobro y el cierre leían el turno por separado, sin
    /// contención real entre ambos).
    /// </summary>
    Task<TurnoCaja?> ObtenerAbiertoConBloqueoAsync(CancellationToken cancellationToken);

    Task<TurnoCaja?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TurnoCaja>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken);

    Task AgregarAsync(TurnoCaja turno, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
