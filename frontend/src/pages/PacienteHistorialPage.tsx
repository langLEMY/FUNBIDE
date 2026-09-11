import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import { imprimirVentana } from '../lib/imprimir'
import type { Paciente } from '../types/paciente'
import type { Usuario } from '../types/usuario'
import { ETIQUETA_ESPECIALIDAD } from '../types/personal'
import {
  ETIQUETA_TIPO_ENTRADA,
  PLACEHOLDER_CUERPO_DOCUMENTO,
  TIPOS_DOCUMENTO,
  TIPOS_RECETA,
  esContenidoDocumento,
  esContenidoReceta,
  parsearContenidoDocumento,
  parsearContenidoNotaClinica,
  parsearContenidoReceta,
  type EntradaHistorial,
  type ItemReceta,
  type TipoEntradaHistorial,
} from '../types/historialClinico'
import './PacienteHistorialPage.css'

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium', timeStyle: 'short' })
const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'long' })

const ITEM_RECETA_VACIO: ItemReceta = { descripcion: '', indicaciones: '' }

export function PacienteHistorialPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { perfil } = useAuth()
  // Admin puede supervisar el historial pero no escribirlo: eso sigue siendo exclusivo
  // del Doctor (ver HistorialClinicoController, POST solo permite RolUsuario.Doctor).
  const puedeRegistrar = perfil?.rol === 'Doctor'

  const [paciente, setPaciente] = useState<Paciente | null>(null)
  const [entradas, setEntradas] = useState<EntradaHistorial[]>([])
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [tipo, setTipo] = useState<TipoEntradaHistorial>('NotaClinica')
  const [diagnostico, setDiagnostico] = useState('')
  const [tratamiento, setTratamiento] = useState('')
  const [notas, setNotas] = useState('')
  const [itemsReceta, setItemsReceta] = useState<ItemReceta[]>([{ ...ITEM_RECETA_VACIO }])
  const [notasGenerales, setNotasGenerales] = useState('')
  const [cuerpoDocumento, setCuerpoDocumento] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorRegistrar, setErrorRegistrar] = useState<string | null>(null)

  const [entradaAImprimir, setEntradaAImprimir] = useState<EntradaHistorial | null>(null)

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<Paciente>(`/api/pacientes/${id}`),
      api.get<EntradaHistorial[]>(`/api/historial-clinico/paciente/${id}`),
      api.get<Usuario[]>('/api/personal'),
    ])
      .then(([pacienteEncontrado, datosEntradas, datosPersonal]) => {
        if (cancelado) return
        setPaciente(pacienteEncontrado)
        setEntradas(datosEntradas)
        setPersonal(datosPersonal)
      })
      .catch((err) => {
        if (cancelado) return
        if (err instanceof ApiError && err.status === 404) {
          setError('No se encontró a ese paciente.')
        } else {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el historial clínico.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [id])

  const doctorDe = (doctorId: string) => personal.find((u) => u.id === doctorId) ?? null

  const limpiarFormulario = () => {
    setDiagnostico('')
    setTratamiento('')
    setNotas('')
    setItemsReceta([{ ...ITEM_RECETA_VACIO }])
    setNotasGenerales('')
    setCuerpoDocumento('')
  }

  const handleRegistrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorRegistrar(null)

    let contenido: object

    if (esContenidoReceta(tipo)) {
      const items = itemsReceta
        .map((item) => ({ descripcion: item.descripcion.trim(), indicaciones: item.indicaciones.trim() }))
        .filter((item) => item.descripcion)
      if (items.length === 0) {
        setErrorRegistrar('Agrega al menos un ítem con descripción antes de guardar.')
        return
      }
      contenido = { items, notasGenerales: notasGenerales.trim() || null }
    } else if (esContenidoDocumento(tipo)) {
      if (!cuerpoDocumento.trim()) {
        setErrorRegistrar('Completa el contenido del documento antes de guardar.')
        return
      }
      contenido = { cuerpo: cuerpoDocumento.trim() }
    } else {
      if (!diagnostico.trim() && !tratamiento.trim() && !notas.trim()) {
        setErrorRegistrar('Completa al menos un campo antes de guardar.')
        return
      }
      contenido = {
        diagnostico: diagnostico.trim() || null,
        tratamiento: tratamiento.trim() || null,
        notas: notas.trim() || null,
      }
    }

    setRegistrando(true)
    try {
      const nueva = await api.post<EntradaHistorial>('/api/historial-clinico', {
        pacienteId: id,
        citaId: null,
        tipo,
        contenido,
      })
      setEntradas((actual) => [nueva, ...actual])
      limpiarFormulario()
    } catch (err) {
      setErrorRegistrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la entrada.')
    } finally {
      setRegistrando(false)
    }
  }

  const actualizarItemReceta = (indice: number, campo: keyof ItemReceta, valor: string) => {
    setItemsReceta((actual) => actual.map((item, i) => (i === indice ? { ...item, [campo]: valor } : item)))
  }

  const imprimir = (entrada: EntradaHistorial) => {
    setEntradaAImprimir(entrada)
    requestAnimationFrame(imprimirVentana)
  }

  const doctorImprimir = useMemo(
    () => (entradaAImprimir ? doctorDe(entradaAImprimir.doctorId) : null),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [entradaAImprimir, personal],
  )
  const contenidoRecetaImprimir =
    entradaAImprimir && esContenidoReceta(entradaAImprimir.tipo)
      ? parsearContenidoReceta(entradaAImprimir.contenido)
      : null
  const contenidoDocumentoImprimir =
    entradaAImprimir && esContenidoDocumento(entradaAImprimir.tipo)
      ? parsearContenidoDocumento(entradaAImprimir.contenido)
      : null

  return (
    <DashboardLayout titulo="Historial clínico">
      <button type="button" className="historial-volver no-imprimir" onClick={() => navigate('/pacientes')}>
        ← Volver
      </button>

      {cargando ? (
        <p className="text-secondary cargando-pulso no-imprimir">Cargando…</p>
      ) : error && !paciente ? (
        <p className="historial-error no-imprimir">{error}</p>
      ) : (
        <>
          {paciente && (
            <section className="historial-encabezado-card no-imprimir">
              <h2>
                {paciente.nombre} {paciente.apellido}
              </h2>
              <p className="text-muted">Cédula {paciente.cedula}</p>
            </section>
          )}

          {puedeRegistrar && (
            <section className="historial-crear-card no-imprimir">
              <h2>Agregar entrada</h2>
              <form className="historial-crear-form" onSubmit={(event) => void handleRegistrar(event)}>
                <label className="historial-tipo-selector">
                  Tipo de entrada
                  <select value={tipo} onChange={(event) => setTipo(event.target.value as TipoEntradaHistorial)}>
                    <option value="NotaClinica">{ETIQUETA_TIPO_ENTRADA.NotaClinica}</option>
                    <optgroup label="Receta electrónica">
                      {TIPOS_RECETA.map((opcion) => (
                        <option key={opcion} value={opcion}>
                          {ETIQUETA_TIPO_ENTRADA[opcion]}
                        </option>
                      ))}
                    </optgroup>
                    <optgroup label="Documentos">
                      {TIPOS_DOCUMENTO.map((opcion) => (
                        <option key={opcion} value={opcion}>
                          {ETIQUETA_TIPO_ENTRADA[opcion]}
                        </option>
                      ))}
                    </optgroup>
                  </select>
                </label>

                {tipo === 'NotaClinica' && (
                  <>
                    <input
                      placeholder="Diagnóstico"
                      value={diagnostico}
                      onChange={(event) => setDiagnostico(event.target.value)}
                    />
                    <input
                      placeholder="Tratamiento"
                      value={tratamiento}
                      onChange={(event) => setTratamiento(event.target.value)}
                    />
                    <textarea placeholder="Notas" value={notas} onChange={(event) => setNotas(event.target.value)} />
                  </>
                )}

                {esContenidoReceta(tipo) && (
                  <div className="historial-receta-items">
                    {itemsReceta.map((item, indice) => (
                      <div key={indice} className="historial-receta-fila">
                        <input
                          placeholder="Descripción (medicamento, examen, vacuna…)"
                          value={item.descripcion}
                          onChange={(event) => actualizarItemReceta(indice, 'descripcion', event.target.value)}
                        />
                        <input
                          placeholder="Indicaciones (dosis, frecuencia…)"
                          value={item.indicaciones}
                          onChange={(event) => actualizarItemReceta(indice, 'indicaciones', event.target.value)}
                        />
                        {itemsReceta.length > 1 && (
                          <button
                            type="button"
                            className="historial-receta-quitar"
                            onClick={() => setItemsReceta((actual) => actual.filter((_, i) => i !== indice))}
                            aria-label="Quitar ítem"
                          >
                            ×
                          </button>
                        )}
                      </div>
                    ))}
                    <button
                      type="button"
                      className="historial-receta-agregar"
                      onClick={() => setItemsReceta((actual) => [...actual, { ...ITEM_RECETA_VACIO }])}
                    >
                      + Agregar ítem
                    </button>
                    <textarea
                      placeholder="Notas generales (opcional)"
                      value={notasGenerales}
                      onChange={(event) => setNotasGenerales(event.target.value)}
                    />
                  </div>
                )}

                {esContenidoDocumento(tipo) && (
                  <textarea
                    placeholder={PLACEHOLDER_CUERPO_DOCUMENTO[tipo] ?? 'Contenido del documento…'}
                    value={cuerpoDocumento}
                    onChange={(event) => setCuerpoDocumento(event.target.value)}
                  />
                )}

                <button type="submit" disabled={registrando}>
                  {registrando ? 'Guardando…' : 'Guardar entrada'}
                </button>
              </form>
              {errorRegistrar && <p className="historial-error">{errorRegistrar}</p>}
            </section>
          )}

          <section className="historial-lista-card no-imprimir">
            {entradas.length === 0 ? (
              <p className="text-secondary">Todavía no hay entradas registradas.</p>
            ) : (
              <ul className="historial-lista">
                {entradas.map((entrada) => {
                  const contenidoNota = entrada.tipo === 'NotaClinica' ? parsearContenidoNotaClinica(entrada.contenido) : null
                  const contenidoReceta = esContenidoReceta(entrada.tipo) ? parsearContenidoReceta(entrada.contenido) : null
                  const contenidoDocumento = esContenidoDocumento(entrada.tipo)
                    ? parsearContenidoDocumento(entrada.contenido)
                    : null

                  return (
                    <li key={entrada.id} className="historial-entrada">
                      <div className="historial-entrada-encabezado">
                        <div>
                          <span className="historial-entrada-tipo">{ETIQUETA_TIPO_ENTRADA[entrada.tipo]}</span>{' '}
                          <span className="text-muted historial-entrada-fecha">
                            {formateadorFechaHora.format(new Date(entrada.registradoEn))}
                          </span>
                        </div>
                        {entrada.tipo !== 'NotaClinica' && (
                          <button type="button" className="historial-boton-imprimir" onClick={() => imprimir(entrada)}>
                            Imprimir
                          </button>
                        )}
                      </div>

                      {contenidoNota?.diagnostico && (
                        <p>
                          <strong>Diagnóstico:</strong> {contenidoNota.diagnostico}
                        </p>
                      )}
                      {contenidoNota?.tratamiento && (
                        <p>
                          <strong>Tratamiento:</strong> {contenidoNota.tratamiento}
                        </p>
                      )}
                      {contenidoNota?.notas && (
                        <p>
                          <strong>Notas:</strong> {contenidoNota.notas}
                        </p>
                      )}

                      {contenidoReceta &&
                        contenidoReceta.items.map((item, indice) => (
                          <p key={indice} className="historial-receta-item">
                            • {item.descripcion}
                            {item.indicaciones ? ` — ${item.indicaciones}` : ''}
                          </p>
                        ))}
                      {contenidoReceta?.notasGenerales && (
                        <p>
                          <strong>Notas:</strong> {contenidoReceta.notasGenerales}
                        </p>
                      )}

                      {contenidoDocumento?.cuerpo && <p>{contenidoDocumento.cuerpo}</p>}
                    </li>
                  )
                })}
              </ul>
            )}
          </section>
        </>
      )}

      {entradaAImprimir && paciente && (
        <div className="historial-comprobante">
          <header className="historial-comprobante-membrete">
            <span className="historial-comprobante-clinica">FUNDACIÓN BIENESTAR Y DESARROLLO</span>
            <span className="historial-comprobante-direccion">
              Calle Guaroa No. 4, Esq. Simón Orozco, Invivienda
              <br />
              Santo Domingo Este, Hainamosa, Distrito Nacional
            </span>
          </header>

          <h1 className="historial-comprobante-tipo">{ETIQUETA_TIPO_ENTRADA[entradaAImprimir.tipo]}</h1>
          <p className="historial-comprobante-fecha">{formateadorFecha.format(new Date(entradaAImprimir.registradoEn))}</p>

          <dl className="historial-comprobante-datos">
            <dt>Paciente</dt>
            <dd>
              {paciente.nombre} {paciente.apellido} — Cédula {paciente.cedula}
              {paciente.edad !== null ? `, ${paciente.edad} años` : ''}
            </dd>
            <dt>Médico</dt>
            <dd>
              {doctorImprimir?.nombreCompleto ?? 'No disponible'}
              {doctorImprimir?.especialidad ? ` — ${ETIQUETA_ESPECIALIDAD[doctorImprimir.especialidad]}` : ''}
            </dd>
          </dl>

          {contenidoRecetaImprimir && (
            <ol className="historial-comprobante-items">
              {contenidoRecetaImprimir.items.map((item, indice) => (
                <li key={indice}>
                  <strong>{item.descripcion}</strong>
                  {item.indicaciones ? ` — ${item.indicaciones}` : ''}
                </li>
              ))}
            </ol>
          )}
          {contenidoRecetaImprimir?.notasGenerales && (
            <p className="historial-comprobante-cuerpo">{contenidoRecetaImprimir.notasGenerales}</p>
          )}

          {contenidoDocumentoImprimir && <p className="historial-comprobante-cuerpo">{contenidoDocumentoImprimir.cuerpo}</p>}

          <div className="historial-comprobante-firma">
            <div className="historial-comprobante-firma-linea" />
            <span>{doctorImprimir?.nombreCompleto ?? 'Firma del médico'}</span>
          </div>
        </div>
      )}
    </DashboardLayout>
  )
}
