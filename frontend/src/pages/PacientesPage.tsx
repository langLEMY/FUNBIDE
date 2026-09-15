import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PacienteRow } from '../components/pacientes/PacienteRow'
import { ImportarExcel } from '../components/ImportarExcel'
import { Boton } from '../components/ui/Boton'
import { CampoTexto } from '../components/ui/CampoTexto'
import { Tooltip } from '../components/ui/Tooltip'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Paciente, PacientesPaginados, ImportarPacientesResultado } from '../types/paciente'
import { ESTADOS_PACIENTE, type EstadoPaciente } from '../types/paciente'
import { esquemaCrearPaciente, type DatosCrearPaciente } from '../schemas/paciente'
import { formatearCedulaEnVivo, formatearTelefonoEnVivo } from '../utils/mascaras'
import './PacientesPage.css'

const FILTRO_TODOS = 'Todos'
const TAMANO_PAGINA = 50
const TAMANO_VENTANA_PAGINACION = 10

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
  const puedeVerHistorial = perfil?.rol === 'Doctor' || perfil?.rol === 'Admin'
  const puedeCrear = Boolean(perfil)
  const puedeImportar = perfil?.rol === 'Lemy' || perfil?.rol === 'Admin'
  const puedeSubirFotoCedula = perfil?.rol === 'Lemy'
  const puedeVerFotoCedula = perfil?.rol === 'Lemy' || perfil?.rol === 'Admin'

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

  const [errorCrear, setErrorCrear] = useState<string | null>(null)
  const {
    register: registrarCampoPaciente,
    handleSubmit: handleSubmitCrearPaciente,
    reset: resetFormularioPaciente,
    formState: { errors: erroresCrearPaciente, isSubmitting: creando },
  } = useForm<DatosCrearPaciente>({
    resolver: zodResolver(esquemaCrearPaciente),
    defaultValues: { nombre: '', apellido: '', cedula: '', telefono: '' },
  })

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

  const handleCrear = async (datos: DatosCrearPaciente) => {
    setErrorCrear(null)
    try {
      await api.post<Paciente>('/api/pacientes', {
        nombre: datos.nombre,
        apellido: datos.apellido,
        cedula: datos.cedula,
        telefono: datos.telefono?.trim() || null,
        edad: null,
        condicion: null,
      })
      resetFormularioPaciente()
      if (pagina === 1) {
        recargar()
      } else {
        setPagina(1)
      }
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar al paciente.')
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
  const inicioVentana = Math.floor((pagina - 1) / TAMANO_VENTANA_PAGINACION) * TAMANO_VENTANA_PAGINACION + 1
  const finVentana = Math.min(inicioVentana + TAMANO_VENTANA_PAGINACION - 1, totalPaginas)
  const numerosPagina = Array.from({ length: finVentana - inicioVentana + 1 }, (_, i) => inicioVentana + i)

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
          <form className="pacientes-crear-form" onSubmit={handleSubmitCrearPaciente(handleCrear)}>
            <CampoTexto
              etiqueta="Nombre"
              obligatorio
              registro={registrarCampoPaciente('nombre')}
              error={erroresCrearPaciente.nombre?.message}
            />
            <CampoTexto
              etiqueta="Apellido"
              obligatorio
              registro={registrarCampoPaciente('apellido')}
              error={erroresCrearPaciente.apellido?.message}
            />
            <CampoTexto
              etiqueta="Cédula"
              obligatorio
              registro={registrarCampoPaciente('cedula')}
              error={erroresCrearPaciente.cedula?.message}
              mascara={formatearCedulaEnVivo}
              inputMode="numeric"
              ayuda={<Tooltip texto="Formato dominicano: 000-0000000-0. Se completa solo mientras escribís." />}
            />
            <CampoTexto
              etiqueta="Teléfono"
              registro={registrarCampoPaciente('telefono')}
              mascara={formatearTelefonoEnVivo}
              inputMode="numeric"
            />
            <Boton type="submit" cargando={creando}>
              Agregar
            </Boton>
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
          <p className="text-secondary cargando-pulso">Cargando pacientes…</p>
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
                    {(puedeEditar || puedeEliminar || puedeVerHistorial || puedeSubirFotoCedula || puedeVerFotoCedula) && (
                      <th>Acciones</th>
                    )}
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
                      puedeSubirFotoCedula={puedeSubirFotoCedula}
                      puedeVerFotoCedula={puedeVerFotoCedula}
                      onActualizado={actualizarEnLista}
                      onEliminado={quitarDeLista}
                    />
                  ))}
                </tbody>
              </table>
            </div>
            <div className="pacientes-paginacion">
              <button
                type="button"
                className="pacientes-paginacion-salto"
                onClick={() => setPagina((p) => p - 1)}
                disabled={pagina <= 1}
              >
                ‹ Anterior
              </button>
              <div className="pacientes-paginacion-numeros">
                {numerosPagina.map((numero) => (
                  <button
                    key={numero}
                    type="button"
                    className={`pacientes-paginacion-numero${numero === pagina ? ' activo' : ''}`}
                    aria-current={numero === pagina ? 'page' : undefined}
                    onClick={() => setPagina(numero)}
                  >
                    {numero}
                  </button>
                ))}
              </div>
              <button
                type="button"
                className="pacientes-paginacion-salto"
                onClick={() => setPagina((p) => p + 1)}
                disabled={pagina >= totalPaginas}
              >
                Siguiente ›
              </button>
            </div>
            <div className="pacientes-paginacion-info text-secondary">
              Página {pagina} de {totalPaginas} · {total} pacientes
            </div>
          </>
        )}
      </section>
    </DashboardLayout>
  )
}
