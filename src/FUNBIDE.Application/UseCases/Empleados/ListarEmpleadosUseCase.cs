using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Empleados;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Empleados;

public interface IListarEmpleadosUseCase : IUseCase<IReadOnlyList<EmpleadoDto>>
{
}

public sealed class ListarEmpleadosUseCase(IEmpleadoRepository empleadoRepository) : IListarEmpleadosUseCase
{
    public async Task<IReadOnlyList<EmpleadoDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var empleados = await empleadoRepository.ObtenerTodosAsync(cancellationToken);

        return empleados
            .Select(e => new EmpleadoDto(e.Id, e.NombreCompleto, e.Cargo))
            .ToList();
    }
}
