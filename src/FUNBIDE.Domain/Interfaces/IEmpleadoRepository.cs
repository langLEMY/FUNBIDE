using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IEmpleadoRepository
{
    Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken);

    void Eliminar(Empleado empleado);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
