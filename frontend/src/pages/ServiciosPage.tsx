import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { ImportarExcel } from '../components/ImportarExcel'
import { api, ApiError } from '../lib/api'
import type { CrearServicioRequest, EditarServicioRequest, ImportarServiciosResult, Servicio } from '../types/servicio'
import type { EspecialidadMedica } from '../types/usuario'
import { ESPECIALIDADES, ETIQUETA_ESPECIALIDAD } from '../types/personal'
import './ServiciosPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
})

const SIN_ESPECIALIDAD = ''

export function ServiciosPage() {
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [precio1, setPrecio1] = useState('')
  const [precio2, setPrecio2] = useState('')
  const [precio3, setPrecio3] = useState('')
  const [especialidad, setEspecialidad] = useState<EspecialidadMedica | typeof SIN_ESPECIALIDAD>(SIN_ESPECIALIDAD)
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  const [editandoId, setEditandoId] = useState<string | null>(null)
  const [edicion, setEdicion] = useState<{
    nombre: string
    precio1: string
    precio2: string
    precio3: string
    especialidad: EspecialidadMedica | typeof SIN_ESPECIALIDAD
  } | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [errorFila, setErrorFila] = useState<string | null>(null)
  const [procesandoId, setProcesandoId] = useState<string | null>(null)
  const [busqueda, setBusqueda] = useState('')
  const [resultadoImport, setResultadoImport] = useState<ImportarServiciosResult | null>(null)

  const cargar = () => {
    setCargando(true)
    api
      .get<Servicio[]>('/api/servicios?incluirInactivos=true')
      .then((datos) => setServicios(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el catálogo de servicios.')
      })
      .finally(() => setCargando(false))
  }

  useEffect(cargar, [])

  const serviciosFiltrados = servicios.filter((s) => {
    const texto = busqueda.trim().toLowerCase()
    if (!texto) return true
    return s.nombre.toLowerCase().includes(texto) || s.codigo.toLowerCase().includes(texto)
  })

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)

    if (!codigo.trim() || !nombre.trim()) {
      setErrorCrear('El código y el nombre son obligatorios.')
      return
    }

    setCreando(true)
    try {
      const request: CrearServicioRequest = {
        codigo: codigo.trim(),
        nombre: nombre.trim(),
        precio1: Number(precio1) || 0,
        precio2: Number(precio2) || 0,
        precio3: Number(precio3) || 0,
        especialidad: especialidad || null,
      }
      const nuevo = await api.post<Servicio>('/api/servicios', request)
      setServicios((actual) => [...actual, nuevo].sort((a, b) => a.nombre.localeCompare(b.nombre)))
      setCodigo('')
      setNombre('')
      setPrecio1('')
      setPrecio2('')
      setPrecio3('')
      setEspecialidad(SIN_ESPECIALIDAD)
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo crear el servicio.')
    } finally {
      setCreando(false)
    }
  }

  const iniciarEdicion = (servicio: Servicio) => {
    setEditandoId(servicio.id)
    setEdicion({
      nombre: servicio.nombre,
      precio1: String(servicio.precio1),
      precio2: String(servicio.precio2),
      precio3: String(servicio.precio3),
      especialidad: servicio.especialidad ?? SIN_ESPECIALIDAD,
    })
    setErrorFila(null)
  }

  const guardarEdicion = async (servicioId: string) => {
    if (!edicion || !edicion.nombre.trim()) {
      setErrorFila('El nombre es obligatorio.')
      return
    }

    setGuardando(true)
    setErrorFila(null)
    try {
      const request: EditarServicioRequest = {
        servicioId,
        nombre: edicion.nombre.trim(),
        precio1: Number(edicion.precio1) || 0,
        precio2: Number(edicion.precio2) || 0,
        precio3: Number(edicion.precio3) || 0,
        especialidad: edicion.especialidad || null,
      }
      const actualizado = await api.patch<Servicio>('/api/servicios', request)
      setServicios((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
      setEditandoId(null)
    } catch (err) {
      setErrorFila(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar el servicio.')
    } finally {
      setGuardando(false)
    }
  }

  const alternarActivo = async (servicio: Servicio) => {
    setProcesandoId(servicio.id)
    try {
      const actualizado = servicio.activo
        ? await api.patch<Servicio>(`/api/servicios/${servicio.id}/desactivar`)
        : await api.patch<Servicio>(`/api/servicios/${servicio.id}/reactivar`)
      setServicios((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo actualizar el servicio.')
    } finally {
      setProcesandoId(null)
    }
  }

  return (
    <DashboardLayout titulo="Precios privados">
      <p className="text-secondary servicios-subtitulo">
        Catálogo de precios para pacientes que pagan de su bolsillo, sin seguro médico. Se usa como lista de opciones
        al cobrar en Caja, en vez de escribir el concepto y el monto a mano.
      </p>

      <section className="servicios-crear-card">
        <h2>Agregar servicio</h2>
        <form className="servicios-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <input placeholder="Código" value={codigo} onChange={(event) => setCodigo(event.target.value)} required />
          <input placeholder="Nombre" value={nombre} onChange={(event) => setNombre(event.target.value)} required />
          <input
            type="number"
            min={0}
            step="0.01"
            placeholder="Precio 1"
            value={precio1}
            onChange={(event) => setPrecio1(event.target.value)}
          />
          <input
            type="number"
            min={0}
            step="0.01"
            placeholder="Precio 2"
            value={precio2}
            onChange={(event) => setPrecio2(event.target.value)}
          />
          <input
            type="number"
            min={0}
            step="0.01"
            placeholder="Precio 3"
            value={precio3}
            onChange={(event) => setPrecio3(event.target.value)}
          />
          <select value={especialidad} onChange={(event) => setEspecialidad(event.target.value as EspecialidadMedica)}>
            <option value={SIN_ESPECIALIDAD}>Sin especialidad</option>
            {ESPECIALIDADES.map((opcion) => (
              <option key={opcion} value={opcion}>
                {ETIQUETA_ESPECIALIDAD[opcion]}
              </option>
            ))}
          </select>
          <button type="submit" disabled={creando}>
            {creando ? 'Agregando…' : 'Agregar'}
          </button>
        </form>
        {errorCrear && <p className="servicios-error">{errorCrear}</p>}
      </section>

      <section className="servicios-tabla-card">
        <div className="servicios-filtros">
          <input
            type="search"
            placeholder="Buscar por nombre o código…"
            value={busqueda}
            onChange={(event) => setBusqueda(event.target.value)}
          />
          <ImportarExcel<ImportarServiciosResult>
            endpoint="/api/servicios/importar"
            onImportado={(resultado) => {
              setResultadoImport(resultado)
              cargar()
            }}
          />
        </div>
        {resultadoImport && (
          <p className="text-secondary servicios-resultado-import">
            {resultadoImport.creados} creados, {resultadoImport.actualizados} actualizados
            {resultadoImport.omitidos > 0 ? `, ${resultadoImport.omitidos} omitidos` : ''} de {resultadoImport.totalFilas} filas.
            {resultadoImport.omisiones.length > 0 && (
              <>
                {' '}
                <details>
                  <summary>Ver detalle de omisiones</summary>
                  <ul>
                    {resultadoImport.omisiones.map((omision, indice) => (
                      <li key={indice}>{omision}</li>
                    ))}
                  </ul>
                </details>
              </>
            )}
          </p>
        )}

        {error && <p className="servicios-error">{error}</p>}
        {errorFila && <p className="servicios-error">{errorFila}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando servicios…</p>
        ) : serviciosFiltrados.length === 0 ? (
          <p className="text-secondary">
            {servicios.length === 0
              ? 'Todavía no hay servicios registrados. Agrégalos a mano o importalos desde Excel.'
              : 'Ningún servicio coincide con la búsqueda.'}
          </p>
        ) : (
          <table className="servicios-tabla">
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th>Especialidad</th>
                <th>Precio 1</th>
                <th>Precio 2</th>
                <th>Precio 3</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {serviciosFiltrados.map((servicio) =>
                editandoId === servicio.id && edicion ? (
                  <tr key={servicio.id}>
                    <td>{servicio.codigo}</td>
                    <td>
                      <input value={edicion.nombre} onChange={(event) => setEdicion({ ...edicion, nombre: event.target.value })} />
                    </td>
                    <td>
                      <select
                        value={edicion.especialidad}
                        onChange={(event) => setEdicion({ ...edicion, especialidad: event.target.value as EspecialidadMedica })}
                      >
                        <option value={SIN_ESPECIALIDAD}>Sin especialidad</option>
                        {ESPECIALIDADES.map((opcion) => (
                          <option key={opcion} value={opcion}>
                            {ETIQUETA_ESPECIALIDAD[opcion]}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={edicion.precio1}
                        onChange={(event) => setEdicion({ ...edicion, precio1: event.target.value })}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={edicion.precio2}
                        onChange={(event) => setEdicion({ ...edicion, precio2: event.target.value })}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={edicion.precio3}
                        onChange={(event) => setEdicion({ ...edicion, precio3: event.target.value })}
                      />
                    </td>
                    <td className="text-muted">{servicio.activo ? 'Activo' : 'Inactivo'}</td>
                    <td className="servicios-acciones">
                      <button type="button" onClick={() => void guardarEdicion(servicio.id)} disabled={guardando}>
                        {guardando ? 'Guardando…' : 'Guardar'}
                      </button>
                      <button type="button" onClick={() => setEditandoId(null)} disabled={guardando}>
                        Cancelar
                      </button>
                    </td>
                  </tr>
                ) : (
                  <tr key={servicio.id} className={servicio.activo ? '' : 'servicios-fila-inactiva'}>
                    <td>{servicio.codigo}</td>
                    <td>{servicio.nombre}</td>
                    <td>{servicio.especialidad ? ETIQUETA_ESPECIALIDAD[servicio.especialidad] : '—'}</td>
                    <td>{formateadorMoneda.format(servicio.precio1)}</td>
                    <td>{formateadorMoneda.format(servicio.precio2)}</td>
                    <td>{formateadorMoneda.format(servicio.precio3)}</td>
                    <td>
                      <span className={`servicios-estado ${servicio.activo ? 'activo' : 'inactivo'}`}>
                        {servicio.activo ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td className="servicios-acciones">
                      <button type="button" onClick={() => iniciarEdicion(servicio)}>
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => void alternarActivo(servicio)}
                        disabled={procesandoId === servicio.id}
                      >
                        {servicio.activo ? 'Desactivar' : 'Reactivar'}
                      </button>
                    </td>
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
