import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { agruparDoctoresPorEspecialidad } from '../lib/agruparDoctores'
import type { CitaAgenda } from '../types/cita'
import type { Paciente, PacientesPaginados } from '../types/paciente'
import type { DoctorSimple } from '../types/doctor'
import './AgendaPage.css'

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

function claseBadgeEstado(estado: CitaAgenda['estado']): string {
  switch (estado) {
    case 'Cancelada':
      return 'agenda-badge agenda-badge-cancelada'
    case 'Completada':
      return 'agenda-badge agenda-badge-completada'
    case 'EnEspera':
      return 'agenda-badge agenda-badge-en-espera'
    default:
      return 'agenda-badge agenda-badge-programada'
  }
}

function construirQuery(fecha: string, doctorId: string): string {
  const params = new URLSearchParams()
  if (fecha) params.set('fecha', fecha)
  if (doctorId) params.set('doctorId', doctorId)
  return params.toString()
}

// FUNBIDE opera en horario de Santo Domingo (UTC-4 todo el año, sin horario de verano —
// ver SystemDateTimeProvider.cs en el backend). Construir la fecha con `new Date(...)` e
// interpretarla en la zona LOCAL DEL NAVEGADOR podía desfasar la cita si el equipo de
// recepción tenía otra zona configurada; con el offset fijo, el mismo "3:00pm" siempre
// cae en las 3:00pm de la clínica sin importar dónde esté el navegador.
function construirFechaHoraClinica(fecha: string, hora: string): string {
  return `${fecha}T${hora}:00-04:00`
}

export function AgendaPage() {
  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const gruposDoctores = useMemo(() => agruparDoctoresPorEspecialidad(doctores), [doctores])
  const [citas, setCitas] = useState<CitaAgenda[]>([])
  const [cargando, setCargando] = useState(true)
  const [cargandoTabla, setCargandoTabla] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [accionandoId, setAccionandoId] = useState<string | null>(null)

  const [filtroFecha, setFiltroFecha] = useState('')
  const [filtroDoctorId, setFiltroDoctorId] = useState('')

  const [busqueda, setBusqueda] = useState('')
  const [busquedaDebounced, setBusquedaDebounced] = useState('')
  const [resultados, setResultados] = useState<Paciente[]>([])
  const [pacienteSeleccionado, setPacienteSeleccionado] = useState<Paciente | null>(null)
  const [doctorId, setDoctorId] = useState('')
  const [motivo, setMotivo] = useState('')
  const [fecha, setFecha] = useState('')
  const [horaInicio, setHoraInicio] = useState('')
  const [horaFin, setHoraFin] = useState('')
  const [agendando, setAgendando] = useState(false)
  const [errorAgendar, setErrorAgendar] = useState<string | null>(null)

  const [recargarClave, setRecargarClave] = useState(0)
  const recargarAgenda = () => setRecargarClave((clave) => clave + 1)

  useEffect(() => {
    let cancelado = false

    api
      .get<DoctorSimple[]>('/api/personal/doctores')
      .then((datos) => {
        if (!cancelado) setDoctores(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar Agenda.')
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
    let cancelado = false
    setCargandoTabla(true)
    api
      .get<CitaAgenda[]>(`/api/citas/agenda?${construirQuery(filtroFecha, filtroDoctorId)}`)
      .then((datos) => {
        if (!cancelado) setCitas(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la agenda.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargandoTabla(false)
      })

    return () => {
      cancelado = true
    }
  }, [filtroFecha, filtroDoctorId, recargarClave])

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

  const handleAgendar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorAgendar(null)

    if (!pacienteSeleccionado) {
      setErrorAgendar('Selecciona un paciente.')
      return
    }
    if (!doctorId) {
      setErrorAgendar('Selecciona un doctor.')
      return
    }
    if (!motivo.trim()) {
      setErrorAgendar('El motivo es obligatorio.')
      return
    }
    if (!fecha || !horaInicio || !horaFin) {
      setErrorAgendar('Ingresa fecha y horario.')
      return
    }
    if (horaFin <= horaInicio) {
      setErrorAgendar('La hora de fin debe ser posterior a la hora de inicio.')
      return
    }

    const inicioIso = construirFechaHoraClinica(fecha, horaInicio)
    const finIso = construirFechaHoraClinica(fecha, horaFin)

    setAgendando(true)
    try {
      await api.post('/api/citas/agendar', {
        pacienteId: pacienteSeleccionado.id,
        doctorId,
        motivo: motivo.trim(),
        inicio: inicioIso,
        fin: finIso,
      })
      setPacienteSeleccionado(null)
      setDoctorId('')
      setMotivo('')
      setFecha('')
      setHoraInicio('')
      setHoraFin('')
      recargarAgenda()
    } catch (err) {
      setErrorAgendar(
        err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agendar la cita.',
      )
    } finally {
      setAgendando(false)
    }
  }

  const handleCancelar = async (citaId: string) => {
    if (!window.confirm('¿Cancelar esta cita? No se puede deshacer.')) {
      return
    }

    setAccionandoId(citaId)
    try {
      await api.patch('/api/citas/cancelar', { citaId })
      recargarAgenda()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cancelar la cita.')
    } finally {
      setAccionandoId(null)
    }
  }

  if (cargando) {
    return (
      <DashboardLayout titulo="Agenda">
        <p className="text-secondary cargando-pulso">Cargando…</p>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout titulo="Agenda">
      {error && <p className="agenda-error">{error}</p>}

      <section className="agenda-crear-card">
        <h2>Cita rápida</h2>
        <div className="agenda-buscador">
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
            <ul className="agenda-resultados-lista">
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

        <form className="agenda-crear-form" onSubmit={(event) => void handleAgendar(event)}>
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
          <input
            type="date"
            min={new Date().toISOString().slice(0, 10)}
            value={fecha}
            onChange={(event) => setFecha(event.target.value)}
            required
          />
          <input type="time" value={horaInicio} onChange={(event) => setHoraInicio(event.target.value)} required />
          <input type="time" value={horaFin} onChange={(event) => setHoraFin(event.target.value)} required />
          <button type="submit" disabled={agendando}>
            {agendando ? 'Agendando…' : 'Agendar'}
          </button>
        </form>
        {errorAgendar && <p className="agenda-error">{errorAgendar}</p>}
      </section>

      <div className="agenda-filtros">
        <select value={filtroDoctorId} onChange={(event) => setFiltroDoctorId(event.target.value)}>
          <option value="">Todos los doctores</option>
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
        <input type="date" value={filtroFecha} onChange={(event) => setFiltroFecha(event.target.value)} />
        {(filtroFecha || filtroDoctorId) && (
          <button
            type="button"
            className="agenda-filtros-limpiar"
            onClick={() => {
              setFiltroFecha('')
              setFiltroDoctorId('')
            }}
          >
            Limpiar filtros
          </button>
        )}
      </div>

      <section className="agenda-tabla-card">
        {cargandoTabla ? (
          <p className="text-secondary cargando-pulso">Cargando citas…</p>
        ) : citas.length === 0 ? (
          <p className="text-secondary">No hay citas que coincidan con el filtro.</p>
        ) : (
          <div className="agenda-tabla-scroll">
            <table className="agenda-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Doctor</th>
                  <th>Motivo</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {citas.map((cita) => (
                  <tr key={cita.id}>
                    <td>{cita.pacienteNombre}</td>
                    <td className="text-muted">{cita.doctorNombre}</td>
                    <td className="text-muted">{cita.motivo}</td>
                    <td className="text-muted">{cita.inicio ? formateadorFechaHora.format(new Date(cita.inicio)) : '—'}</td>
                    <td className="text-muted">{cita.fin ? formateadorFechaHora.format(new Date(cita.fin)) : '—'}</td>
                    <td>
                      <span className={claseBadgeEstado(cita.estado)}>{cita.estado}</span>
                    </td>
                    <td>
                      {(cita.estado === 'Programada' || cita.estado === 'EnEspera') && (
                        <button
                          type="button"
                          onClick={() => void handleCancelar(cita.id)}
                          disabled={accionandoId === cita.id}
                        >
                          {accionandoId === cita.id ? 'Cancelando…' : 'Cancelar'}
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
