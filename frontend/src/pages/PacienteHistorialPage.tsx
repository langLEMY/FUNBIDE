import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { Paciente } from '../types/paciente'
import type { EntradaHistorial } from '../types/historialClinico'
import { parsearContenidoHistorial } from '../types/historialClinico'
import './PacienteHistorialPage.css'

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium', timeStyle: 'short' })

export function PacienteHistorialPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const [paciente, setPaciente] = useState<Paciente | null>(null)
  const [entradas, setEntradas] = useState<EntradaHistorial[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [diagnostico, setDiagnostico] = useState('')
  const [tratamiento, setTratamiento] = useState('')
  const [notas, setNotas] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorRegistrar, setErrorRegistrar] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<Paciente>(`/api/pacientes/${id}`),
      api.get<EntradaHistorial[]>(`/api/historial-clinico/paciente/${id}`),
    ])
      .then(([pacienteEncontrado, datosEntradas]) => {
        if (cancelado) return
        setPaciente(pacienteEncontrado)
        setEntradas(datosEntradas)
      })
      .catch((err) => {
        if (cancelado) return
        if (err instanceof ApiError && err.status === 404) {
          setError('No se encontró a ese paciente.')
        } else {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el historial clínico.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [id])

  const handleRegistrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorRegistrar(null)

    if (!diagnostico.trim() && !tratamiento.trim() && !notas.trim()) {
      setErrorRegistrar('Completa al menos un campo antes de guardar.')
      return
    }

    setRegistrando(true)
    try {
      const nueva = await api.post<EntradaHistorial>('/api/historial-clinico', {
        pacienteId: id,
        citaId: null,
        contenido: {
          diagnostico: diagnostico.trim() || null,
          tratamiento: tratamiento.trim() || null,
          notas: notas.trim() || null,
        },
      })
      setEntradas((actual) => [nueva, ...actual])
      setDiagnostico('')
      setTratamiento('')
      setNotas('')
    } catch (err) {
      setErrorRegistrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la entrada.')
    } finally {
      setRegistrando(false)
    }
  }

  return (
    <DashboardLayout titulo="Historial clínico">
      <button type="button" className="historial-volver" onClick={() => navigate('/pacientes')}>
        ← Volver
      </button>

      {cargando ? (
        <p className="text-secondary">Cargando…</p>
      ) : error && !paciente ? (
        <p className="historial-error">{error}</p>
      ) : (
        <>
          {paciente && (
            <section className="historial-encabezado-card">
              <h2>
                {paciente.nombre} {paciente.apellido}
              </h2>
              <p className="text-muted">Cédula {paciente.cedula}</p>
            </section>
          )}

          <section className="historial-crear-card">
            <h2>Agregar entrada</h2>
            <form className="historial-crear-form" onSubmit={(event) => void handleRegistrar(event)}>
              <input
                placeholder="Diagnóstico"
                value={diagnostico}
                onChange={(event) => setDiagnostico(event.target.value)}
              />
              <input
                placeholder="Tratamiento"
                value={tratamiento}
                onChange={(event) => setTratamiento(event.target.value)}
              />
              <textarea placeholder="Notas" value={notas} onChange={(event) => setNotas(event.target.value)} />
              <button type="submit" disabled={registrando}>
                {registrando ? 'Guardando…' : 'Guardar entrada'}
              </button>
            </form>
            {errorRegistrar && <p className="historial-error">{errorRegistrar}</p>}
          </section>

          <section className="historial-lista-card">
            {entradas.length === 0 ? (
              <p className="text-secondary">Todavía no hay entradas registradas.</p>
            ) : (
              <ul className="historial-lista">
                {entradas.map((entrada) => {
                  const contenido = parsearContenidoHistorial(entrada.contenido)
                  return (
                    <li key={entrada.id} className="historial-entrada">
                      <p className="text-muted historial-entrada-fecha">
                        {formateadorFechaHora.format(new Date(entrada.registradoEn))}
                      </p>
                      {contenido?.diagnostico && (
                        <p>
                          <strong>Diagnóstico:</strong> {contenido.diagnostico}
                        </p>
                      )}
                      {contenido?.tratamiento && (
                        <p>
                          <strong>Tratamiento:</strong> {contenido.tratamiento}
                        </p>
                      )}
                      {contenido?.notas && (
                        <p>
                          <strong>Notas:</strong> {contenido.notas}
                        </p>
                      )}
                    </li>
                  )
                })}
              </ul>
            )}
          </section>
        </>
      )}
    </DashboardLayout>
  )
}
