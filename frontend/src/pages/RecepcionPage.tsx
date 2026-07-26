import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { agruparDoctoresPorEspecialidad } from '../lib/agruparDoctores'
import type { CitaAgenda } from '../types/cita'
import type { Paciente, PacientesPaginados } from '../types/paciente'
import type { DoctorSimple } from '../types/doctor'
import './RecepcionPage.css'

const formateadorHora = new Intl.DateTimeFormat('es-DO', { timeStyle: 'short' })

function claseBadgeEstado(estado: CitaAgenda['estado']): string {
  switch (estado) {
    case 'EnEspera':
      return 'recepcion-badge recepcion-badge-en-espera'
    default:
      return 'recepcion-badge recepcion-badge-programada'
  }
}

export function RecepcionPage() {
  const [salaDeEspera, setSalaDeEspera] = useState<CitaAgenda[]>([])
  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const gruposDoctores = useMemo(() => agruparDoctoresPorEspecialidad(doctores), [doctores])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [accionandoId, setAccionandoId] = useState<string | null>(null)

  const [busqueda, setBusqueda] = useState('')
  const [busquedaDebounced, setBusquedaDebounced] = useState('')
  const [resultados, setResultados] = useState<Paciente[]>([])
  const [pacienteSeleccionado, setPacienteSeleccionado] = useState<Paciente | null>(null)
  const [doctorId, setDoctorId] = useState('')
  const [motivo, setMotivo] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorLlegada, setErrorLlegada] = useState<string | null>(null)

  const cargarSalaDeEspera = () => {
    api
      .get<CitaAgenda[]>('/api/citas/sala-espera')
      .then((datos) => setSalaDeEspera(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la sala de espera.')
      })
  }

  useEffect(() => {
    let cancelado = false

    Promise.all([api.get<CitaAgenda[]>('/api/citas/sala-espera'), api.get<DoctorSimple[]>('/api/personal/doctores')])
      .then(([sala, listaDoctores]) => {
        if (cancelado) return
        setSalaDeEspera(sala)
        setDoctores(listaDoctores)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar Recepción.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda), 300)
    return () => clearTimeout(temporizador)
  }, [busqueda])

  useEffect(() => {
    if (!busquedaDebounced.trim()) {
      setResultados([])
      return
    }

    let cancelado = false
    api
      .get<PacientesPaginados>(`/api/pacientes?pagina=1&tamanoPagina=10&busqueda=${encodeURIComponent(busquedaDebounced.trim())}`)
      .then((datos) => {
        if (!cancelado) setResultados(datos.items)
      })
      .catch(() => undefined)

    return () => {
      cancelado = true
    }
  }, [busquedaDebounced])

  const handleRegistrarLlegada = async (citaId: string) => {
    setAccionandoId(citaId)
    try {
      await api.patch(`/api/citas/registrar-llegada`, { citaId })
      cargarSalaDeEspera()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la llegada.')
    } finally {
      setAccionandoId(null)
    }
  }

  const handleLlegadaDirecta = async (event: FormEvent) => {
    event.preventDefault()
    setErrorLlegada(null)

    if (!pacienteSeleccionado) {
      setErrorLlegada('Selecciona un paciente.')
      return
    }
    if (!doctorId) {
      setErrorLlegada('Selecciona un doctor.')
      return
    }
    if (!motivo.trim()) {
      setErrorLlegada('El motivo es obligatorio.')
      return
    }

    setRegistrando(true)
    try {
      await api.post('/api/citas/llegada-directa', {
        pacienteId: pacienteSeleccionado.id,
        doctorId,
        motivo: motivo.trim(),
      })
      setPacienteSeleccionado(null)
      setDoctorId('')
      setMotivo('')
      cargarSalaDeEspera()
    } catch (err) {
      setErrorLlegada(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la llegada.')
    } finally {
      setRegistrando(false)
    }
  }

  if (cargando) {
    return (
      <DashboardLayout titulo="Recepción">
        <p className="text-secondary cargando-pulso">Cargando…</p>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout titulo="Recepción">
      {error && <p className="recepcion-error">{error}</p>}

      <section className="recepcion-llegada-card">
        <h2>Llegada rápida (sin cita)</h2>
        <div className="recepcion-buscador">
          <input
            type="search"
            placeholder="Buscar paciente por nombre o cédula…"
            value={pacienteSeleccionado ? `${pacienteSeleccionado.nombre} ${pacienteSeleccionado.apellido}` : busqueda}
            onChange={(event) => {
              setPacienteSeleccionado(null)
              setBusqueda(event.target.value)
            }}
          />
          {resultados.length > 0 && !pacienteSeleccionado && (
            <ul className="recepcion-resultados-lista">
              {resultados.map((paciente) => (
                <li key={paciente.id}>
                  <button
                    type="button"
                    onClick={() => {
                      setPacienteSeleccionado(paciente)
                      setBusqueda('')
                      setResultados([])
                    }}
                  >
                    {paciente.nombre} {paciente.apellido} — {paciente.cedula}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        <form className="recepcion-llegada-form" onSubmit={(event) => void handleLlegadaDirecta(event)}>
          <select value={doctorId} onChange={(event) => setDoctorId(event.target.value)}>
            <option value="">Selecciona un doctor…</option>
            {gruposDoctores.map((grupo) => (
              <optgroup key={grupo.etiqueta} label={grupo.etiqueta}>
                {grupo.doctores.map((doctor) => (
                  <option key={doctor.id} value={doctor.id}>
                    {doctor.nombreCompleto}
                  </option>
                ))}
              </optgroup>
            ))}
          </select>
          <input placeholder="Motivo" value={motivo} onChange={(event) => setMotivo(event.target.value)} required />
          <button type="submit" disabled={registrando}>
            {registrando ? 'Registrando…' : 'Registrar llegada'}
          </button>
        </form>
        {errorLlegada && <p className="recepcion-error">{errorLlegada}</p>}
      </section>

      <section className="recepcion-tabla-card">
        <h2>Sala de espera</h2>
        {salaDeEspera.length === 0 ? (
          <p className="text-secondary">No hay pacientes en espera.</p>
        ) : (
          <div className="recepcion-tabla-scroll">
            <table className="recepcion-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Doctor</th>
                  <th>Motivo</th>
                  <th>Hora</th>
                  <th>Estado</th>
                  <th>Deuda</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {salaDeEspera.map((cita) => (
                  <tr key={cita.id}>
                    <td>{cita.pacienteNombre}</td>
                    <td className="text-muted">{cita.doctorNombre}</td>
                    <td className="text-muted">{cita.motivo}</td>
                    <td className="text-muted">{cita.inicio ? formateadorHora.format(new Date(cita.inicio)) : '—'}</td>
                    <td>
                      <span className={claseBadgeEstado(cita.estado)}>{cita.estado}</span>
                    </td>
                    <td>
                      {cita.deudaPendiente > 0 && (
                        <span className="recepcion-badge-deuda">Deuda pendiente</span>
                      )}
                    </td>
                    <td>
                      {cita.estado === 'Programada' && (
                        <button
                          type="button"
                          onClick={() => void handleRegistrarLlegada(cita.id)}
                          disabled={accionandoId === cita.id}
                        >
                          {accionandoId === cita.id ? 'Registrando…' : 'Marcar llegada'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </DashboardLayout>
  )
}
