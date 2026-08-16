namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Reproduce exactamente el acceso a módulos que cada <see cref="RolUsuario"/> ya
/// tenía antes de existir <see cref="ModuloPermiso"/>, para que ningún usuario sin
/// overrides explícitos (ver <c>PermisoUsuario</c>) note un cambio de comportamiento.
/// Única desviación intencional: Admin no incluye <see cref="ModuloPermiso.Directorio"/>
/// — esa ruta nunca apareció en su menú (solo era alcanzable tecleando la URL), así
/// que se trata como "nunca estuvo habilitada" en vez de perpetuar el descuido.
/// </summary>
public static class PermisosPorRolDefault
{
    private static readonly IReadOnlyDictionary<RolUsuario, IReadOnlySet<ModuloPermiso>> Matriz =
        new Dictionary<RolUsuario, IReadOnlySet<ModuloPermiso>>
        {
            [RolUsuario.Admin] = new HashSet<ModuloPermiso>
            {
                ModuloPermiso.Dashboard,
                ModuloPermiso.Resumen,
                ModuloPermiso.Finanzas,
                ModuloPermiso.Gastos,
                ModuloPermiso.Donaciones,
                ModuloPermiso.Operaciones,
                ModuloPermiso.Inventario,
                ModuloPermiso.Aseguradoras,
                ModuloPermiso.Actividad,
                ModuloPermiso.Pacientes,
                ModuloPermiso.HistorialClinico,
            },
            [RolUsuario.Lemy] = new HashSet<ModuloPermiso>
            {
                ModuloPermiso.Directorio,
                ModuloPermiso.Pacientes,
                ModuloPermiso.Inventario,
                ModuloPermiso.Aseguradoras,
                ModuloPermiso.Actividad,
            },
            [RolUsuario.Doctor] = new HashSet<ModuloPermiso>
            {
                ModuloPermiso.Pacientes,
                ModuloPermiso.Inventario,
                ModuloPermiso.HistorialClinico,
            },
            [RolUsuario.Fondos] = new HashSet<ModuloPermiso>
            {
                ModuloPermiso.Caja,
                ModuloPermiso.Cobros,
                ModuloPermiso.Agenda,
                ModuloPermiso.Recepcion,
                ModuloPermiso.Pacientes,
                ModuloPermiso.Inventario,
            },
        };

    public static IReadOnlySet<ModuloPermiso> Para(RolUsuario rol) => Matriz[rol];
}
