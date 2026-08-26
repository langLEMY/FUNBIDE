using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Servicios;

public interface IImportarServiciosUseCase : IUseCase<Stream, ImportarServiciosResultDto>
{
}

/// <summary>
/// Importa el catálogo de precios privados desde un Excel de un sistema contable anterior
/// (columnas Código/Descripción/Precio 1/Precio 2/Precio 3, ver <c>ExcelLectorService</c>).
/// Reconcilia por código igual que <c>ImportarInventarioUseCase</c>. Omite filas donde las
/// tres tarifas están en blanco o en cero — el archivo de origen mezcla servicios
/// facturables con categorías de gasto interno (alquileres, nómina, etc.) que siempre
/// traen precio 0 y no son un servicio que se le cobre a un paciente.
/// </summary>
public sealed class ImportarServiciosUseCase(
    IExcelLectorService excelLector,
    IServicioRepository servicioRepository) : IImportarServiciosUseCase
{
    public async Task<ImportarServiciosResultDto> EjecutarAsync(Stream request, CancellationToken cancellationToken)
    {
        var filas = excelLector.LeerFilas(request);

        var existentes = await servicioRepository.ObtenerTodosParaImportarAsync(cancellationToken);
        var porCodigo = new Dictionary<string, Servicio>(StringComparer.OrdinalIgnoreCase);
        foreach (var servicio in existentes)
        {
            porCodigo.TryAdd(servicio.Codigo, servicio);
        }

        var omisiones = new List<string>();
        var creados = 0;
        var actualizados = 0;

        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];
            var numeroFila = i + 2; // +1 por índice base 1, +1 por la fila de encabezado

            var codigo = fila.Buscar("Código", "Codigo", "Cod")?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                omisiones.Add($"Fila {numeroFila}: sin código, omitida.");
                continue;
            }

            var nombre = fila.Buscar("Nombre", "Descripción", "Descripcion", "Servicio");
            if (string.IsNullOrWhiteSpace(nombre))
            {
                omisiones.Add($"Fila {numeroFila} ({codigo}): sin nombre, omitida.");
                continue;
            }

            var precio1 = LeerMonto(fila.Buscar("Precio 1", "Precio1")) ?? 0m;
            var precio2 = LeerMonto(fila.Buscar("Precio 2", "Precio2")) ?? 0m;
            var precio3 = LeerMonto(fila.Buscar("Precio 3", "Precio3")) ?? 0m;

            if (precio1 == 0m && precio2 == 0m && precio3 == 0m)
            {
                omisiones.Add($"Fila {numeroFila} ({nombre}): las 3 tarifas están en cero o en blanco, no es un servicio facturable, omitida.");
                continue;
            }

            if (porCodigo.TryGetValue(codigo, out var servicioExistente))
            {
                servicioExistente.ActualizarPrecios(nombre, precio1, precio2, precio3);
                actualizados++;
                continue;
            }

            var nuevoServicio = new Servicio(codigo, nombre, precio1, precio2, precio3);
            await servicioRepository.AgregarAsync(nuevoServicio, cancellationToken);
            porCodigo[codigo] = nuevoServicio;
            creados++;
        }

        await servicioRepository.GuardarCambiosAsync(cancellationToken);

        return new ImportarServiciosResultDto(filas.Count, creados, actualizados, omisiones.Count, omisiones.Take(50).ToList());
    }

    private static decimal? LeerMonto(string? texto) =>
        texto is not null && decimal.TryParse(texto, out var monto) && monto >= 0 ? monto : null;
}
