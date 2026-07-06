import type { RolUsuario } from '../types/usuario'

const COLORES: Record<RolUsuario, string> = {
  Admin: 'var(--series-pacientes)',
  Doctor: 'var(--series-dinero)',
  Fondos: 'var(--danger)',
  Farmacia: 'var(--series-pacientes)',
  Lemy: 'var(--series-dinero)',
}

export function colorPorRol(rol: RolUsuario): string {
  return COLORES[rol]
}
