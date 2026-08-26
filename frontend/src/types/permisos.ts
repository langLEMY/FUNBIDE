import type { ModuloPermiso, RolUsuario } from './usuario'

export interface ModuloPermisoEstado {
  modulo: ModuloPermiso
  concedido: boolean
  esOverride: boolean
  defaultDelRol: boolean
}

export interface PermisosUsuario {
  usuarioId: string
  nombreCompleto: string
  rol: RolUsuario
  modulos: ModuloPermisoEstado[]
}

export interface PermisoDeseado {
  modulo: ModuloPermiso
  concedido: boolean
}

export const ETIQUETA_MODULO: Record<ModuloPermiso, string> = {
  Dashboard: 'Dashboard',
  Resumen: 'Resumen',
  Finanzas: 'Finanzas',
  Gastos: 'Gastos',
  Donaciones: 'Donaciones',
  Operaciones: 'Operaciones',
  Inventario: 'Inventario',
  Aseguradoras: 'Aseguradoras',
  Servicios: 'Precios privados',
  Actividad: 'Actividad (auditoría)',
  Pacientes: 'Base de datos de pacientes',
  HistorialClinico: 'Historial clínico',
  Directorio: 'Directorio de personal',
  Caja: 'Caja',
  Cobros: 'Cobros',
  Agenda: 'Agenda',
  Recepcion: 'Recepción',
}

/** Agrupa el catálogo para la pantalla de edición, con el mismo criterio visual que el menú lateral. */
export const GRUPOS_MODULOS: { grupo: string; modulos: ModuloPermiso[] }[] = [
  { grupo: 'Principal', modulos: ['Dashboard', 'Resumen'] },
  { grupo: 'Finanzas', modulos: ['Finanzas', 'Gastos', 'Donaciones'] },
  { grupo: 'Clínico', modulos: ['Pacientes', 'HistorialClinico'] },
  { grupo: 'Operaciones', modulos: ['Operaciones', 'Inventario', 'Aseguradoras', 'Servicios', 'Directorio'] },
  { grupo: 'Caja y Agenda', modulos: ['Caja', 'Cobros', 'Agenda', 'Recepcion'] },
  { grupo: 'Sistema', modulos: ['Actividad'] },
]
