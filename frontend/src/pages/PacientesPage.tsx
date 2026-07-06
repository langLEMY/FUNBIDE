import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PacienteRow } from '../components/pacientes/PacienteRow'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Paciente } from '../types/paciente'
import { ESTADOS_PACIENTE, type EstadoPaciente } from '../types/paciente'
import './PacientesPage.css'

const FILTRO_TODOS = 'Todos'

export function PacientesPage() {
  const { perfil } = useAuth()
  const puedeEditar = perfil?.rol === 'Lemy'
  const puedeEliminar = perfil?.rol === 'Lemy' || perfil?.rol === 'Admin' || perfil?.rol === 'Doctor'
  const puedeVerHistorial = perfil?.rol === 'Doctor'
  const puedeCrear = Boolean(perfil)

  const [pacientes, setPacientes] = useState<Paciente[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<EstadoPaciente | typeof FILTRO_TODOS>(FILTRO_TODOS)

  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [cedula, setCedula] = useState('')
  const [telefono, setTelefono] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<Paciente[]>('/api/pacientes')
      .then((datos) => {
        if (!cancelado) setPacientes(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la base de datos de pacientes.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const ordenar = (lista: Paciente[]) =>
    [...lista].sort((a, b) => a.nombre.localeCompare(b.nombre) || a.apellido.localeCompare(b.apellido))

  const actualizarEnLista = (paciente: Paciente) => {
    setPacientes((actual) => ordenar(actual.map((p) => (p.id === paciente.id ? paciente : p))))
  }

  const quitarDeLista = (pacienteId: string) => {
    setPacientes((actual) => actual.filter((p) => p.id !== pacienteId))
  }

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)
    setCreando(true)
    try {
      const nuevo = await api.post<Paciente>('/api/pacientes', {
        nombre,
        apellido,
        cedula,
        telefono: telefono.trim() || null,
        edad: null,
        condicion: null,
      })
      setPacientes((actual) => ordenar([...actual, nuevo]))
      setNombre('')
      setApellido('')
      setCedula('')
      setTelefono('')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar al paciente.')
    } finally {
      setCreando(false)
    }
  }

  const pacientesFiltrados = useMemo(() => {
    const busquedaNormalizada = busqueda.trim().toLowerCase()
    return pacientes.filter((paciente) => {
      const coincideEstado = filtroEstado === FILTRO_TODOS || paciente.estado === filtroEstado
      if (!coincideEstado) return false
      if (!busquedaNormalizada) return true
      const campos = [paciente.nombre, paciente.apellido, paciente.condicion ?? ''].join(' ').toLowerCase()
      return campos.includes(busquedaNormalizada)
    })
  }, [pacientes, busqueda, filtroEstado])

  return (
    <DashboardLayout titulo="Pacientes">
      {puedeCrear && (
        <section className="pacientes-crear-card">
          <h2>Agregar paciente</h2>
          <form className="pacientes-crear-form" onSubmit={(event) => void handleCrear(event)}>
            <input
              placeholder="Nombre"
              value={nombre}
              onChange={(event) => setNombre(event.target.value)}
              required
            />
            <input
              placeholder="Apellido"
              value={apellido}
              onChange={(event) => setApellido(event.target.value)}
              required
            />
            <input
              placeholder="Cédula"
              value={cedula}
              onChange={(event) => setCedula(event.target.value)}
              required
            />
            <input
              placeholder="Teléfono (opcional)"
              value={telefono}
              onChange={(event) => setTelefono(event.target.value)}
            />
            <button type="submit" disabled={creando}>
              {creando ? 'Agregando…' : 'Agregar'}
            </button>
          </form>
          {errorCrear && <p className="pacientes-error">{errorCrear}</p>}
        </section>
      )}

      <div className="pacientes-buscador">
        <input
          type="search"
          placeholder="Buscar por nombre o condición…"
          value={busqueda}
          onChange={(event) => setBusqueda(event.target.value)}
        />
      </div>

      <div className="pacientes-tabs" role="tablist">
        <button
          type="button"
          role="tab"
          aria-selected={filtroEstado === FILTRO_TODOS}
          className={`pacientes-tab${filtroEstado === FILTRO_TODOS ? ' activo' : ''}`}
          onClick={() => setFiltroEstado(FILTRO_TODOS)}
        >
          Todos
        </button>
        {ESTADOS_PACIENTE.map((estado) => (
          <button
            key={estado}
            type="button"
            role="tab"
            aria-selected={filtroEstado === estado}
            className={`pacientes-tab${filtroEstado === estado ? ' activo' : ''}`}
            onClick={() => setFiltroEstado(estado)}
          >
            {estado}
          </button>
        ))}
      </div>

      <section className="pacientes-tabla-card">
        {error && <p className="pacientes-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando pacientes…</p>
        ) : pacientesFiltrados.length === 0 ? (
          <p className="text-secondary">
            {pacientes.length === 0
              ? 'Todavía no hay pacientes registrados.'
              : 'No hay pacientes que coincidan con la búsqueda o el filtro.'}
          </p>
        ) : (
          <div className="pacientes-tabla-scroll">
            <table className="pacientes-tabla">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Edad</th>
                  <th>Condición</th>
                  <th>Última visita</th>
                  <th>Estado</th>
                  <th>Cédula</th>
                  <th>Teléfono</th>
                  {(puedeEditar || puedeEliminar || puedeVerHistorial) && <th>Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {pacientesFiltrados.map((paciente) => (
                  <PacienteRow
                    key={paciente.id}
                    paciente={paciente}
                    puedeEditar={puedeEditar}
                    puedeEliminar={puedeEliminar}
                    puedeVerHistorial={puedeVerHistorial}
                    onActualizado={actualizarEnLista}
                    onEliminado={quitarDeLista}
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
