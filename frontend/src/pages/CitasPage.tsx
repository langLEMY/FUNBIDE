import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { CitaRow } from '../components/citas/CitaRow'
import { api, ApiError } from '../lib/api'
import type { Cita } from '../types/cita'
import type { Paciente, PacientesPaginados } from '../types/paciente'
import './CitasPage.css'

type Tab = 'Pendientes' | 'Programadas' | 'En espera' | 'Completadas'
const TABS: Tab[] = ['Pendientes', 'Programadas', 'En espera', 'Completadas']

export function CitasPage() {
  const [pendientes, setPendientes] = useState<Cita[]>([])
  const [programadas, setProgramadas] = useState<Cita[]>([])
  const [enEspera, setEnEspera] = useState<Cita[]>([])
  const [completadas, setCompletadas] = useState<Cita[]>([])
  const [pacientes, setPacientes] = useState<Paciente[]>([])
  const [tab, setTab] = useState<Tab>('Pendientes')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [pacienteId, setPacienteId] = useState('')
  const [motivo, setMotivo] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<Cita[]>('/api/citas/pendientes'),
      api.get<Cita[]>('/api/citas/programadas'),
      api.get<Cita[]>('/api/citas/en-espera'),
      api.get<Cita[]>('/api/citas/completadas'),
      api.get<PacientesPaginados>('/api/pacientes?pagina=1&tamanoPagina=100'),
    ])
      .then(([datosPendientes, datosProgramadas, datosEnEspera, datosCompletadas, datosPacientes]) => {
        if (cancelado) return
        setPendientes(datosPendientes)
        setProgramadas(datosProgramadas)
        setEnEspera(datosEnEspera)
        setCompletadas(datosCompletadas)
        setPacientes(datosPacientes.items)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar las citas.')
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

  const nombreDePaciente = (pacienteIdBuscado: string) => nombrePorPacienteId.get(pacienteIdBuscado) ?? 'Desconocido'

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)

    if (!pacienteId) {
      setErrorCrear('Selecciona un paciente.')
      return
    }
    if (!motivo.trim()) {
      setErrorCrear('El motivo es obligatorio.')
      return
    }

    setCreando(true)
    try {
      const nueva = await api.post<Cita>('/api/citas', { pacienteId, motivo: motivo.trim() })
      setPendientes((actual) => [nueva, ...actual])
      setMotivo('')
      setPacienteId('')
      setTab('Pendientes')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo crear la cita.')
    } finally {
      setCreando(false)
    }
  }

  const moverAProgramadas = (cita: Cita) => {
    setPendientes((actual) => actual.filter((c) => c.id !== cita.id))
    setProgramadas((actual) => [cita, ...actual])
  }

  const moverACompletadas = (cita: Cita) => {
    setProgramadas((actual) => actual.filter((c) => c.id !== cita.id))
    setEnEspera((actual) => actual.filter((c) => c.id !== cita.id))
    setCompletadas((actual) => [cita, ...actual])
  }

  const listaActiva =
    tab === 'Pendientes'
      ? pendientes
      : tab === 'Programadas'
        ? programadas
        : tab === 'En espera'
          ? enEspera
          : completadas

  return (
    <DashboardLayout titulo="Citas">
      <section className="citas-crear-card">
        <h2>Nueva cita</h2>
        <form className="citas-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <select value={pacienteId} onChange={(event) => setPacienteId(event.target.value)}>
            <option value="">Selecciona un paciente…</option>
            {pacientes.map((paciente) => (
              <option key={paciente.id} value={paciente.id}>
                {paciente.nombre} {paciente.apellido}
              </option>
            ))}
          </select>
          <input placeholder="Motivo" value={motivo} onChange={(event) => setMotivo(event.target.value)} required />
          <button type="submit" disabled={creando}>
            {creando ? 'Creando…' : 'Crear cita'}
          </button>
        </form>
        {errorCrear && <p className="citas-error">{errorCrear}</p>}
      </section>

      <div className="citas-tabs" role="tablist">
        {TABS.map((opcion) => (
          <button
            key={opcion}
            type="button"
            role="tab"
            aria-selected={tab === opcion}
            className={`citas-tab${tab === opcion ? ' activo' : ''}`}
            onClick={() => setTab(opcion)}
          >
            {opcion}
          </button>
        ))}
      </div>

      <section className="citas-tabla-card">
        {error && <p className="citas-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando citas…</p>
        ) : listaActiva.length === 0 ? (
          <p className="text-secondary">No hay citas {tab.toLowerCase()}.</p>
        ) : (
          <div className="citas-tabla-scroll">
            <table className="citas-tabla">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th>Motivo</th>
                  <th>Estado</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Notas de cierre</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {listaActiva.map((cita) => (
                  <CitaRow
                    key={cita.id}
                    cita={cita}
                    nombrePaciente={nombreDePaciente(cita.pacienteId)}
                    onProgramada={moverAProgramadas}
                    onCompletada={moverACompletadas}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </DashboardLayout>
  )
}
