namespace FUNBIDE.API.Authorization;

/// <summary>
/// Marca un controlador o acción como append-only: <c>AppendOnlyGuardMiddleware</c>
/// rechaza con 403 cualquier verbo PUT, PATCH o DELETE dirigido a él, sin importar
/// si en algún momento se agrega por error una acción de actualización al controlador.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SoloLecturaEInsercionAttribute : Attribute;
