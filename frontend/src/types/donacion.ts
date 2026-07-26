export interface Donacion {
  id: string
  donanteNombre: string
  donanteContacto: string | null
  monto: number
  concepto: string
  registradoEn: string
}

export interface RegistrarDonacionRequest {
  donanteNombre: string
  donanteContacto: string | null
  monto: number
  concepto: string
}
