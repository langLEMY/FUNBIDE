using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IResumenDiarioRepository
{
    /// <summary>
    /// Obtiene el resumen de <paramref name="fecha"/> con bloqueo de fila (SELECT ... FOR
    /// UPDATE) si ya existe, o crea uno nuevo en memoria si es el primer evento del día.
    /// Debe invocarse dentro de una transacción abierta por <see cref="IUnitOfWork"/> para
    /// que el bloqueo serialice acumulaciones concurrentes sobre el mismo día.
    /// </summary>
    Task<ResumenDiario> ObtenerOCrearConBloqueoAsync(DateOnly fecha, CancellationToken cancellationToken);

    Task<ResumenDiario?> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancellationToken);

    Task<IReadOnlyList<ResumenDiario>> ObtenerPorMesAsync(int anio, int mes, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
