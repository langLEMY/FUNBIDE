using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Empleados;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Empleados;

public interface IImportarEmpleadosUseCase : IUseCase<Stream, ImportarEmpleadosResultDto>
{
}

/// <summary>
/// Importa el directorio de personal desde un Excel de un sistema anterior. Sin
/// restricción de duplicados: a diferencia de pacientes, <see cref="Empleado"/> no
/// tiene una clave única de negocio.
/// </summary>
public sealed class ImportarEmpleadosUseCase(
    IExcelLectorService excelLector,
    IEmpleadoRepository empleadoRepository) : IImportarEmpleadosUseCase
{
    public async Task<ImportarEmpleadosResultDto> EjecutarAsync(Stream request, CancellationToken cancellationToken)
    {
        var filas = excelLector.LeerFilas(request);

        var omisiones = new List<string>();
        var creados = 0;

        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];
            var numeroFila = i + 2;

            var nombreCompleto = fila.Buscar("Descripción", "Descripcion", "Nombre", "Nombre completo");
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                omisiones.Add($"Fila {numeroFila}: sin nombre, omitida.");
                continue;
            }

            var cargo = fila.Buscar("Clase", "Cargo");

            var empleado = new Empleado(nombreCompleto, cargo);
            await empleadoRepository.AgregarAsync(empleado, cancellationToken);
            creados++;
        }

        await empleadoRepository.GuardarCambiosAsync(cancellationToken);

        return new ImportarEmpleadosResultDto(filas.Count, creados, omisiones.Count, omisiones.Take(50).ToList());
    }
}
