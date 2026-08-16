using FUNBIDE.Domain.Enums;

namespace FUNBIDE.API.Authorization;

/// <summary>
/// Declara qué módulo(s) togglables (<see cref="ModuloPermiso"/>) dan acceso a un
/// endpoint, con semántica OR cuando hay más de uno (igual que
/// <see cref="RequiereRolAttribute"/>) — necesario para endpoints consumidos por dos
/// páginas del frontend con permisos independientes. Lo evalúa
/// <c>PermisoAuthorizationMiddleware</c> contra el set efectivo de permisos del
/// usuario (default de su rol + overrides), no contra su rol directamente.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequierePermisoAttribute(params ModuloPermiso[] modulosPermitidos) : Attribute
{
    public IReadOnlyCollection<ModuloPermiso> ModulosPermitidos { get; } = modulosPermitidos;
}
