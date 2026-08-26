namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Acceso a módulos por defecto para cada <see cref="RolUsuario"/> sin overrides
/// explícitos (ver <c>PermisoUsuario</c>). Originalmente reproducía tal cual el acceso
/// previo a que existiera <see cref="ModuloPermiso"/> (única desviación intencional de
/// esa época: Admin no incluye <see cref="ModuloPermiso.Directorio"/>, porque esa ruta
/// nunca apareció en su menú). Se le siguen agregando módulos nuevos a medida que se
/// habilitan features para un rol (p. ej. <see cref="ModuloPermiso.Servicios"/> para
/// Admin/Lemy, o <see cref="ModuloPermiso.Recepcion"/> de solo lectura para Lemy).
///
/// Fondos es el puesto de caja/recepción: agenda citas, asigna doctor, gestiona la sala
/// de espera, y además opera la caja (abre/cierra turno, cobra con seguro/ARS aplicando
/// sus descuentos) — es el rol de cajera de la fundación. Admin también tiene
/// <see cref="ModuloPermiso.Caja"/>/<see cref="ModuloPermiso.Cobros"/> para supervisión,
/// igual que el resto del dinero de la fundación (Finanzas/Gastos/Donaciones).
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
                ModuloPermiso.Servicios,
                ModuloPermiso.Actividad,
                ModuloPermiso.Pacientes,
                ModuloPermiso.HistorialClinico,
                ModuloPermiso.Caja,
                ModuloPermiso.Cobros,
            },
            [RolUsuario.Lemy] = new HashSet<ModuloPermiso>
            {
                ModuloPermiso.Directorio,
                ModuloPermiso.Pacientes,
                ModuloPermiso.Inventario,
                ModuloPermiso.Aseguradoras,
                ModuloPermiso.Servicios,
                ModuloPermiso.Actividad,
                // Solo lectura: ListarSalaDeEsperaUseCase es la única acción de Recepción
                // a la que Lemy tiene ruta/página — no puede registrar llegadas ni llegadas
                // directas, esas siguen siendo exclusivas de Fondos por rol.
                ModuloPermiso.Recepcion,
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
