import type { RolUsuario } from '../types/usuario'

const COLORES: Record<RolUsuario, string> = {
  Admin: 'var(--acento-primario)',
  Doctor: 'var(--series-dinero)',
  Fondos: 'var(--danger)',
  Lemy: 'var(--series-dinero)',
}

export function colorPorRol(rol: RolUsuario): string {
  return COLORES[rol]
}
