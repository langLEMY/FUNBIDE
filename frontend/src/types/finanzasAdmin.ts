import type { TipoMovimientoFinanciero } from './finanzas'

export interface MovimientoImportante {
  id: string
  fecha: string
  origen: 'Cobro' | 'Manual'
  tipo: TipoMovimientoFinanciero
  concepto: string
  monto: number
  pacienteNombre: string | null
}

export interface ResumenMensual {
  mes: number
  ingresos: number
  gastos: number
  ganancia: number
  /** Subconjunto de `ingresos` que es fondo interno de la fundación (excedente negociado con las aseguradoras). */
  fondoGanancias: number
}

export interface RegistrarGastoAdminRequest {
  concepto: string
  monto: number
}
