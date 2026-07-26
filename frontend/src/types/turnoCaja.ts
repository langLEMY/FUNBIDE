export type EstadoTurnoCaja = 'Abierto' | 'Cerrado'

export interface TurnoCaja {
  id: string
  usuarioAperturaId: string
  montoInicial: number
  abiertoEn: string
  estado: EstadoTurnoCaja
  usuarioCierreId: string | null
  montoFinalContado: number | null
  montoEsperado: number | null
  diferencia: number | null
  notas: string | null
  cerradoEn: string | null
}

export interface AbrirTurnoCajaRequest {
  montoInicial: number
}

export interface CerrarTurnoCajaRequest {
  montoFinalContado: number
  notas: string | null
}

// Variante de TurnoCaja con nombres en vez de ids, para la vista de supervisión de Admin.
export interface TurnoCajaAdmin {
  id: string
  usuarioAperturaNombre: string
  montoInicial: number
  abiertoEn: string
  estado: EstadoTurnoCaja
  usuarioCierreNombre: string | null
  montoFinalContado: number | null
  montoEsperado: number | null
  diferencia: number | null
  notas: string | null
  cerradoEn: string | null
}

export interface ResumenCaja {
  cajaAbierta: boolean
  efectivoEnCaja: number
  totalFacturadoHoy: number
  totalEfectivo: number
  totalTarjeta: number
  totalTransferencia: number
  totalCubiertoPorSeguro: number
  salidasAutorizadas: number
  pacientesEnEspera: number
  pacientesConDeudaPendiente: number
  consultasPendientesDeCobro: number
  gastosAdminEnElTurno: number
}
