namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Invalida la copia en caché de <c>ConfiguracionSistema</c> que usa MantenimientoMiddleware
/// para no consultar la base en cada request autenticado. Sin invalidar acá, apagar/prender
/// el modo mantenimiento tardaría hasta que expire el TTL de esa caché en reflejarse.
/// </summary>
public interface IConfiguracionSistemaCache
{
    void Invalidar();
}
