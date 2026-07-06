export interface ContenidoHistorial {
  diagnostico: string | null
  tratamiento: string | null
  notas: string | null
}

export interface EntradaHistorial {
  id: string
  pacienteId: string
  doctorId: string
  citaId: string | null
  contenido: string
  registradoEn: string
}

export interface RegistrarEntradaHistorialRequest {
  pacienteId: string
  citaId: string | null
  contenido: ContenidoHistorial
}

export function parsearContenidoHistorial(contenido: string): ContenidoHistorial | null {
  try {
    return JSON.parse(contenido) as ContenidoHistorial
  } catch {
    return null
  }
}
