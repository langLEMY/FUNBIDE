export type PlanSenasa = 'Contributivo' | 'Pensionado' | 'Larimar'

export const PLANES_SENASA: PlanSenasa[] = ['Contributivo', 'Pensionado', 'Larimar']

export interface TarifarioProcedimiento {
  id: string
  seguroMedicoId: string
  plan: PlanSenasa
  procedimiento: string
  montoSeguro: number
  montoPaciente: number
  montoTotal: number
}

export interface ImportarTarifarioResult {
  totalFilas: number
  creados: number
  actualizados: number
  omitidos: number
  omisiones: string[]
}
