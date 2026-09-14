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

/** Los cinco subtipos de receta comparten la misma forma de contenido (lista de ítems repetible), pero cada uno con sus propios campos — ver CAMPOS_ITEM_POR_TIPO. */
export const TIPOS_RECETA: TipoEntradaHistorial[] = [
  'RecetaMedicamento',
  'RecetaImagenes',
  'RecetaAnaliticas',
  'RecetaVacunas',
  'RecetaProcedimientos',
]

/** Los documentos formales comparten la misma forma de contenido (un solo formulario de campos, sin lista repetible) — ver CAMPOS_DOCUMENTO_POR_TIPO. */
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
  RecetaImagenes: 'Orden de imágenes',
  RecetaAnaliticas: 'Orden de analíticas / laboratorio',
  RecetaVacunas: 'Registro de vacuna',
  RecetaProcedimientos: 'Orden de procedimientos',
  LicenciaMedica: 'Licencia médica',
  Prequirurgica: 'Evaluación prequirúrgica',
  ConstanciaConsulta: 'Constancia de consulta',
  Referimiento: 'Referimiento',
  OrdenMedica: 'Orden médica',
  EvolucionMedica: 'Evolución médica',
}

/** Un campo de texto simple (input) o multilínea (textarea) o una lista fija de opciones (select). */
export type TipoCampo = 'texto' | 'area' | 'fecha' | 'numero' | 'select'

export interface CampoEstructurado {
  clave: string
  etiqueta: string
  tipo: TipoCampo
  placeholder?: string
  opciones?: string[]
  /** El valor de este campo es lo que se resalta como "título" de cada ítem/documento en la lista y al imprimir. */
  esPrincipal?: boolean
}

/**
 * Campos de cada ítem repetible de una receta, por tipo — todos comparten el mismo
 * "contenedor" (ContenidoReceta: items + notasGenerales), pero cada tipo pide datos
 * distintos por ítem (dosis de un medicamento vs. lote de una vacuna, etc.).
 */
export const CAMPOS_ITEM_POR_TIPO: Partial<Record<TipoEntradaHistorial, CampoEstructurado[]>> = {
  RecetaMedicamento: [
    { clave: 'medicamento', etiqueta: 'Medicamento', tipo: 'texto', esPrincipal: true },
    { clave: 'presentacion', etiqueta: 'Presentación', tipo: 'texto', placeholder: 'Tableta 500mg, jarabe…' },
    { clave: 'dosis', etiqueta: 'Dosis', tipo: 'texto', placeholder: '1 tableta' },
    { clave: 'frecuencia', etiqueta: 'Frecuencia', tipo: 'texto', placeholder: 'Cada 8 horas' },
    { clave: 'duracion', etiqueta: 'Duración', tipo: 'texto', placeholder: 'Por 7 días' },
    { clave: 'via', etiqueta: 'Vía', tipo: 'texto', placeholder: 'Oral' },
    { clave: 'indicaciones', etiqueta: 'Indicaciones adicionales', tipo: 'texto' },
  ],
  RecetaImagenes: [
    { clave: 'estudio', etiqueta: 'Estudio de imagen', tipo: 'texto', esPrincipal: true, placeholder: 'Radiografía de tórax' },
    { clave: 'region', etiqueta: 'Región / zona', tipo: 'texto' },
    { clave: 'indicaciones', etiqueta: 'Indicaciones', tipo: 'texto' },
  ],
  RecetaAnaliticas: [
    { clave: 'estudio', etiqueta: 'Analítica / examen de laboratorio', tipo: 'texto', esPrincipal: true },
    { clave: 'indicaciones', etiqueta: 'Indicaciones (ej. en ayunas)', tipo: 'texto' },
  ],
  RecetaVacunas: [
    { clave: 'vacuna', etiqueta: 'Vacuna', tipo: 'texto', esPrincipal: true },
    { clave: 'dosisNumero', etiqueta: 'Dosis (1ra, 2da, refuerzo…)', tipo: 'texto' },
    { clave: 'viaAplicacion', etiqueta: 'Vía de aplicación', tipo: 'texto', placeholder: 'Intramuscular' },
    { clave: 'proximaDosis', etiqueta: 'Próxima dosis', tipo: 'fecha' },
  ],
  RecetaProcedimientos: [
    { clave: 'procedimiento', etiqueta: 'Procedimiento', tipo: 'texto', esPrincipal: true },
    { clave: 'indicaciones', etiqueta: 'Indicaciones / preparación', tipo: 'texto' },
  ],
}

/** Campos del formulario único (no repetible) de cada documento formal, por tipo. */
export const CAMPOS_DOCUMENTO_POR_TIPO: Partial<Record<TipoEntradaHistorial, CampoEstructurado[]>> = {
  LicenciaMedica: [
    { clave: 'diagnostico', etiqueta: 'Diagnóstico', tipo: 'texto' },
    { clave: 'diasReposo', etiqueta: 'Días de reposo', tipo: 'numero' },
    { clave: 'fechaDesde', etiqueta: 'Desde', tipo: 'fecha' },
    { clave: 'fechaHasta', etiqueta: 'Hasta', tipo: 'fecha' },
    { clave: 'tipoReposo', etiqueta: 'Tipo de reposo', tipo: 'select', opciones: ['Domiciliario', 'Laboral', 'Escolar'] },
    { clave: 'observaciones', etiqueta: 'Observaciones', tipo: 'area' },
  ],
  Prequirurgica: [
    { clave: 'procedimientoPropuesto', etiqueta: 'Procedimiento propuesto', tipo: 'texto' },
    { clave: 'riesgoQuirurgico', etiqueta: 'Riesgo quirúrgico', tipo: 'select', opciones: ['Bajo', 'Moderado', 'Alto'] },
    { clave: 'apto', etiqueta: 'Apto para cirugía', tipo: 'select', opciones: ['Sí', 'No'] },
    { clave: 'estudiosRevisados', etiqueta: 'Estudios revisados', tipo: 'area' },
    { clave: 'recomendaciones', etiqueta: 'Recomendaciones', tipo: 'area' },
  ],
  ConstanciaConsulta: [
    { clave: 'fechaConsulta', etiqueta: 'Fecha de la consulta', tipo: 'fecha' },
    { clave: 'motivo', etiqueta: 'Motivo de consulta', tipo: 'texto' },
    { clave: 'textoConstancia', etiqueta: 'Texto de la constancia', tipo: 'area' },
  ],
  Referimiento: [
    { clave: 'especialidadDestino', etiqueta: 'Especialidad a la que se refiere', tipo: 'texto' },
    { clave: 'centroODoctorDestino', etiqueta: 'Centro o médico destino', tipo: 'texto' },
    { clave: 'motivo', etiqueta: 'Motivo del referimiento', tipo: 'area' },
    { clave: 'resumenClinico', etiqueta: 'Resumen clínico', tipo: 'area' },
    { clave: 'urgente', etiqueta: 'Urgente', tipo: 'select', opciones: ['Sí', 'No'] },
  ],
  OrdenMedica: [
    { clave: 'indicaciones', etiqueta: 'Indicaciones', tipo: 'area' },
    { clave: 'observaciones', etiqueta: 'Observaciones', tipo: 'area' },
  ],
  EvolucionMedica: [
    { clave: 'subjetivo', etiqueta: 'Subjetivo (lo que refiere el paciente)', tipo: 'area' },
    { clave: 'objetivo', etiqueta: 'Objetivo (hallazgos del examen)', tipo: 'area' },
    { clave: 'analisis', etiqueta: 'Análisis / diagnóstico', tipo: 'area' },
    { clave: 'plan', etiqueta: 'Plan', tipo: 'area' },
  ],
}

export interface ContenidoNotaClinica {
  diagnostico: string | null
  tratamiento: string | null
  notas: string | null
}

/** Un ítem de receta es un diccionario libre de clave→valor: qué claves trae depende del tipo (ver CAMPOS_ITEM_POR_TIPO). */
export type ItemReceta = Record<string, string>

export interface ContenidoReceta {
  items: ItemReceta[]
  notasGenerales: string | null
}

/** Un documento formal es un diccionario libre de clave→valor: qué claves trae depende del tipo (ver CAMPOS_DOCUMENTO_POR_TIPO). */
export interface ContenidoDocumento {
  campos: Record<string, string>
  /** Compatibilidad con entradas viejas que solo tenían un cuerpo de texto libre, sin campos estructurados. */
  cuerpo?: string
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

/** Campo cuyo valor se resalta como título de un ítem de receta (el primero marcado esPrincipal, o el primero de la lista). */
export function campoPrincipalReceta(tipo: TipoEntradaHistorial): string {
  const campos = CAMPOS_ITEM_POR_TIPO[tipo] ?? []
  return (campos.find((c) => c.esPrincipal) ?? campos[0])?.clave ?? ''
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
    const datos = JSON.parse(contenido) as Partial<ContenidoDocumento>
    return { campos: datos.campos ?? {}, cuerpo: datos.cuerpo }
  } catch {
    return null
  }
}
