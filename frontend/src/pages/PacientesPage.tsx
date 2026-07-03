import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PacienteRow } from '../components/pacientes/PacienteRow'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Paciente } from '../types/paciente'
import './PacientesPage.css'

export function PacientesPage() {
  const { perfil } = useAuth()
  const puedeAdministrar = perfil?.rol === 'Lemy'

  const [pacientes, setPacientes] = useState<Paciente[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

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

  return (
    <DashboardLayout titulo="Pacientes">
      {puedeAdministrar && (
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
          <p className="pacientes-nota text-muted">
            La foto de la cédula es opcional y se sube desde la tabla, después de agregar al paciente.
          </p>
        </section>
      )}

      <section className="pacientes-tabla-card">
        {error && <p className="pacientes-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando pacientes…</p>
        ) : pacientes.length === 0 ? (
          <p className="text-secondary">Todavía no hay pacientes registrados.</p>
        ) : (
          <table className="pacientes-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Apellido</th>
                <th>Cédula</th>
                <th>Teléfono</th>
                <th>Foto de cédula</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {pacientes.map((paciente) => (
                <PacienteRow
                  key={paciente.id}
                  paciente={paciente}
                  puedeAdministrar={puedeAdministrar}
                  onActualizado={actualizarEnLista}
                  onEliminado={quitarDeLista}
                />
              ))}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
