using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IServicioRepository
{
    Task<IReadOnlyList<Servicio>> ObtenerTodosAsync(bool incluirInactivos, CancellationToken cancellationToken);

    Task<Servicio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Servicio?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken);

    /// <summary>Con seguimiento de cambios, para que <c>ImportarServiciosUseCase</c> reconcilie por código dentro de la misma unidad de trabajo.</summary>
    Task<IReadOnlyList<Servicio>> ObtenerTodosParaImportarAsync(CancellationToken cancellationToken);

    Task AgregarAsync(Servicio servicio, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
