import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { ImportarExcel } from '../components/ImportarExcel'
import { api, ApiError } from '../lib/api'
import type { SeguroMedico } from '../types/seguroMedico'
import {
  ETIQUETA_PLAN,
  PLANES_ASEGURADORA,
  type ImportarTarifarioResult,
  type PlanAseguradora,
  type TarifarioProcedimiento,
} from '../types/tarifarioProcedimiento'
import type { EspecialidadMedica } from '../types/usuario'
import { ESPECIALIDADES, ETIQUETA_ESPECIALIDAD } from '../types/personal'
import './AseguradorasPage.css'

const SIN_ESPECIALIDAD = ''

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
})

export function AseguradorasPage() {
  const [seguros, setSeguros] = useState<SeguroMedico[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [nombre, setNombre] = useState('')
  const [porcentaje, setPorcentaje] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  const [editandoId, setEditandoId] = useState<string | null>(null)
  const [nombreEdit, setNombreEdit] = useState('')
  const [porcentajeEdit, setPorcentajeEdit] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [errorFila, setErrorFila] = useState<string | null>(null)
  const [procesandoId, setProcesandoId] = useState<string | null>(null)

  const [seguroTarifarioId, setSeguroTarifarioId] = useState('')
  const [planTarifario, setPlanTarifario] = useState<PlanAseguradora>('Contributivo')
  const [tarifario, setTarifario] = useState<TarifarioProcedimiento[]>([])
  const [cargandoTarifario, setCargandoTarifario] = useState(false)
  const [errorTarifario, setErrorTarifario] = useState<string | null>(null)
  const [busquedaTarifario, setBusquedaTarifario] = useState('')
  const [resultadoImport, setResultadoImport] = useState<ImportarTarifarioResult | null>(null)

  const [editandoTarifarioId, setEditandoTarifarioId] = useState<string | null>(null)
  const [montoSeguroEdit, setMontoSeguroEdit] = useState('')
  const [montoPacienteEdit, setMontoPacienteEdit] = useState('')
  const [montoTotalEdit, setMontoTotalEdit] = useState('')
  const [montoFondoEdit, setMontoFondoEdit] = useState('')
  const [guardandoTarifario, setGuardandoTarifario] = useState(false)
  const [errorFilaTarifario, setErrorFilaTarifario] = useState<string | null>(null)

  const [procedimientoNuevo, setProcedimientoNuevo] = useState('')
  const [montoSeguroNuevo, setMontoSeguroNuevo] = useState('')
  const [montoPacienteNuevo, setMontoPacienteNuevo] = useState('')
  const [montoTotalNuevo, setMontoTotalNuevo] = useState('')
  const [montoFondoNuevo, setMontoFondoNuevo] = useState('')
  const [especialidadNueva, setEspecialidadNueva] = useState<EspecialidadMedica | typeof SIN_ESPECIALIDAD>(SIN_ESPECIALIDAD)
  const [creandoTarifario, setCreandoTarifario] = useState(false)
  const [errorCrearTarifario, setErrorCrearTarifario] = useState<string | null>(null)

  const cargar = () => {
    setCargando(true)
    api
      .get<SeguroMedico[]>('/api/seguros-medicos?incluirInactivos=true')
      .then((datos) => setSeguros(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el catálogo de aseguradoras.')
      })
      .finally(() => setCargando(false))
  }

  useEffect(cargar, [])

  useEffect(() => {
    if (seguroTarifarioId || seguros.length === 0) return
    // Preselecciona SENASA si existe; si no, la primera aseguradora de la lista.
    const senasa = seguros.find((s) => s.nombre.toUpperCase().includes('SENASA'))
    setSeguroTarifarioId((senasa ?? seguros[0]).id)
  }, [seguros, seguroTarifarioId])

  // El selector de plan queda siempre visible (no solo para Senasa): cualquier aseguradora
  // nueva puede tener su propia subdivisión de planes, y "Estandar" sigue disponible para
  // las que no la tienen (Renacer, Aps). Al cambiar de aseguradora se resetea a Estandar en
  // vez de arrastrar el plan de la aseguradora anterior.
  useEffect(() => {
    setPlanTarifario('Estandar')
  }, [seguroTarifarioId])

  const cargarTarifario = () => {
    if (!seguroTarifarioId) return
    setCargandoTarifario(true)
    setErrorTarifario(null)
    api
      .get<TarifarioProcedimiento[]>(`/api/tarifario-procedimientos?seguroMedicoId=${seguroTarifarioId}&plan=${planTarifario}`)
      .then((datos) => setTarifario(datos))
      .catch((err) => {
        setErrorTarifario(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el tarifario.')
      })
      .finally(() => setCargandoTarifario(false))
  }

  useEffect(cargarTarifario, [seguroTarifarioId, planTarifario])

  const tarifarioFiltrado = useMemo(() => {
    const texto = busquedaTarifario.trim().toLowerCase()
    if (!texto) return tarifario
    return tarifario.filter((t) => t.procedimiento.toLowerCase().includes(texto))
  }, [tarifario, busquedaTarifario])

  const iniciarEdicionTarifario = (fila: TarifarioProcedimiento) => {
    setEditandoTarifarioId(fila.id)
    setMontoSeguroEdit(String(fila.montoSeguro))
    setMontoPacienteEdit(String(fila.montoPaciente))
    setMontoTotalEdit(String(fila.montoTotal))
    setMontoFondoEdit(fila.montoFondo ? String(fila.montoFondo) : '')
    setErrorFilaTarifario(null)
  }

  const guardarEdicionTarifario = async (fila: TarifarioProcedimiento) => {
    const montoSeguroNumero = Number(montoSeguroEdit)
    const montoPacienteNumero = Number(montoPacienteEdit)
    const montoTotalNumero = Number(montoTotalEdit)
    const montoFondoNumero = montoFondoEdit.trim() ? Number(montoFondoEdit) : null

    if (!Number.isFinite(montoSeguroNumero) || montoSeguroNumero < 0) {
      setErrorFilaTarifario('El monto que cubre el seguro no puede ser negativo.')
      return
    }
    if (!Number.isFinite(montoPacienteNumero) || montoPacienteNumero < 0) {
      setErrorFilaTarifario('El monto a cargo del paciente no puede ser negativo.')
      return
    }
    if (!Number.isFinite(montoTotalNumero) || montoTotalNumero <= 0) {
      setErrorFilaTarifario('El monto total debe ser mayor que cero.')
      return
    }
    if (montoFondoNumero !== null && (!Number.isFinite(montoFondoNumero) || montoFondoNumero < 0)) {
      setErrorFilaTarifario('La ganancia para el fondo interno no puede ser negativa.')
      return
    }

    setGuardandoTarifario(true)
    setErrorFilaTarifario(null)
    try {
      const actualizada = await api.patch<TarifarioProcedimiento>('/api/tarifario-procedimientos', {
        tarifarioProcedimientoId: fila.id,
        montoSeguro: montoSeguroNumero,
        montoPaciente: montoPacienteNumero,
        montoTotal: montoTotalNumero,
        montoFondo: montoFondoNumero,
        especialidad: fila.especialidad,
      })
      setTarifario((actual) => actual.map((t) => (t.id === actualizada.id ? actualizada : t)))
      setEditandoTarifarioId(null)
    } catch (err) {
      setErrorFilaTarifario(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar el procedimiento.')
    } finally {
      setGuardandoTarifario(false)
    }
  }

  const handleCrearTarifario = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrearTarifario(null)

    const montoSeguroNumero = Number(montoSeguroNuevo)
    const montoPacienteNumero = Number(montoPacienteNuevo)
    const montoTotalNumero = Number(montoTotalNuevo)
    const montoFondoNumero = montoFondoNuevo.trim() ? Number(montoFondoNuevo) : null

    if (!procedimientoNuevo.trim()) {
      setErrorCrearTarifario('El nombre del procedimiento es obligatorio.')
      return
    }
    if (!Number.isFinite(montoSeguroNumero) || montoSeguroNumero < 0) {
      setErrorCrearTarifario('El monto que cubre el seguro no puede ser negativo.')
      return
    }
    if (!Number.isFinite(montoPacienteNumero) || montoPacienteNumero < 0) {
      setErrorCrearTarifario('El monto a cargo del paciente no puede ser negativo.')
      return
    }
    if (!Number.isFinite(montoTotalNumero) || montoTotalNumero <= 0) {
      setErrorCrearTarifario('El monto total debe ser mayor que cero.')
      return
    }
    if (montoFondoNumero !== null && (!Number.isFinite(montoFondoNumero) || montoFondoNumero < 0)) {
      setErrorCrearTarifario('La ganancia para el fondo interno no puede ser negativa.')
      return
    }

    setCreandoTarifario(true)
    try {
      const creado = await api.post<TarifarioProcedimiento>('/api/tarifario-procedimientos', {
        seguroMedicoId: seguroTarifarioId,
        plan: planTarifario,
        procedimiento: procedimientoNuevo.trim(),
        montoSeguro: montoSeguroNumero,
        montoPaciente: montoPacienteNumero,
        montoTotal: montoTotalNumero,
        montoFondo: montoFondoNumero,
        especialidad: especialidadNueva || null,
      })
      setTarifario((actual) => [...actual, creado].sort((a, b) => a.procedimiento.localeCompare(b.procedimiento)))
      setProcedimientoNuevo('')
      setMontoSeguroNuevo('')
      setMontoPacienteNuevo('')
      setMontoTotalNuevo('')
      setMontoFondoNuevo('')
      setEspecialidadNueva(SIN_ESPECIALIDAD)
    } catch (err) {
      setErrorCrearTarifario(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar el procedimiento.')
    } finally {
      setCreandoTarifario(false)
    }
  }

  const importarEndpoint = seguroTarifarioId
    ? `/api/tarifario-procedimientos/importar?seguroMedicoId=${seguroTarifarioId}&plan=${planTarifario}`
    : ''

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)

    const porcentajeNumero = Number(porcentaje)
    if (!nombre.trim()) {
      setErrorCrear('El nombre es obligatorio.')
      return
    }
    if (!porcentaje.trim() || !Number.isFinite(porcentajeNumero) || porcentajeNumero <= 0 || porcentajeNumero > 100) {
      setErrorCrear('Ingresa un porcentaje de cobertura entre 1 y 100.')
      return
    }

    setCreando(true)
    try {
      const nueva = await api.post<SeguroMedico>('/api/seguros-medicos', {
        nombre: nombre.trim(),
        porcentajeCobertura: porcentajeNumero,
      })
      setSeguros((actual) => [...actual, nueva].sort((a, b) => a.nombre.localeCompare(b.nombre)))
      setNombre('')
      setPorcentaje('')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo crear la aseguradora.')
    } finally {
      setCreando(false)
    }
  }

  const iniciarEdicion = (seguro: SeguroMedico) => {
    setEditandoId(seguro.id)
    setNombreEdit(seguro.nombre)
    setPorcentajeEdit(String(seguro.porcentajeCobertura))
    setErrorFila(null)
  }

  const guardarEdicion = async (seguroMedicoId: string) => {
    const porcentajeNumero = Number(porcentajeEdit)
    if (!nombreEdit.trim() || !Number.isFinite(porcentajeNumero) || porcentajeNumero <= 0 || porcentajeNumero > 100) {
      setErrorFila('Ingresa un nombre y un porcentaje entre 1 y 100.')
      return
    }

    setGuardando(true)
    setErrorFila(null)
    try {
      const actualizado = await api.patch<SeguroMedico>('/api/seguros-medicos', {
        seguroMedicoId,
        nombre: nombreEdit.trim(),
        porcentajeCobertura: porcentajeNumero,
      })
      setSeguros((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
      setEditandoId(null)
    } catch (err) {
      setErrorFila(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar la aseguradora.')
    } finally {
      setGuardando(false)
    }
  }

  const alternarActivo = async (seguro: SeguroMedico) => {
    setProcesandoId(seguro.id)
    try {
      const actualizado = seguro.activo
        ? await api.patch<SeguroMedico>(`/api/seguros-medicos/${seguro.id}/desactivar`)
        : await api.patch<SeguroMedico>(`/api/seguros-medicos/${seguro.id}/reactivar`)
      setSeguros((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo actualizar la aseguradora.')
    } finally {
      setProcesandoId(null)
    }
  }

  return (
    <DashboardLayout titulo="Aseguradoras">
      <section className="aseguradoras-crear-card">
        <h2>Agregar aseguradora (ARS)</h2>
        <form className="aseguradoras-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <input placeholder="Nombre" value={nombre} onChange={(event) => setNombre(event.target.value)} required />
          <input
            type="number"
            min={1}
            max={100}
            step="0.01"
            placeholder="% de cobertura"
            value={porcentaje}
            onChange={(event) => setPorcentaje(event.target.value)}
            required
          />
          <button type="submit" disabled={creando}>
            {creando ? 'Agregando…' : 'Agregar'}
          </button>
        </form>
        {errorCrear && <p className="aseguradoras-error">{errorCrear}</p>}
      </section>

      <section className="aseguradoras-tabla-card">
        {error && <p className="aseguradoras-error">{error}</p>}
        {errorFila && <p className="aseguradoras-error">{errorFila}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando aseguradoras…</p>
        ) : seguros.length === 0 ? (
          <p className="text-secondary">Todavía no hay aseguradoras registradas.</p>
        ) : (
          <table className="aseguradoras-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>% Cobertura</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {seguros.map((seguro) =>
                editandoId === seguro.id ? (
                  <tr key={seguro.id}>
                    <td>
                      <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={1}
                        max={100}
                        step="0.01"
                        value={porcentajeEdit}
                        onChange={(event) => setPorcentajeEdit(event.target.value)}
                      />
                    </td>
                    <td className="text-muted">{seguro.activo ? 'Activo' : 'Inactivo'}</td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => void guardarEdicion(seguro.id)} disabled={guardando}>
                        {guardando ? 'Guardando…' : 'Guardar'}
                      </button>
                      <button type="button" onClick={() => setEditandoId(null)} disabled={guardando}>
                        Cancelar
                      </button>
                    </td>
                  </tr>
                ) : (
                  <tr key={seguro.id} className={seguro.activo ? '' : 'aseguradoras-fila-inactiva'}>
                    <td>{seguro.nombre}</td>
                    <td>{seguro.porcentajeCobertura}%</td>
                    <td>
                      <span className={`aseguradoras-estado ${seguro.activo ? 'activo' : 'inactivo'}`}>
                        {seguro.activo ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => iniciarEdicion(seguro)}>
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => void alternarActivo(seguro)}
                        disabled={procesandoId === seguro.id}
                      >
                        {seguro.activo ? 'Desactivar' : 'Reactivar'}
                      </button>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
        )}
      </section>

      <section className="aseguradoras-tabla-card">
        <h2>Tarifario por procedimiento</h2>
        <p className="text-secondary aseguradoras-tarifario-subtitulo">
          Montos fijos negociados por procedimiento (Senasa con sus 3 planes; Renacer y Aps con un único plan).
          Al cobrar con esta aseguradora, el procedimiento elegido define el monto exacto — no un porcentaje.
          La columna "Ganancia (fondo interno)" es el excedente que paga la aseguradora por encima de lo reconocido
          al paciente: no se le cobra a nadie, entra directo como ingreso interno de la fundación en cada cobro.
          Editá cualquier fila para ajustarla sin tener que rehacer el import de Excel.
        </p>

        <div className="aseguradoras-tarifario-filtros">
          <label className="aseguradoras-tarifario-label">
            Aseguradora
            <select value={seguroTarifarioId} onChange={(event) => setSeguroTarifarioId(event.target.value)}>
              {seguros.map((seguro) => (
                <option key={seguro.id} value={seguro.id}>
                  {seguro.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="aseguradoras-tarifario-label">
            Plan
            <select value={planTarifario} onChange={(event) => setPlanTarifario(event.target.value as PlanAseguradora)}>
              {PLANES_ASEGURADORA.map((plan) => (
                <option key={plan} value={plan}>
                  {ETIQUETA_PLAN[plan]}
                </option>
              ))}
            </select>
          </label>
          <input
            type="search"
            placeholder="Buscar procedimiento…"
            value={busquedaTarifario}
            onChange={(event) => setBusquedaTarifario(event.target.value)}
          />
        </div>

        {seguroTarifarioId && (
          <ImportarExcel<ImportarTarifarioResult>
            endpoint={importarEndpoint}
            onImportado={(resultado) => {
              setResultadoImport(resultado)
              cargarTarifario()
            }}
          />
        )}

        {seguroTarifarioId && (
          <details className="aseguradoras-tarifario-agregar">
            <summary>Agregar un procedimiento a mano (sin Excel)</summary>
            <form className="aseguradoras-crear-form" onSubmit={(event) => void handleCrearTarifario(event)}>
              <input
                placeholder="Nombre del procedimiento"
                value={procedimientoNuevo}
                onChange={(event) => setProcedimientoNuevo(event.target.value)}
                required
              />
              <input
                type="number"
                min={0}
                step="0.01"
                placeholder="Cubre el seguro"
                value={montoSeguroNuevo}
                onChange={(event) => setMontoSeguroNuevo(event.target.value)}
                required
              />
              <input
                type="number"
                min={0}
                step="0.01"
                placeholder="Paga el paciente"
                value={montoPacienteNuevo}
                onChange={(event) => setMontoPacienteNuevo(event.target.value)}
                required
              />
              <input
                type="number"
                min={0}
                step="0.01"
                placeholder="Total"
                value={montoTotalNuevo}
                onChange={(event) => setMontoTotalNuevo(event.target.value)}
                required
              />
              <input
                type="number"
                min={0}
                step="0.01"
                placeholder="Ganancia / fondo interno (opcional)"
                value={montoFondoNuevo}
                onChange={(event) => setMontoFondoNuevo(event.target.value)}
              />
              <select
                value={especialidadNueva}
                onChange={(event) => setEspecialidadNueva(event.target.value as EspecialidadMedica)}
              >
                <option value={SIN_ESPECIALIDAD}>Sin especialidad</option>
                {ESPECIALIDADES.map((opcion) => (
                  <option key={opcion} value={opcion}>
                    {ETIQUETA_ESPECIALIDAD[opcion]}
                  </option>
                ))}
              </select>
              <button type="submit" disabled={creandoTarifario}>
                {creandoTarifario ? 'Agregando…' : 'Agregar procedimiento'}
              </button>
            </form>
            {errorCrearTarifario && <p className="aseguradoras-error">{errorCrearTarifario}</p>}
          </details>
        )}
        {resultadoImport && (
          <p className="text-secondary aseguradoras-tarifario-resultado">
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

        {errorTarifario && <p className="aseguradoras-error">{errorTarifario}</p>}
        {errorFilaTarifario && <p className="aseguradoras-error">{errorFilaTarifario}</p>}

        {cargandoTarifario ? (
          <p className="text-secondary cargando-pulso">Cargando tarifario…</p>
        ) : tarifarioFiltrado.length === 0 ? (
          <p className="text-secondary">
            {tarifario.length === 0
              ? 'Todavía no hay tarifario cargado para este plan. Importalo desde Excel.'
              : 'Ningún procedimiento coincide con la búsqueda.'}
          </p>
        ) : (
          <table className="aseguradoras-tabla">
            <thead>
              <tr>
                <th>Procedimiento</th>
                <th>Cubre el seguro</th>
                <th>Paga el paciente</th>
                <th>Total</th>
                <th>Ganancia (fondo interno)</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {tarifarioFiltrado.map((fila) =>
                editandoTarifarioId === fila.id ? (
                  <tr key={fila.id}>
                    <td>{fila.procedimiento}</td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={montoSeguroEdit}
                        onChange={(event) => setMontoSeguroEdit(event.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={montoPacienteEdit}
                        onChange={(event) => setMontoPacienteEdit(event.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={montoTotalEdit}
                        onChange={(event) => setMontoTotalEdit(event.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        placeholder="Sin ganancia"
                        value={montoFondoEdit}
                        onChange={(event) => setMontoFondoEdit(event.target.value)}
                      />
                    </td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => void guardarEdicionTarifario(fila)} disabled={guardandoTarifario}>
                        {guardandoTarifario ? 'Guardando…' : 'Guardar'}
                      </button>
                      <button type="button" onClick={() => setEditandoTarifarioId(null)} disabled={guardandoTarifario}>
                        Cancelar
                      </button>
                    </td>
                  </tr>
                ) : (
                  <tr key={fila.id}>
                    <td>{fila.procedimiento}</td>
                    <td>{formateadorMoneda.format(fila.montoSeguro)}</td>
                    <td>{formateadorMoneda.format(fila.montoPaciente)}</td>
                    <td>{formateadorMoneda.format(fila.montoTotal)}</td>
                    <td>{fila.montoFondo ? formateadorMoneda.format(fila.montoFondo) : '—'}</td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => iniciarEdicionTarifario(fila)}>
                        Editar
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
