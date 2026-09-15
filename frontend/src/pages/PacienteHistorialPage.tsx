import { ArrowLeft } from 'lucide-react'
import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { Modal } from '../components/ui/Modal'
import { Boton } from '../components/ui/Boton'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import { imprimirVentana } from '../lib/imprimir'
import type { Paciente } from '../types/paciente'
import type { DoctorSimple } from '../types/doctor'
import { ETIQUETA_ESPECIALIDAD } from '../types/personal'
import {
  CAMPOS_DOCUMENTO_POR_TIPO,
  CAMPOS_ITEM_POR_TIPO,
  ETIQUETA_TIPO_ENTRADA,
  TIPOS_DOCUMENTO,
  TIPOS_RECETA,
  campoPrincipalReceta,
  esContenidoDocumento,
  esContenidoReceta,
  parsearContenidoDocumento,
  parsearContenidoNotaClinica,
  parsearContenidoReceta,
  type CampoEstructurado,
  type EntradaHistorial,
  type ItemReceta,
  type TipoEntradaHistorial,
} from '../types/historialClinico'
import './PacienteHistorialPage.css'

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium', timeStyle: 'short' })
const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'long' })

function itemVacio(tipo: TipoEntradaHistorial): ItemReceta {
  const campos = CAMPOS_ITEM_POR_TIPO[tipo] ?? []
  return Object.fromEntries(campos.map((c) => [c.clave, '']))
}

function campoInput(
  campo: CampoEstructurado,
  valor: string,
  onChange: (valor: string) => void,
  idPrefijo: string,
) {
  const id = `${idPrefijo}-${campo.clave}`
  if (campo.tipo === 'area') {
    return <textarea key={campo.clave} id={id} placeholder={campo.etiqueta} value={valor} onChange={(e) => onChange(e.target.value)} />
  }
  if (campo.tipo === 'select') {
    return (
      <select key={campo.clave} id={id} value={valor} onChange={(e) => onChange(e.target.value)}>
        <option value="">{campo.etiqueta}…</option>
        {(campo.opciones ?? []).map((opcion) => (
          <option key={opcion} value={opcion}>
            {opcion}
          </option>
        ))}
      </select>
    )
  }
  return (
    <input
      key={campo.clave}
      id={id}
      type={campo.tipo === 'fecha' ? 'date' : campo.tipo === 'numero' ? 'number' : 'text'}
      placeholder={campo.placeholder ?? campo.etiqueta}
      value={valor}
      onChange={(e) => onChange(e.target.value)}
    />
  )
}

export function PacienteHistorialPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { perfil } = useAuth()
  // Admin puede supervisar el historial pero no escribirlo: eso sigue siendo exclusivo
  // del Doctor (ver HistorialClinicoController, POST solo permite RolUsuario.Doctor).
  const puedeRegistrar = perfil?.rol === 'Doctor'

  const [paciente, setPaciente] = useState<Paciente | null>(null)
  const [entradas, setEntradas] = useState<EntradaHistorial[]>([])
  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [tipo, setTipo] = useState<TipoEntradaHistorial>('NotaClinica')
  const [diagnostico, setDiagnostico] = useState('')
  const [tratamiento, setTratamiento] = useState('')
  const [notas, setNotas] = useState('')
  const [itemsReceta, setItemsReceta] = useState<ItemReceta[]>([itemVacio('NotaClinica')])
  const [notasGenerales, setNotasGenerales] = useState('')
  const [camposDocumento, setCamposDocumento] = useState<Record<string, string>>({})
  const [registrando, setRegistrando] = useState(false)
  const [errorRegistrar, setErrorRegistrar] = useState<string | null>(null)

  const [entradaAImprimir, setEntradaAImprimir] = useState<EntradaHistorial | null>(null)
  const [mostrarConfirmacionSalir, setMostrarConfirmacionSalir] = useState(false)

  // El formulario de registro es el único "largo" de esta página (notas clínicas,
  // recetas o documentos con varios campos) — perder eso por un click accidental en
  // "Volver" es justo el escenario que esta confirmación evita.
  const hayCambiosSinGuardar =
    diagnostico.trim() !== '' ||
    tratamiento.trim() !== '' ||
    notas.trim() !== '' ||
    notasGenerales.trim() !== '' ||
    itemsReceta.some((item) => Object.values(item).some((valor) => valor.trim() !== '')) ||
    Object.values(camposDocumento).some((valor) => valor.trim() !== '')

  useEffect(() => {
    if (!hayCambiosSinGuardar) return

    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault()
    }
    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [hayCambiosSinGuardar])

  const handleClickVolver = () => {
    if (hayCambiosSinGuardar) {
      setMostrarConfirmacionSalir(true)
      return
    }
    navigate('/pacientes')
  }

  useEffect(() => {
    let cancelado = false

    // /api/personal/doctores (no /api/personal): el Doctor no tiene permiso para ver el
    // listado completo de personal, solo el de doctores — necesario para resolver su
    // propio nombre/especialidad/exequátur al imprimir (ver PersonalController).
    Promise.all([
      api.get<Paciente>(`/api/pacientes/${id}`),
      api.get<EntradaHistorial[]>(`/api/historial-clinico/paciente/${id}`),
      api.get<DoctorSimple[]>('/api/personal/doctores'),
    ])
      .then(([pacienteEncontrado, datosEntradas, datosDoctores]) => {
        if (cancelado) return
        setPaciente(pacienteEncontrado)
        setEntradas(datosEntradas)
        setDoctores(datosDoctores)
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

  // doctorId de una entrada es el SupabaseUserId (currentUser.UsuarioId al registrarla),
  // igual que DoctorSimple.id — ver ListarDoctoresUseCase.
  const doctorDe = (doctorId: string) => doctores.find((d) => d.id === doctorId) ?? null

  const cambiarTipo = (nuevoTipo: TipoEntradaHistorial) => {
    setTipo(nuevoTipo)
    setItemsReceta([itemVacio(nuevoTipo)])
    setCamposDocumento({})
  }

  const limpiarFormulario = () => {
    setDiagnostico('')
    setTratamiento('')
    setNotas('')
    setItemsReceta([itemVacio(tipo)])
    setNotasGenerales('')
    setCamposDocumento({})
  }

  const handleRegistrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorRegistrar(null)

    let contenido: object

    if (esContenidoReceta(tipo)) {
      const clavePrincipal = campoPrincipalReceta(tipo)
      const items = itemsReceta
        .map((item) => Object.fromEntries(Object.entries(item).map(([k, v]) => [k, v.trim()])))
        .filter((item) => item[clavePrincipal])
      if (items.length === 0) {
        setErrorRegistrar('Completa al menos un ítem antes de guardar.')
        return
      }
      contenido = { items, notasGenerales: notasGenerales.trim() || null }
    } else if (esContenidoDocumento(tipo)) {
      const campos = Object.fromEntries(
        Object.entries(camposDocumento)
          .map(([k, v]) => [k, v.trim()])
          .filter(([, v]) => v),
      )
      if (Object.keys(campos).length === 0) {
        setErrorRegistrar('Completa al menos un campo antes de guardar.')
        return
      }
      contenido = { campos }
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

  const actualizarItemReceta = (indice: number, clave: string, valor: string) => {
    setItemsReceta((actual) => actual.map((item, i) => (i === indice ? { ...item, [clave]: valor } : item)))
  }

  const imprimir = (entrada: EntradaHistorial) => {
    setEntradaAImprimir(entrada)
    requestAnimationFrame(imprimirVentana)
  }

  const doctorImprimir = useMemo(
    () => (entradaAImprimir ? doctorDe(entradaAImprimir.doctorId) : null),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [entradaAImprimir, doctores],
  )
  const contenidoRecetaImprimir =
    entradaAImprimir && esContenidoReceta(entradaAImprimir.tipo) ? parsearContenidoReceta(entradaAImprimir.contenido) : null
  const contenidoDocumentoImprimir =
    entradaAImprimir && esContenidoDocumento(entradaAImprimir.tipo)
      ? parsearContenidoDocumento(entradaAImprimir.contenido)
      : null
  const camposDocumentoImprimir = entradaAImprimir ? CAMPOS_DOCUMENTO_POR_TIPO[entradaAImprimir.tipo] ?? [] : []
  const camposItemImprimir = entradaAImprimir ? CAMPOS_ITEM_POR_TIPO[entradaAImprimir.tipo] ?? [] : []

  const camposItemActual = CAMPOS_ITEM_POR_TIPO[tipo] ?? []
  const camposDocumentoActual = CAMPOS_DOCUMENTO_POR_TIPO[tipo] ?? []

  return (
    <DashboardLayout titulo="Historial clínico">
      <button type="button" className="historial-volver no-imprimir" onClick={handleClickVolver}>
        <ArrowLeft size={15} aria-hidden="true" /> Volver
      </button>

      <Modal
        abierto={mostrarConfirmacionSalir}
        onCerrar={() => setMostrarConfirmacionSalir(false)}
        titulo="¿Salir sin guardar?"
        subtitulo="Hay una entrada del historial sin guardar. Si salís ahora, se pierde."
        acciones={
          <>
            <Boton variante="secundario" onClick={() => setMostrarConfirmacionSalir(false)}>
              Seguir editando
            </Boton>
            <Boton variante="destructivo" onClick={() => navigate('/pacientes')}>
              Salir sin guardar
            </Boton>
          </>
        }
      >
        <></>
      </Modal>

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
                  <select value={tipo} onChange={(event) => cambiarTipo(event.target.value as TipoEntradaHistorial)}>
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
                        <div className="historial-receta-campos">
                          {camposItemActual.map((campo) =>
                            campoInput(campo, item[campo.clave] ?? '', (valor) => actualizarItemReceta(indice, campo.clave, valor), `item-${indice}`),
                          )}
                        </div>
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
                      onClick={() => setItemsReceta((actual) => [...actual, itemVacio(tipo)])}
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
                  <div className="historial-documento-campos">
                    {camposDocumentoActual.map((campo) =>
                      campoInput(
                        campo,
                        camposDocumento[campo.clave] ?? '',
                        (valor) => setCamposDocumento((actual) => ({ ...actual, [campo.clave]: valor })),
                        'doc',
                      ),
                    )}
                  </div>
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
                  const camposItem = CAMPOS_ITEM_POR_TIPO[entrada.tipo] ?? []
                  const camposDoc = CAMPOS_DOCUMENTO_POR_TIPO[entrada.tipo] ?? []

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
                        contenidoReceta.items.map((item, indice) => {
                          const clavePrincipal = campoPrincipalReceta(entrada.tipo)
                          const resto = camposItem.filter((c) => c.clave !== clavePrincipal && item[c.clave])
                          return (
                            <p key={indice} className="historial-receta-item">
                              • {item[clavePrincipal]}
                              {resto.length > 0 && ` — ${resto.map((c) => `${c.etiqueta.toLowerCase()}: ${item[c.clave]}`).join(', ')}`}
                            </p>
                          )
                        })}
                      {contenidoReceta?.notasGenerales && (
                        <p>
                          <strong>Notas:</strong> {contenidoReceta.notasGenerales}
                        </p>
                      )}

                      {contenidoDocumento &&
                        camposDoc
                          .filter((c) => contenidoDocumento.campos[c.clave])
                          .map((c) => (
                            <p key={c.clave}>
                              <strong>{c.etiqueta}:</strong> {contenidoDocumento.campos[c.clave]}
                            </p>
                          ))}
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
              {doctorImprimir?.exequatur ? ` — Exequátur ${doctorImprimir.exequatur}` : ''}
            </dd>
          </dl>

          {contenidoRecetaImprimir && (
            <ol className="historial-comprobante-items">
              {contenidoRecetaImprimir.items.map((item, indice) => {
                const clavePrincipal = campoPrincipalReceta(entradaAImprimir.tipo)
                const resto = camposItemImprimir.filter((c) => c.clave !== clavePrincipal && item[c.clave])
                return (
                  <li key={indice}>
                    <strong>{item[clavePrincipal]}</strong>
                    {resto.length > 0 && ` — ${resto.map((c) => `${c.etiqueta.toLowerCase()}: ${item[c.clave]}`).join(', ')}`}
                  </li>
                )
              })}
            </ol>
          )}
          {contenidoRecetaImprimir?.notasGenerales && (
            <p className="historial-comprobante-cuerpo">{contenidoRecetaImprimir.notasGenerales}</p>
          )}

          {contenidoDocumentoImprimir && (
            <dl className="historial-comprobante-datos">
              {camposDocumentoImprimir
                .filter((c) => contenidoDocumentoImprimir.campos[c.clave])
                .map((c) => (
                  <div key={c.clave} className="historial-comprobante-campo-doc">
                    <dt>{c.etiqueta}</dt>
                    <dd>{contenidoDocumentoImprimir.campos[c.clave]}</dd>
                  </div>
                ))}
            </dl>
          )}
          {contenidoDocumentoImprimir?.cuerpo && <p className="historial-comprobante-cuerpo">{contenidoDocumentoImprimir.cuerpo}</p>}

          <div className="historial-comprobante-firma">
            <div className="historial-comprobante-firma-linea" />
            <span>{doctorImprimir?.nombreCompleto ?? 'Firma del médico'}</span>
          </div>
        </div>
      )}
    </DashboardLayout>
  )
}
