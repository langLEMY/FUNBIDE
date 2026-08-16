import { useEffect, useMemo, useRef, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { agruparDoctoresPorEspecialidad } from '../lib/agruparDoctores'
import type { TurnoCajaAdmin } from '../types/turnoCaja'
import type { CitaAgenda } from '../types/cita'
import type { DoctorSimple } from '../types/doctor'
import './OperacionesPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
})
const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

function aFechaISO(fecha: Date): string {
  return fecha.toISOString().slice(0, 10)
}

function inicioDeHoy(): Date {
  const ahora = new Date()
  ahora.setHours(0, 0, 0, 0)
  return ahora
}

function sumarDias(fecha: Date, dias: number): Date {
  const copia = new Date(fecha)
  copia.setDate(copia.getDate() + dias)
  return copia
}

const PRESETS_TURNOS: { etiqueta: string; calcularInicio: (fin: Date) => Date }[] = [
  { etiqueta: 'Hoy', calcularInicio: (fin) => fin },
  { etiqueta: 'Últimos 7 días', calcularInicio: (fin) => sumarDias(fin, -7) },
  { etiqueta: 'Últimos 30 días', calcularInicio: (fin) => sumarDias(fin, -30) },
]

function claseBadgeEstadoCita(estado: CitaAgenda['estado']): string {
  switch (estado) {
    case 'Cancelada':
      return 'operaciones-badge operaciones-badge-cancelada'
    case 'Completada':
      return 'operaciones-badge operaciones-badge-completada'
    case 'EnEspera':
      return 'operaciones-badge operaciones-badge-en-espera'
    default:
      return 'operaciones-badge operaciones-badge-programada'
  }
}

function construirQueryAgenda(fecha: string, doctorId: string): string {
  const params = new URLSearchParams()
  if (fecha) params.set('fecha', fecha)
  if (doctorId) params.set('doctorId', doctorId)
  return params.toString()
}

export function OperacionesPage() {
  const hoy = inicioDeHoy()

  // Turnos de caja
  const [desdeTurnos, setDesdeTurnos] = useState(aFechaISO(sumarDias(hoy, -7)))
  const [hastaTurnos, setHastaTurnos] = useState(aFechaISO(hoy))
  const [turnos, setTurnos] = useState<TurnoCajaAdmin[]>([])
  const [cargandoTurnos, setCargandoTurnos] = useState(true)
  const [errorTurnos, setErrorTurnos] = useState<string | null>(null)

  // Agenda
  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const gruposDoctores = useMemo(() => agruparDoctoresPorEspecialidad(doctores), [doctores])
  const [filtroFecha, setFiltroFecha] = useState(aFechaISO(hoy))
  const [filtroDoctorId, setFiltroDoctorId] = useState('')
  const [agenda, setAgenda] = useState<CitaAgenda[]>([])
  const [cargandoAgenda, setCargandoAgenda] = useState(true)
  const [errorAgenda, setErrorAgenda] = useState<string | null>(null)

  // Sala de espera
  const [salaDeEspera, setSalaDeEspera] = useState<CitaAgenda[]>([])
  const [cargandoSala, setCargandoSala] = useState(true)
  const [errorSala, setErrorSala] = useState<string | null>(null)

  // cargarTurnos se llama a mano desde varios lugares (presets, submit del filtro), no
  // solo desde un efecto — por eso el guard de "respuesta obsoleta" usa un contador de
  // petición en vez de la variable local `cancelado` que usan los demás fetches de esta
  // página. Sin esto, un preset "Hoy" seguido rápido de "Últimos 30 días" podía terminar
  // mostrando los turnos de un rango que ya no es el seleccionado si la primera respuesta
  // llegaba después que la segunda.
  const peticionTurnosRef = useRef(0)

  const cargarTurnos = (desdeValor: string, hastaValor: string) => {
    const idPeticion = ++peticionTurnosRef.current
    setCargandoTurnos(true)
    setErrorTurnos(null)
    const desdeIso = encodeURIComponent(`${desdeValor}T00:00:00.000Z`)
    const hastaIso = encodeURIComponent(`${hastaValor}T23:59:59.999Z`)
    api
      .get<TurnoCajaAdmin[]>(`/api/caja/turnos?desde=${desdeIso}&hasta=${hastaIso}`)
      .then((datos) => {
        if (idPeticion === peticionTurnosRef.current) setTurnos(datos)
      })
      .catch((err) => {
        if (idPeticion === peticionTurnosRef.current) {
          setErrorTurnos(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar los turnos de caja.')
        }
      })
      .finally(() => {
        if (idPeticion === peticionTurnosRef.current) setCargandoTurnos(false)
      })
  }

  useEffect(() => {
    cargarTurnos(desdeTurnos, hastaTurnos)
    // Solo al montar: los presets y el filtro disparan la carga ellos mismos.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const aplicarPresetTurnos = (preset: (typeof PRESETS_TURNOS)[number]) => {
    const fin = inicioDeHoy()
    const inicio = preset.calcularInicio(fin)
    const desdeValor = aFechaISO(inicio)
    const hastaValor = aFechaISO(fin)
    setDesdeTurnos(desdeValor)
    setHastaTurnos(hastaValor)
    cargarTurnos(desdeValor, hastaValor)
  }

  useEffect(() => {
    let cancelado = false
    api
      .get<DoctorSimple[]>('/api/personal/doctores')
      .then((datos) => {
        if (!cancelado) setDoctores(datos)
      })
      .catch(() => undefined)
    return () => {
      cancelado = true
    }
  }, [])

  useEffect(() => {
    let cancelado = false
    setCargandoAgenda(true)
    api
      .get<CitaAgenda[]>(`/api/citas/agenda?${construirQueryAgenda(filtroFecha, filtroDoctorId)}`)
      .then((datos) => {
        if (!cancelado) setAgenda(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setErrorAgenda(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la agenda.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargandoAgenda(false)
      })
    return () => {
      cancelado = true
    }
  }, [filtroFecha, filtroDoctorId])

  useEffect(() => {
    let cancelado = false
    api
      .get<CitaAgenda[]>('/api/citas/sala-espera')
      .then((datos) => {
        if (!cancelado) setSalaDeEspera(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setErrorSala(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la sala de espera.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargandoSala(false)
      })
    return () => {
      cancelado = true
    }
  }, [])

  return (
    <DashboardLayout titulo="Operaciones">
      <p className="text-secondary operaciones-intro">
        Vista de solo lectura: qué pasó en Caja/Recepción y en la agenda de los doctores. No se puede abrir/cerrar
        caja ni agendar/cancelar citas desde acá — eso sigue siendo de Fondos y Doctor.
      </p>

      <section className="operaciones-tabla-card">
        <div className="operaciones-seccion-header">
          <p className="operaciones-seccion-titulo">Turnos de caja</p>
          <div className="operaciones-presets">
            {PRESETS_TURNOS.map((preset) => (
              <button key={preset.etiqueta} type="button" onClick={() => aplicarPresetTurnos(preset)}>
                {preset.etiqueta}
              </button>
            ))}
          </div>
        </div>
        <form
          className="operaciones-rango"
          onSubmit={(event) => {
            event.preventDefault()
            cargarTurnos(desdeTurnos, hastaTurnos)
          }}
        >
          <label>
            Desde
            <input
              type="date"
              value={desdeTurnos}
              max={hastaTurnos}
              onChange={(event) => setDesdeTurnos(event.target.value)}
            />
          </label>
          <label>
            Hasta
            <input
              type="date"
              value={hastaTurnos}
              min={desdeTurnos}
              onChange={(event) => setHastaTurnos(event.target.value)}
            />
          </label>
          <button type="submit" disabled={cargandoTurnos}>
            {cargandoTurnos ? 'Filtrando…' : 'Filtrar'}
          </button>
        </form>

        {errorTurnos && <p className="operaciones-error">{errorTurnos}</p>}

        {cargandoTurnos ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : turnos.length === 0 ? (
          <p className="text-secondary">Sin turnos de caja en este rango.</p>
        ) : (
          <div className="operaciones-tabla-scroll">
            <table className="operaciones-tabla">
              <thead>
                <tr>
                  <th>Abierto</th>
                  <th>Abrió</th>
                  <th>Monto inicial</th>
                  <th>Estado</th>
                  <th>Cerrado</th>
                  <th>Cerró</th>
                  <th>Contado</th>
                  <th>Esperado</th>
                  <th>Diferencia</th>
                </tr>
              </thead>
              <tbody>
                {turnos.map((turno) => (
                  <tr key={turno.id}>
                    <td className="text-muted">{formateadorFechaHora.format(new Date(turno.abiertoEn))}</td>
                    <td>{turno.usuarioAperturaNombre}</td>
                    <td>{formateadorMoneda.format(turno.montoInicial)}</td>
                    <td>
                      <span
                        className={`operaciones-badge ${turno.estado === 'Abierto' ? 'operaciones-badge-en-espera' : 'operaciones-badge-completada'}`}
                      >
                        {turno.estado}
                      </span>
                    </td>
                    <td className="text-muted">
                      {turno.cerradoEn ? formateadorFechaHora.format(new Date(turno.cerradoEn)) : '—'}
                    </td>
                    <td>{turno.usuarioCierreNombre ?? '—'}</td>
                    <td>{turno.montoFinalContado != null ? formateadorMoneda.format(turno.montoFinalContado) : '—'}</td>
                    <td>{turno.montoEsperado != null ? formateadorMoneda.format(turno.montoEsperado) : '—'}</td>
                    <td className={turno.diferencia && turno.diferencia !== 0 ? 'operaciones-diferencia-distinta' : ''}>
                      {turno.diferencia != null ? formateadorMoneda.format(turno.diferencia) : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="operaciones-tabla-card">
        <div className="operaciones-seccion-header">
          <p className="operaciones-seccion-titulo">Agenda</p>
        </div>
        <div className="operaciones-filtros">
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
              className="operaciones-filtros-limpiar"
              onClick={() => {
                setFiltroFecha('')
                setFiltroDoctorId('')
              }}
            >
              Limpiar filtros
            </button>
          )}
        </div>

        {errorAgenda && <p className="operaciones-error">{errorAgenda}</p>}

        {cargandoAgenda ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : agenda.length === 0 ? (
          <p className="text-secondary">No hay citas que coincidan con el filtro.</p>
        ) : (
          <div className="operaciones-tabla-scroll">
            <table className="operaciones-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Doctor</th>
                  <th>Motivo</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {agenda.map((cita) => (
                  <tr key={cita.id}>
                    <td>{cita.pacienteNombre}</td>
                    <td className="text-muted">{cita.doctorNombre}</td>
                    <td className="text-muted">{cita.motivo}</td>
                    <td className="text-muted">{cita.inicio ? formateadorFechaHora.format(new Date(cita.inicio)) : '—'}</td>
                    <td className="text-muted">{cita.fin ? formateadorFechaHora.format(new Date(cita.fin)) : '—'}</td>
                    <td>
                      <span className={claseBadgeEstadoCita(cita.estado)}>{cita.estado}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="operaciones-tabla-card">
        <p className="operaciones-seccion-titulo">Sala de espera ahora</p>

        {errorSala && <p className="operaciones-error">{errorSala}</p>}

        {cargandoSala ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : salaDeEspera.length === 0 ? (
          <p className="text-secondary">No hay nadie en sala de espera en este momento.</p>
        ) : (
          <div className="operaciones-tabla-scroll">
            <table className="operaciones-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Doctor</th>
                  <th>Motivo</th>
                  <th>Llegó</th>
                </tr>
              </thead>
              <tbody>
                {salaDeEspera.map((cita) => (
                  <tr key={cita.id}>
                    <td>{cita.pacienteNombre}</td>
                    <td className="text-muted">{cita.doctorNombre}</td>
                    <td className="text-muted">{cita.motivo}</td>
                    <td className="text-muted">{cita.inicio ? formateadorFechaHora.format(new Date(cita.inicio)) : '—'}</td>
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
