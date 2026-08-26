import type { EspecialidadMedica } from './usuario'

export interface Servicio {
  id: string
  codigo: string
  nombre: string
  precio1: number
  precio2: number
  precio3: number
  especialidad: EspecialidadMedica | null
  activo: boolean
}

export interface CrearServicioRequest {
  codigo: string
  nombre: string
  precio1: number
  precio2: number
  precio3: number
  especialidad: EspecialidadMedica | null
}

export interface EditarServicioRequest {
  servicioId: string
  nombre: string
  precio1: number
  precio2: number
  precio3: number
  especialidad: EspecialidadMedica | null
}

export interface ImportarServiciosResult {
  totalFilas: number
  creados: number
  actualizados: number
  omitidos: number
  omisiones: string[]
}
