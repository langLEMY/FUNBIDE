import { useEffect, useMemo, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { StatCard } from '../components/dashboard/StatCard'
import { api, ApiError } from '../lib/api'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import type { Cita } from '../types/cita'
import type { Paciente } from '../types/paciente'
import './DashboardDoctorPage.css'

const formateadorEntero = new Intl.NumberFormat('es-DO')
const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

export function DashboardDoctorPage() {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)

  const [pendientes, setPendientes] = useState<Cita[]>([])
  const [programadas, setProgramadas] = useState<Cita[]>([])
  const [pacientes, setPacientes] = useState<Paciente[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<Cita[]>('/api/citas/pendientes'),
      api.get<Cita[]>('/api/citas/programadas'),
      api.get<Paciente[]>('/api/pacientes'),
    ])
      .then(([datosPendientes, datosProgramadas, datosPacientes]) => {
        if (cancelado) return
        setPendientes(datosPendientes)
        setProgramadas(datosProgramadas)
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
      mapa.set(paciente.id, `${paciente.nombre} ${paciente.apellido}`)
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
      <p className="text-muted dashboard-doctor-aviso">
        Panel temporal — más adelante va a tener más indicadores clínicos.
      </p>

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
          etiqueta="Pacientes totales"
          valor={cargando ? '—' : formateadorEntero.format(pacientes.length)}
          colorSerie={chartColors.actividad}
        />
      </section>

      <section className="dashboard-doctor-proximas-card">
        <h2>Próximas citas</h2>
        {cargando ? (
          <p className="text-secondary">Cargando…</p>
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
