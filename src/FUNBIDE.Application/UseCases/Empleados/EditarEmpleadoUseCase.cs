using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Empleados;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Empleados;

public interface IEditarEmpleadoUseCase : IUseCase<EditarEmpleadoRequest, EmpleadoDto>
{
}

public sealed class EditarEmpleadoUseCase(IEmpleadoRepository empleadoRepository) : IEditarEmpleadoUseCase
{
    public async Task<EmpleadoDto> EjecutarAsync(EditarEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var empleado = await empleadoRepository.ObtenerPorIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Empleado), request.EmpleadoId);

        empleado.ActualizarDatos(request.NombreCompleto, request.Cargo);
        await empleadoRepository.GuardarCambiosAsync(cancellationToken);

        return new EmpleadoDto(empleado.Id, empleado.NombreCompleto, empleado.Cargo);
    }
}
