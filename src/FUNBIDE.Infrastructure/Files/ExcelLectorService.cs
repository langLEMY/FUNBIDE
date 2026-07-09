using System.Text;
using ExcelDataReader;
using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Files;

/// <summary>
/// Implementación sobre ExcelDataReader. Soporta tanto .xlsx (OOXML) como .xls
/// (BIFF legado) — este último requiere <see cref="CodePagesEncodingProvider"/>
/// porque el proyecto corre con <c>InvariantGlobalization</c> habilitado, que por
/// defecto no trae soporte para las páginas de código de Windows que usan los
/// archivos .xls antiguos.
/// </summary>
public sealed class ExcelLectorService : IExcelLectorService
{
    static ExcelLectorService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public IReadOnlyList<IReadOnlyDictionary<string, string?>> LeerFilas(Stream contenido)
    {
        using var reader = ExcelReaderFactory.CreateReader(contenido);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true },
        });

        var tabla = dataSet.Tables[0];
        var encabezados = tabla.Columns.Cast<System.Data.DataColumn>()
            .Select(c => c.ColumnName.Trim())
            .ToList();

        var filas = new List<IReadOnlyDictionary<string, string?>>();
        foreach (System.Data.DataRow fila in tabla.Rows)
        {
            var valores = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var encabezado in encabezados)
            {
                var valor = fila[encabezado];
                valores[encabezado] = valor is null or DBNull ? null : Convert.ToString(valor)?.Trim();
            }

            filas.Add(valores);
        }

        return filas;
    }
}
