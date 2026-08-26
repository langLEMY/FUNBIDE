import { useEffect, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { CitaAgenda } from '../types/cita'
import './SalaEsperaPage.css'

const formateadorHora = new Intl.DateTimeFormat('es-DO', { timeStyle: 'short' })

// Mismo intervalo que RecepcionPage.tsx y el badge del Sidebar, para que las tres vistas
// de la sala de espera nunca se vean desincronizadas entre sí por más de eso.
const INTERVALO_REFRESCO_MS = 20000

function claseBadgeEstado(estado: CitaAgenda['estado']): string {
  switch (estado) {
    case 'EnEspera':
      return 'sala-espera-badge sala-espera-badge-en-espera'
    default:
      return 'sala-espera-badge sala-espera-badge-programada'
  }
}

/**
 * Vista de solo lectura de la sala de espera para Admin y Lemy — mismos datos que
 * RecepcionPage.tsx, sin el formulario de "Llegada rápida" ni el botón "Marcar llegada"
 * (esas acciones siguen siendo exclusivas de Fondos).
 */
export function SalaEsperaPage() {
  const [salaDeEspera, setSalaDeEspera] = useState<CitaAgenda[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const cargar = () => {
    api
      .get<CitaAgenda[]>('/api/citas/sala-espera')
      .then((datos) => setSalaDeEspera(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la sala de espera.')
      })
      .finally(() => setCargando(false))
  }

  useEffect(() => {
    cargar()
    const intervalo = setInterval(cargar, INTERVALO_REFRESCO_MS)
    return () => clearInterval(intervalo)
  }, [])

  if (cargando) {
    return (
      <DashboardLayout titulo="Sala de espera">
        <p className="text-secondary cargando-pulso">Cargando…</p>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout titulo="Sala de espera">
      <p className="text-secondary sala-espera-subtitulo">
        Pacientes que ya llegaron o tienen cita programada para hoy, y con qué doctor. Solo lectura — registrar
        llegadas se hace desde Recepción.
      </p>

      {error && <p className="sala-espera-error">{error}</p>}

      <section className="sala-espera-tabla-card">
        {salaDeEspera.length === 0 ? (
          <p className="text-secondary">No hay pacientes en espera.</p>
        ) : (
          <div className="sala-espera-tabla-scroll">
            <table className="sala-espera-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Doctor</th>
                  <th>Motivo</th>
                  <th>Hora</th>
                  <th>Estado</th>
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
