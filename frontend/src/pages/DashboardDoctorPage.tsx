import { useEffect, useMemo, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { StatCard } from '../components/dashboard/StatCard'
import { api, ApiError } from '../lib/api'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import type { Cita, PacienteDelDoctor } from '../types/cita'
import './DashboardDoctorPage.css'

const formateadorEntero = new Intl.NumberFormat('es-DO')
const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

export function DashboardDoctorPage() {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)

  const [pendientes, setPendientes] = useState<Cita[]>([])
  const [programadas, setProgramadas] = useState<Cita[]>([])
  const [completadas, setCompletadas] = useState<Cita[]>([])
  const [pacientes, setPacientes] = useState<PacienteDelDoctor[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<Cita[]>('/api/citas/pendientes'),
      api.get<Cita[]>('/api/citas/programadas'),
      api.get<Cita[]>('/api/citas/completadas'),
      // Pacientes que alguna vez tuvieron una cita con este doctor (no todos los
      // pacientes de la clínica: el dominio no tiene un vínculo directo Paciente-Doctor).
      api.get<PacienteDelDoctor[]>('/api/citas/pacientes'),
    ])
      .then(([datosPendientes, datosProgramadas, datosCompletadas, datosPacientes]) => {
        if (cancelado) return
        setPendientes(datosPendientes)
        setProgramadas(datosProgramadas)
        setCompletadas(datosCompletadas)
        setPacientes(datosPacientes)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el dashboard.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const nombrePorPacienteId = useMemo(() => {
    const mapa = new Map<string, string>()
    for (const paciente of pacientes) {
      mapa.set(paciente.pacienteId, paciente.nombreCompleto)
    }
    return mapa
  }, [pacientes])

  const proximasCitas = useMemo(
    () =>
      [...programadas]
        .filter((cita) => cita.inicio)
        .sort((a, b) => (a.inicio ?? '').localeCompare(b.inicio ?? ''))
        .slice(0, 5),
    [programadas],
  )

  return (
    <DashboardLayout titulo="Dashboard">
      {error && <p className="dashboard-error">{error}</p>}

      <section className="dashboard-stats">
        <StatCard
          etiqueta="Citas pendientes"
          valor={cargando ? '—' : formateadorEntero.format(pendientes.length)}
          colorSerie={chartColors.dinero}
        />
        <StatCard
          etiqueta="Citas programadas"
          valor={cargando ? '—' : formateadorEntero.format(programadas.length)}
          colorSerie={chartColors.pacientes}
        />
        <StatCard
          etiqueta="Consultas completadas"
          valor={cargando ? '—' : formateadorEntero.format(completadas.length)}
          colorSerie={chartColors.pacientes}
        />
        <StatCard
          etiqueta="Pacientes totales"
          valor={cargando ? '—' : formateadorEntero.format(pacientes.length)}
          colorSerie={chartColors.actividad}
        />
      </section>

      <section className="dashboard-doctor-proximas-card">
        <h2>Próximas citas</h2>
        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : proximasCitas.length === 0 ? (
          <p className="text-secondary">No hay citas programadas.</p>
        ) : (
          <ul className="dashboard-doctor-proximas-lista">
            {proximasCitas.map((cita) => (
              <li key={cita.id}>
                <span>{nombrePorPacienteId.get(cita.pacienteId) ?? 'Desconocido'}</span>
                <span className="text-muted">{cita.motivo}</span>
                <span className="text-muted">{cita.inicio ? formateadorFecha.format(new Date(cita.inicio)) : '—'}</span>
              </li>
            ))}
          </ul>
        )}
      </section>
    </DashboardLayout>
  )
}
