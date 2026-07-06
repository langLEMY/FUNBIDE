import { useState } from 'react'
import { api, ApiError } from '../../lib/api'
import type { Cita } from '../../types/cita'

interface CitaRowProps {
  cita: Cita
  nombrePaciente: string
  onProgramada: (cita: Cita) => void
  onCompletada: (cita: Cita) => void
}

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

function claseBadgeEstado(estado: Cita['estado']): string {
  switch (estado) {
    case 'Pendiente':
      return 'citas-badge citas-badge-pendiente'
    case 'Programada':
      return 'citas-badge citas-badge-programada'
    case 'Completada':
      return 'citas-badge citas-badge-completada'
    case 'Cancelada':
      return 'citas-badge citas-badge-cancelada'
  }
}

export function CitaRow({ cita, nombrePaciente, onProgramada, onCompletada }: CitaRowProps) {
  const [mostrandoAccion, setMostrandoAccion] = useState(false)
  const [inicio, setInicio] = useState('')
  const [fin, setFin] = useState('')
  const [notasCierre, setNotasCierre] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const cancelar = () => {
    setMostrandoAccion(false)
    setInicio('')
    setFin('')
    setNotasCierre('')
    setError(null)
  }

  const confirmarProgramar = async () => {
    if (!inicio || !fin) {
      setError('Ingresa fecha/hora de inicio y fin.')
      return
    }
    const inicioIso = new Date(inicio).toISOString()
    const finIso = new Date(fin).toISOString()
    if (new Date(inicioIso) <= new Date()) {
      setError('La cita debe programarse en el futuro.')
      return
    }
    if (new Date(finIso) <= new Date(inicioIso)) {
      setError('La hora de fin debe ser posterior a la de inicio.')
      return
    }

    setError(null)
    setEnviando(true)
    try {
      const actualizada = await api.patch<Cita>('/api/citas/programar', {
        citaId: cita.id,
        inicio: inicioIso,
        fin: finIso,
      })
      onProgramada(actualizada)
      cancelar()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo programar la cita.')
    } finally {
      setEnviando(false)
    }
  }

  const confirmarCompletar = async () => {
    if (!notasCierre.trim()) {
      setError('Las notas de cierre son obligatorias.')
      return
    }

    setError(null)
    setEnviando(true)
    try {
      const actualizada = await api.patch<Cita>('/api/citas/completar', {
        citaId: cita.id,
        notasCierre: notasCierre.trim(),
      })
      onCompletada(actualizada)
      cancelar()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo completar la cita.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <>
      <tr>
        <td>{nombrePaciente}</td>
        <td className="text-muted">{cita.motivo}</td>
        <td>
          <span className={claseBadgeEstado(cita.estado)}>{cita.estado}</span>
        </td>
        <td className="text-muted">{cita.inicio ? formateadorFechaHora.format(new Date(cita.inicio)) : '—'}</td>
        <td className="text-muted">{cita.fin ? formateadorFechaHora.format(new Date(cita.fin)) : '—'}</td>
        <td className="text-muted">{cita.notasCierre ?? '—'}</td>
        <td className="citas-acciones">
          {cita.estado === 'Pendiente' &&
            (mostrandoAccion ? (
              <div className="citas-accion-form">
                <input type="datetime-local" value={inicio} onChange={(event) => setInicio(event.target.value)} />
                <input type="datetime-local" value={fin} onChange={(event) => setFin(event.target.value)} />
                <button type="button" onClick={() => void confirmarProgramar()} disabled={enviando}>
                  {enviando ? 'Programando…' : 'Confirmar'}
                </button>
                <button type="button" onClick={cancelar} disabled={enviando}>
                  Cancelar
                </button>
              </div>
            ) : (
              <button type="button" onClick={() => setMostrandoAccion(true)}>
                Programar
              </button>
            ))}

          {cita.estado === 'Programada' &&
            (mostrandoAccion ? (
              <div className="citas-accion-form">
                <textarea
                  placeholder="Notas de cierre"
                  value={notasCierre}
                  onChange={(event) => setNotasCierre(event.target.value)}
                />
                <button type="button" onClick={() => void confirmarCompletar()} disabled={enviando}>
                  {enviando ? 'Completando…' : 'Confirmar'}
                </button>
                <button type="button" onClick={cancelar} disabled={enviando}>
                  Cancelar
                </button>
              </div>
            ) : (
              <button type="button" onClick={() => setMostrandoAccion(true)}>
                Completar
              </button>
            ))}
        </td>
      </tr>
      {error && (
        <tr>
          <td colSpan={7} className="citas-row-error">
            {error}
          </td>
        </tr>
      )}
    </>
  )
}
