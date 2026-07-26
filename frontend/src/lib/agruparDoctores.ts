import type { DoctorSimple } from '../types/doctor'
import { ETIQUETA_ESPECIALIDAD } from '../types/personal'

export interface GrupoDoctores {
  etiqueta: string
  doctores: DoctorSimple[]
}

/** Agrupa doctores por especialidad para segmentar los selectores de Agenda/Recepción/Operaciones. */
export function agruparDoctoresPorEspecialidad(doctores: DoctorSimple[]): GrupoDoctores[] {
  const grupos = new Map<string, DoctorSimple[]>()

  for (const doctor of doctores) {
    const etiqueta = doctor.especialidad ? ETIQUETA_ESPECIALIDAD[doctor.especialidad] : 'Sin especialidad asignada'
    const lista = grupos.get(etiqueta) ?? []
    lista.push(doctor)
    grupos.set(etiqueta, lista)
  }

  return [...grupos.entries()]
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([etiqueta, lista]) => ({
      etiqueta,
      doctores: [...lista].sort((a, b) => a.nombreCompleto.localeCompare(b.nombreCompleto)),
    }))
}
