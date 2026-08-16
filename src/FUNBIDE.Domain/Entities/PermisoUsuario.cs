using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Excepción explícita al acceso por defecto del rol de un usuario a un
/// <see cref="ModuloPermiso"/> (ver <see cref="PermisosPorRolDefault"/>): solo existe
/// una fila aquí cuando alguien concedió un módulo que el rol no daría, o revocó uno
/// que sí daría. El permiso efectivo lo calcula <c>IPermisoResolverService</c>
/// combinando esta tabla con el default del rol.
/// </summary>
public sealed class PermisoUsuario : Entity
{
    public Guid UsuarioId { get; private set; }
    public ModuloPermiso Modulo { get; private set; }
    public bool Concedido { get; private set; }
    public DateTimeOffset ActualizadoEn { get; private set; }
    public Guid ActualizadoPorUsuarioId { get; private set; }

    private PermisoUsuario() { }

    public PermisoUsuario(Guid usuarioId, ModuloPermiso modulo, bool concedido, Guid actualizadoPorUsuarioId, DateTimeOffset ahora)
    {
        UsuarioId = usuarioId;
        Modulo = modulo;
        Concedido = concedido;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        ActualizadoEn = ahora;
    }

    public void Actualizar(bool concedido, Guid actualizadoPorUsuarioId, DateTimeOffset ahora)
    {
        Concedido = concedido;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        ActualizadoEn = ahora;
    }
}
