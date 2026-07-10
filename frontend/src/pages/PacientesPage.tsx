import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PacienteRow } from '../components/pacientes/PacienteRow'
import { ImportarExcel } from '../components/ImportarExcel'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Paciente, PacientesPaginados, ImportarPacientesResultado } from '../types/paciente'
import { ESTADOS_PACIENTE, type EstadoPaciente } from '../types/paciente'
import './PacientesPage.css'

const FILTRO_TODOS = 'Todos'
const TAMANO_PAGINA = 50

function construirQuery(pagina: number, busqueda: string, filtroEstado: EstadoPaciente | typeof FILTRO_TODOS): string {
  const params = new URLSearchParams()
  params.set('pagina', String(pagina))
  params.set('tamanoPagina', String(TAMANO_PAGINA))
  if (busqueda.trim()) params.set('busqueda', busqueda.trim())
  if (filtroEstado !== FILTRO_TODOS) params.set('estado', filtroEstado)
  return params.toString()
}

export function PacientesPage() {
  const { perfil } = useAuth()
  const puedeEditar = perfil?.rol === 'Lemy'
  const puedeEliminar = perfil?.rol === 'Lemy' || perfil?.rol === 'Admin' || perfil?.rol === 'Doctor'
  const puedeVerHistorial = perfil?.rol === 'Doctor'
  const puedeCrear = Boolean(perfil)
  const puedeImportar = perfil?.rol === 'Lemy' || perfil?.rol === 'Admin'

  const [pacientes, setPacientes] = useState<Paciente[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [resultadoImportacion, setResultadoImportacion] = useState<ImportarPacientesResultado | null>(null)
  const [recargarClave, setRecargarClave] = useState(0)

  const [busqueda, setBusqueda] = useState('')
  const [busquedaDebounced, setBusquedaDebounced] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<EstadoPaciente | typeof FILTRO_TODOS>(FILTRO_TODOS)

  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [cedula, setCedula] = useState('')
  const [telefono, setTelefono] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda), 300)
    return () => clearTimeout(temporizador)
  }, [busqueda])

  useEffect(() => {
    setPagina(1)
  }, [busquedaDebounced, filtroEstado])

  useEffect(() => {
    let cancelado = false

    setCargando(true)
    api
      .get<PacientesPaginados>(`/api/pacientes?${construirQuery(pagina, busquedaDebounced, filtroEstado)}`)
      .then((datos) => {
        if (cancelado) return
        if (datos.items.length === 0 && datos.pagina > 1) {
          setPagina((actual) => actual - 1)
          return
        }
        setPacientes(datos.items)
        setTotal(datos.total)
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
  }, [pagina, busquedaDebounced, filtroEstado, recargarClave])

  const recargar = () => setRecargarClave((clave) => clave + 1)

  const actualizarEnLista = (_paciente: Paciente) => recargar()

  const quitarDeLista = (_pacienteId: string) => recargar()

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)
    setCreando(true)
    try {
      await api.post<Paciente>('/api/pacientes', {
        nombre,
        apellido,
        cedula,
        telefono: telefono.trim() || null,
        edad: null,
        condicion: null,
      })
      setNombre('')
      setApellido('')
      setCedula('')
      setTelefono('')
      if (pagina === 1) {
        recargar()
      } else {
        setPagina(1)
      }
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar al paciente.')
    } finally {
      setCreando(false)
    }
  }

  const handleImportado = (resultado: ImportarPacientesResultado) => {
    setResultadoImportacion(resultado)
    if (pagina === 1) {
      recargar()
    } else {
      setPagina(1)
    }
  }

  const totalPaginas = Math.max(1, Math.ceil(total / TAMANO_PAGINA))

  return (
    <DashboardLayout titulo="Pacientes">
      {puedeImportar && (
        <section className="pacientes-crear-card">
          <h2>Importar pacientes desde Excel</h2>
          <ImportarExcel<ImportarPacientesResultado>
            endpoint="/api/pacientes/importar"
            onImportado={handleImportado}
          />
          {resultadoImportacion && (
            <div className="pacientes-importar-resumen">
              <p>
                {resultadoImportacion.creados} pacientes creados y {resultadoImportacion.actualizados} actualizados
                de {resultadoImportacion.totalFilas} filas.
                {resultadoImportacion.identificacionesAjustadas > 0 &&
                  ` ${resultadoImportacion.identificacionesAjustadas} identificaciones se ajustaron automáticamente (muy cortas o duplicadas).`}
                {resultadoImportacion.omitidos > 0 && ` ${resultadoImportacion.omitidos} filas omitidas.`}
              </p>
              {resultadoImportacion.omisiones.length > 0 && (
                <ul className="pacientes-importar-omisiones">
                  {resultadoImportacion.omisiones.map((omision, indice) => (
                    <li key={indice}>{omision}</li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </section>
      )}

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
        ) : pacientes.length === 0 ? (
          <p className="text-secondary">
            {total === 0 && !busqueda.trim() && filtroEstado === FILTRO_TODOS
              ? 'Todavía no hay pacientes registrados.'
              : 'No hay pacientes que coincidan con la búsqueda o el filtro.'}
          </p>
        ) : (
          <>
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
                  {pacientes.map((paciente) => (
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
            <div className="pacientes-paginacion">
              <button type="button" onClick={() => setPagina((p) => p - 1)} disabled={pagina <= 1}>
                Anterior
              </button>
              <span className="text-secondary">
                Página {pagina} de {totalPaginas} · {total} pacientes
              </span>
              <button
                type="button"
                onClick={() => setPagina((p) => p + 1)}
                disabled={pagina >= totalPaginas}
              >
                Siguiente
              </button>
            </div>
          </>
        )}
      </section>
    </DashboardLayout>
  )
}
