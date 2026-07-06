export type TipoMovimientoFinanciero = 'Ingreso' | 'Egreso'

export const TIPOS_MOVIMIENTO_FINANCIERO: TipoMovimientoFinanciero[] = ['Ingreso', 'Egreso']

export interface MovimientoFinanciero {
  id: string
  tipo: TipoMovimientoFinanciero
  monto: number
  concepto: string
  citaId: string | null
  registradoEn: string
}

export interface RegistrarMovimientoFinancieroRequest {
  tipo: TipoMovimientoFinanciero
  monto: number
  concepto: string
  citaId: string | null
}
