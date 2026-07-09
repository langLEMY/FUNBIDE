import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { ImportarExcel } from '../components/ImportarExcel'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Empleado, ImportarEmpleadosResultado } from '../types/empleado'
import './DirectorioPage.css'

export function DirectorioPage() {
  const { perfil } = useAuth()
  const puedeAdministrar = perfil?.rol === 'Lemy'

  const [empleados, setEmpleados] = useState<Empleado[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [resultadoImportacion, setResultadoImportacion] = useState<ImportarEmpleadosResultado | null>(null)

  const [nombreNuevo, setNombreNuevo] = useState('')
  const [cargoNuevo, setCargoNuevo] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  const [editandoId, setEditandoId] = useState<string | null>(null)
  const [nombreEdit, setNombreEdit] = useState('')
  const [cargoEdit, setCargoEdit] = useState('')
  const [guardando, setGuardando] = useState(false)

  useEffect(() => {
    let cancelado = false

    setCargando(true)
    api
      .get<Empleado[]>('/api/empleados')
      .then((datos) => {
        if (!cancelado) setEmpleados(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el directorio.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [resultadoImportacion])

  const ordenarPorNombre = (lista: Empleado[]) =>
    [...lista].sort((a, b) => a.nombreCompleto.localeCompare(b.nombreCompleto))

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)
    setCreando(true)
    try {
      const nuevo = await api.post<Empleado>('/api/empleados', {
        nombreCompleto: nombreNuevo,
        cargo: cargoNuevo.trim() || null,
      })
      setEmpleados((actual) => ordenarPorNombre([...actual, nuevo]))
      setNombreNuevo('')
      setCargoNuevo('')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar a la persona.')
    } finally {
      setCreando(false)
    }
  }

  const iniciarEdicion = (empleado: Empleado) => {
    setEditandoId(empleado.id)
    setNombreEdit(empleado.nombreCompleto)
    setCargoEdit(empleado.cargo ?? '')
  }

  const guardarEdicion = async (id: string) => {
    setGuardando(true)
    setError(null)
    try {
      const actualizado = await api.patch<Empleado>('/api/empleados', {
        empleadoId: id,
        nombreCompleto: nombreEdit,
        cargo: cargoEdit.trim() || null,
      })
      setEmpleados((actual) => ordenarPorNombre(actual.map((e) => (e.id === id ? actualizado : e))))
      setEditandoId(null)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar el cambio.')
    } finally {
      setGuardando(false)
    }
  }

  const eliminar = async (empleado: Empleado) => {
    if (!window.confirm(`¿Eliminar a ${empleado.nombreCompleto} del directorio?`)) {
      return
    }

    setError(null)
    try {
      await api.delete(`/api/empleados/${empleado.id}`)
      setEmpleados((actual) => actual.filter((e) => e.id !== empleado.id))
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo eliminar.')
    }
  }

  return (
    <DashboardLayout titulo="Directorio">
      {puedeAdministrar && (
        <section className="directorio-crear-card">
          <h2>Importar personal desde Excel</h2>
          <ImportarExcel<ImportarEmpleadosResultado>
            endpoint="/api/empleados/importar"
            onImportado={setResultadoImportacion}
          />
          {resultadoImportacion && (
            <div className="directorio-importar-resumen">
              <p>
                {resultadoImportacion.creados} de {resultadoImportacion.totalFilas} filas importadas.
                {resultadoImportacion.omitidos > 0 && ` ${resultadoImportacion.omitidos} filas omitidas.`}
              </p>
              {resultadoImportacion.omisiones.length > 0 && (
                <ul className="directorio-importar-omisiones">
                  {resultadoImportacion.omisiones.map((omision, indice) => (
                    <li key={indice}>{omision}</li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </section>
      )}

      {puedeAdministrar && (
        <section className="directorio-crear-card">
          <h2>Agregar persona</h2>
          <form className="directorio-crear-form" onSubmit={(event) => void handleCrear(event)}>
            <input
              placeholder="Nombre completo"
              value={nombreNuevo}
              onChange={(event) => setNombreNuevo(event.target.value)}
              required
            />
            <input
              placeholder="Cargo (opcional)"
              value={cargoNuevo}
              onChange={(event) => setCargoNuevo(event.target.value)}
            />
            <button type="submit" disabled={creando}>
              {creando ? 'Agregando…' : 'Agregar'}
            </button>
          </form>
          {errorCrear && <p className="directorio-error">{errorCrear}</p>}
        </section>
      )}

      <section className="directorio-tabla-card">
        {error && <p className="directorio-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando directorio…</p>
        ) : empleados.length === 0 ? (
          <p className="text-secondary">Todavía no hay nadie registrado.</p>
        ) : (
          <table className="directorio-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Cargo</th>
                {puedeAdministrar && <th>Acciones</th>}
              </tr>
            </thead>
            <tbody>
              {empleados.map((empleado) =>
                editandoId === empleado.id ? (
                  <tr key={empleado.id}>
                    <td>
                      <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} />
                    </td>
                    <td>
                      <input value={cargoEdit} onChange={(event) => setCargoEdit(event.target.value)} />
                    </td>
                    <td className="directorio-acciones">
                      <button type="button" onClick={() => void guardarEdicion(empleado.id)} disabled={guardando}>
                        {guardando ? 'Guardando…' : 'Guardar'}
                      </button>
                      <button type="button" onClick={() => setEditandoId(null)} disabled={guardando}>
                        Cancelar
                      </button>
                    </td>
                  </tr>
                ) : (
                  <tr key={empleado.id}>
                    <td>{empleado.nombreCompleto}</td>
                    <td className="text-muted">{empleado.cargo ?? '—'}</td>
                    {puedeAdministrar && (
                      <td className="directorio-acciones">
                        <button type="button" onClick={() => iniciarEdicion(empleado)}>
                          Editar
                        </button>
                        <button
                          type="button"
                          className="directorio-boton-peligro"
                          onClick={() => void eliminar(empleado)}
                        >
                          Eliminar
                        </button>
                      </td>
                    )}
                  </tr>
                ),
              )}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
