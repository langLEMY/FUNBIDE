namespace FUNBIDE.API.Authorization;

/// <summary>
/// Excepción explícita al bloqueo de <c>MantenimientoMiddleware</c>: el endpoint marcado
/// sigue respondiendo aunque el modo mantenimiento esté activo, para que cualquier
/// usuario autenticado (no solo LEMY) pueda seguir viendo por qué el resto del sistema
/// no responde, en vez de recibir un 503 sin contexto.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class VisibleDuranteMantenimientoAttribute : Attribute;
