namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Módulos de negocio cuyo acceso se puede conceder o revocar individualmente por
/// usuario, además del acceso por defecto que ya da <see cref="RolUsuario"/> (ver
/// <see cref="PermisosPorRolDefault"/>). Deliberadamente NO incluye la gestión de
/// Personal, la propia administración de Permisos, Sistema/Mantenimiento ni el flujo
/// de Citas del Doctor: quedan siempre gateados por rol puro para que nadie pueda
/// bloquearse a sí mismo la capacidad de administrar el sistema.
/// </summary>
public enum ModuloPermiso
{
    Dashboard,
    Resumen,
    Finanzas,
    Gastos,
    Donaciones,
    Operaciones,
    Inventario,
    Aseguradoras,
    Actividad,
    Pacientes,
    HistorialClinico,
    Directorio,
    Caja,
    Cobros,
    Agenda,
    Recepcion,
}
