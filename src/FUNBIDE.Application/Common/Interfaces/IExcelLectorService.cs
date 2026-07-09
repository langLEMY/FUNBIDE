namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Lee un archivo Excel (.xls o .xlsx) y lo devuelve como filas indexadas por el
/// encabezado de columna (primera fila de la hoja), para que cada caso de uso de
/// importación mapee las columnas que le interesan sin acoplarse a la librería de
/// lectura concreta.
/// </summary>
public interface IExcelLectorService
{
    IReadOnlyList<IReadOnlyDictionary<string, string?>> LeerFilas(Stream contenido);
}
