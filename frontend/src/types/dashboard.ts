export interface ResumenDiario {
  fecha: string
  pacientesAtendidos: number
  dineroMovido: number
}

export interface ItemStockBajo {
  id: string
  codigo: string
  nombre: string
  stockActual: number
  stockMinimo: number
}

export interface PacienteConDeuda {
  pacienteId: string
  pacienteNombre: string
  montoAdeudado: number
}

export interface AlertasAdmin {
  stockBajo: ItemStockBajo[]
  pacientesConDeuda: PacienteConDeuda[]
}
