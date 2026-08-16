using FUNBIDE.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FUNBIDE.Infrastructure.Caching;

/// <summary>
/// Clave y TTL compartidos entre MantenimientoMiddleware (lector, cachea el resultado de
/// <c>IConfiguracionSistemaRepository.ObtenerAsync</c> para no pegarle a Postgres en cada
/// request autenticado) y este invalidador (usado por CambiarModoMantenimientoUseCase
/// justo después de guardar el toggle). El TTL corto es solo una red de seguridad —la
/// invalidación explícita es lo que hace que el cambio se sienta instantáneo.
/// </summary>
public static class ConfiguracionSistemaCacheClave
{
    public const string Valor = "sistema:configuracion";
    public static readonly TimeSpan Duracion = TimeSpan.FromSeconds(5);
}

public sealed class ConfiguracionSistemaCache(IMemoryCache memoryCache) : IConfiguracionSistemaCache
{
    public void Invalidar() => memoryCache.Remove(ConfiguracionSistemaCacheClave.Valor);
}
