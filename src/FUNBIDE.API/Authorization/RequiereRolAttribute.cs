using FUNBIDE.Domain.Enums;

namespace FUNBIDE.API.Authorization;

/// <summary>
/// Declara qué roles de negocio (ADMIN, DOCTOR, FONDOS, LEMY) pueden invocar un
/// endpoint. Es metadata pura: la evalúa <c>RoleAuthorizationMiddleware</c>, que la
/// contrasta contra el claim de rol proyectado desde el JWT de Supabase.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiereRolAttribute(params RolUsuario[] rolesPermitidos) : Attribute
{
    public IReadOnlyCollection<RolUsuario> RolesPermitidos { get; } = rolesPermitidos;
}
