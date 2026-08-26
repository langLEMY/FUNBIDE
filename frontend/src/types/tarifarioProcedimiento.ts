export type PlanAseguradora = 'Subsidiado' | 'Contributivo' | 'Pensionado' | 'PlanEspecial' | 'Larimar' | 'Estandar'

export const PLANES_ASEGURADORA: PlanAseguradora[] = [
  'Subsidiado',
  'Contributivo',
  'Pensionado',
  'PlanEspecial',
  'Larimar',
  'Estandar',
]

/** Etiqueta legible para los planes cuyo nombre interno no es ya una sola palabra en español. */
export const ETIQUETA_PLAN: Record<PlanAseguradora, string> = {
  Subsidiado: 'Subsidiado',
  Contributivo: 'Contributivo',
  Pensionado: 'Pensionado',
  PlanEspecial: 'Plan Especial',
  Larimar: 'Larimar',
  Estandar: 'Estándar',
}

export interface TarifarioProcedimiento {
  id: string
  seguroMedicoId: string
  plan: PlanAseguradora
  procedimiento: string
  montoSeguro: number
  montoPaciente: number
  montoTotal: number
  /** Excedente que va al fondo interno de la fundación (hoy solo Renacer) — no se le cobra al paciente. */
  montoFondo: number | null
  /** Especialidad de FUNBIDE para agrupar/filtrar en la UI — null si no tiene equivalente claro. */
  especialidad: string | null
}

export interface ImportarTarifarioResult {
  totalFilas: number
  creados: number
  actualizados: number
  omitidos: number
  omisiones: string[]
}
