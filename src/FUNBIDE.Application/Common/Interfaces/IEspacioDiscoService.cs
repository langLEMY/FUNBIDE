using FUNBIDE.Application.DTOs.Sistema;

namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Espacio libre en el disco donde vive el respaldo de la base de datos (o, si el
/// respaldo está deshabilitado, el disco donde corre la propia API). Diagnóstico
/// preventivo: un disco lleno rompe el backup y la subida de fotos en silencio.
/// </summary>
public interface IEspacioDiscoService
{
    Task<EstadoDiscoDto> ObtenerEstadoAsync(CancellationToken cancellationToken);
}
