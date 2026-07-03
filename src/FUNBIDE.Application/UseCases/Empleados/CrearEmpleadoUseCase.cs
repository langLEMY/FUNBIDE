using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Empleados;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Empleados;

public interface ICrearEmpleadoUseCase : IUseCase<CrearEmpleadoRequest, EmpleadoDto>
{
}

public sealed class CrearEmpleadoUseCase(IEmpleadoRepository empleadoRepository) : ICrearEmpleadoUseCase
{
    public async Task<EmpleadoDto> EjecutarAsync(CrearEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var empleado = new Empleado(request.NombreCompleto, request.Cargo);

        await empleadoRepository.AgregarAsync(empleado, cancellationToken);
        await empleadoRepository.GuardarCambiosAsync(cancellationToken);

        return new EmpleadoDto(empleado.Id, empleado.NombreCompleto, empleado.Cargo);
    }
}
