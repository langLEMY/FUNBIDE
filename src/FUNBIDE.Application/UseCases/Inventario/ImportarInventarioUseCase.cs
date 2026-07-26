using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IImportarInventarioUseCase : IUseCase<Stream, ImportarInventarioResultDto>
{
}

/// <summary>
/// Importa ítems de inventario desde un Excel de un sistema anterior o un conteo físico.
/// Cada fila se reconcilia por código (normalizado) contra los ítems ya existentes: si hay
/// coincidencia, se actualizan nombre/stock mínimo/stock actual (igual que un ajuste manual
/// tras un conteo, ver <see cref="InventarioItem.AjustarStock"/>) en vez de crear un ítem
/// nuevo — así reimportar el mismo archivo (por ejemplo, un conteo periódico) no duplica
/// ítems con el mismo código.
/// </summary>
public sealed class ImportarInventarioUseCase(
    IExcelLectorService excelLector,
    IInventarioRepository inventarioRepository) : IImportarInventarioUseCase
{
    public async Task<ImportarInventarioResultDto> EjecutarAsync(Stream request, CancellationToken cancellationToken)
    {
        var filas = excelLector.LeerFilas(request);

        var existentes = await inventarioRepository.ObtenerTodosParaImportarAsync(cancellationToken);
        var itemsPorCodigo = new Dictionary<string, InventarioItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in existentes)
        {
            itemsPorCodigo.TryAdd(item.Codigo, item);
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

            var nombre = fila.Buscar("Nombre", "Artículo", "Articulo", "Descripción", "Descripcion");
            if (string.IsNullOrWhiteSpace(nombre))
            {
                omisiones.Add($"Fila {numeroFila}: sin nombre, omitida.");
                continue;
            }

            var stockActual = LeerEntero(fila.Buscar("Stock", "Stock Actual", "Existencia", "Existencias")) ?? 0;
            var stockMinimo = LeerEntero(fila.Buscar("Stock Mínimo", "Stock Minimo", "Mínimo", "Minimo")) ?? 0;
            var categoria = LeerCategoria(fila.Buscar("Categoría", "Categoria", "Tipo"));

            if (itemsPorCodigo.TryGetValue(codigo, out var itemExistente))
            {
                itemExistente.ActualizarDatos(nombre, stockMinimo);
                itemExistente.AjustarStock(stockActual);
                actualizados++;
                continue;
            }

            var nuevoItem = new InventarioItem(codigo, nombre, stockActual, categoria, stockMinimo);
            await inventarioRepository.AgregarAsync(nuevoItem, cancellationToken);
            itemsPorCodigo[codigo] = nuevoItem;
            creados++;
        }

        await inventarioRepository.GuardarCambiosAsync(cancellationToken);

        return new ImportarInventarioResultDto(filas.Count, creados, actualizados, omisiones.Count, omisiones.Take(50).ToList());
    }

    private static int? LeerEntero(string? texto) =>
        texto is not null && double.TryParse(texto, out var numero) && numero >= 0 ? (int)numero : null;

    /// <summary>Por defecto Insumo si la columna falta o no coincide con ningún valor reconocido, en vez de omitir la fila.</summary>
    private static CategoriaInventario LeerCategoria(string? texto)
    {
        if (texto is null)
        {
            return CategoriaInventario.Insumo;
        }

        var normalizado = texto.Trim().TrimEnd('s');
        return normalizado.Equals("Medicamento", StringComparison.OrdinalIgnoreCase)
            ? CategoriaInventario.Medicamento
            : CategoriaInventario.Insumo;
    }
}
