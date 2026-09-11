/** Debe coincidir exactamente con FUNBIDE.Domain.Enums.TipoEntradaHistorial. */
export type TipoEntradaHistorial =
  | 'NotaClinica'
  | 'RecetaMedicamento'
  | 'RecetaImagenes'
  | 'RecetaAnaliticas'
  | 'RecetaVacunas'
  | 'RecetaProcedimientos'
  | 'LicenciaMedica'
  | 'Prequirurgica'
  | 'ConstanciaConsulta'
  | 'Referimiento'
  | 'OrdenMedica'
  | 'EvolucionMedica'

/** Los cinco subtipos de receta comparten la misma forma de contenido (ver ContenidoReceta). */
export const TIPOS_RECETA: TipoEntradaHistorial[] = [
  'RecetaMedicamento',
  'RecetaImagenes',
  'RecetaAnaliticas',
  'RecetaVacunas',
  'RecetaProcedimientos',
]

/** Los documentos formales comparten la misma forma de contenido (ver ContenidoDocumento). */
export const TIPOS_DOCUMENTO: TipoEntradaHistorial[] = [
  'LicenciaMedica',
  'Prequirurgica',
  'ConstanciaConsulta',
  'Referimiento',
  'OrdenMedica',
  'EvolucionMedica',
]

export const ETIQUETA_TIPO_ENTRADA: Record<TipoEntradaHistorial, string> = {
  NotaClinica: 'Nota clínica',
  RecetaMedicamento: 'Receta de medicamentos',
  RecetaImagenes: 'Receta de imágenes',
  RecetaAnaliticas: 'Receta de analíticas',
  RecetaVacunas: 'Receta de vacunas',
  RecetaProcedimientos: 'Receta de procedimientos',
  LicenciaMedica: 'Licencia médica',
  Prequirurgica: 'Prequirúrgica',
  ConstanciaConsulta: 'Constancia de consulta',
  Referimiento: 'Referimiento',
  OrdenMedica: 'Orden médica',
  EvolucionMedica: 'Evolución médica',
}

/** Placeholder contextual para el cuerpo de texto libre de cada documento formal. */
export const PLACEHOLDER_CUERPO_DOCUMENTO: Partial<Record<TipoEntradaHistorial, string>> = {
  LicenciaMedica: 'Motivo y días de reposo indicados…',
  Prequirurgica: 'Evaluación y hallazgos previos a la cirugía…',
  ConstanciaConsulta: 'Motivo de consulta y horario de asistencia…',
  Referimiento: 'Médico o centro al que se refiere, y motivo…',
  OrdenMedica: 'Indicaciones para el paciente…',
  EvolucionMedica: 'Evolución del paciente desde la última consulta…',
}

export interface ContenidoNotaClinica {
  diagnostico: string | null
  tratamiento: string | null
  notas: string | null
}

export interface ItemReceta {
  descripcion: string
  indicaciones: string
}

export interface ContenidoReceta {
  items: ItemReceta[]
  notasGenerales: string | null
}

export interface ContenidoDocumento {
  cuerpo: string
}

export type ContenidoHistorial = ContenidoNotaClinica | ContenidoReceta | ContenidoDocumento

export interface EntradaHistorial {
  id: string
  pacienteId: string
  doctorId: string
  citaId: string | null
  tipo: TipoEntradaHistorial
  contenido: string
  registradoEn: string
}

export interface RegistrarEntradaHistorialRequest {
  pacienteId: string
  citaId: string | null
  tipo: TipoEntradaHistorial
  contenido: ContenidoHistorial
}

export function esContenidoReceta(tipo: TipoEntradaHistorial): boolean {
  return TIPOS_RECETA.includes(tipo)
}

export function esContenidoDocumento(tipo: TipoEntradaHistorial): boolean {
  return TIPOS_DOCUMENTO.includes(tipo)
}

export function parsearContenidoNotaClinica(contenido: string): ContenidoNotaClinica | null {
  try {
    return JSON.parse(contenido) as ContenidoNotaClinica
  } catch {
    return null
  }
}

export function parsearContenidoReceta(contenido: string): ContenidoReceta | null {
  try {
    const datos = JSON.parse(contenido) as Partial<ContenidoReceta>
    return { items: datos.items ?? [], notasGenerales: datos.notasGenerales ?? null }
  } catch {
    return null
  }
}

export function parsearContenidoDocumento(contenido: string): ContenidoDocumento | null {
  try {
    return JSON.parse(contenido) as ContenidoDocumento
  } catch {
    return null
  }
}
