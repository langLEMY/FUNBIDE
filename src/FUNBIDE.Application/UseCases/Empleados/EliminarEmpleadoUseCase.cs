using FUNBIDE.Application.Common;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Empleados;

public interface IEliminarEmpleadoUseCase : IUseCase<Guid, Guid>
{
}

/// <summary>
/// A diferencia de <c>EliminarUsuarioUseCase</c> (que desactiva sin borrar), este sí
/// borra la fila de verdad: <see cref="FUNBIDE.Domain.Entities.Empleado"/> no tiene
/// relevancia de auditoría clínica ni referencias históricas que preservar.
/// </summary>
public sealed class EliminarEmpleadoUseCase(IEmpleadoRepository empleadoRepository) : IEliminarEmpleadoUseCase
{
    public async Task<Guid> EjecutarAsync(Guid empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await empleadoRepository.ObtenerPorIdAsync(empleadoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Empleado), empleadoId);

        empleadoRepository.Eliminar(empleado);
        await empleadoRepository.GuardarCambiosAsync(cancellationToken);

        return empleadoId;
    }
}
