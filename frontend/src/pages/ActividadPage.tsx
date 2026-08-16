import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { Usuario } from '../types/usuario'
import './ActividadPage.css'

interface RegistroAuditoria {
  id: string
  usuarioId: string | null
  accion: string
  recurso: string
  codigoRespuestaHttp: number | null
  detalle: string
  registradoEn: string
}

interface DetalleLogin {
  NombreUsuario?: string
  Ip?: string
  Dispositivo?: string
}

function parsearDetalleLogin(detalle: string): DetalleLogin | null {
  try {
    return JSON.parse(detalle) as DetalleLogin
  } catch {
    return null
  }
}

const FILTRO_TODOS = 'Todos'
type Categoria = 'Login' | 'Cambios' | 'Inventario' | 'Pacientes'
const CATEGORIAS: Categoria[] = ['Login', 'Cambios', 'Inventario', 'Pacientes']

function categoriaDe(registro: RegistroAuditoria): Categoria {
  if (registro.accion.startsWith('auth.login.')) return 'Login'
  if (registro.recurso.includes('inventario')) return 'Inventario'
  if (registro.recurso.includes('pacientes')) return 'Pacientes'
  return 'Cambios'
}

function aFechaISO(fecha: Date): string {
  return fecha.toISOString().slice(0, 10)
}

function inicioDeHoy(): Date {
  const ahora = new Date()
  ahora.setHours(0, 0, 0, 0)
  return ahora
}

function sumarDias(fecha: Date, dias: number): Date {
  const copia = new Date(fecha)
  copia.setDate(copia.getDate() + dias)
  return copia
}

const PRESETS: { etiqueta: string; calcularInicio: (fin: Date) => Date }[] = [
  { etiqueta: 'Hoy', calcularInicio: (fin) => fin },
  { etiqueta: 'Últimos 7 días', calcularInicio: (fin) => sumarDias(fin, -7) },
  { etiqueta: 'Últimos 30 días', calcularInicio: (fin) => sumarDias(fin, -30) },
]

const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short' })
const formateadorHora = new Intl.DateTimeFormat('es-DO', { timeStyle: 'short' })

export function ActividadPage() {
  const hoy = inicioDeHoy()
  const [desde, setDesde] = useState(aFechaISO(sumarDias(hoy, -7)))
  const [hasta, setHasta] = useState(aFechaISO(hoy))
  const [registros, setRegistros] = useState<RegistroAuditoria[]>([])
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [filtroCategoria, setFiltroCategoria] = useState<Categoria | typeof FILTRO_TODOS>(FILTRO_TODOS)
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .get<Usuario[]>('/api/personal')
      .then(setPersonal)
      .catch(() => {
        // El directorio de personal es solo para mostrar nombres; si falla, se
        // muestra "Desconocido" en su lugar sin bloquear la bitácora.
      })
  }, [])

  const cargar = async (desdeValor: string, hastaValor: string) => {
    setCargando(true)
    setError(null)
    try {
      const desdeIso = encodeURIComponent(`${desdeValor}T00:00:00.000Z`)
      const hastaIso = encodeURIComponent(`${hastaValor}T23:59:59.999Z`)
      const datos = await api.get<RegistroAuditoria[]>(`/api/auditoria?desde=${desdeIso}&hasta=${hastaIso}`)
      setRegistros(datos)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la actividad.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    void cargar(desde, hasta)
    // Solo al montar: los filtros posteriores se disparan por el usuario, no por cambios de estado.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const aplicarPreset = (preset: (typeof PRESETS)[number]) => {
    const fin = inicioDeHoy()
    const inicio = preset.calcularInicio(fin)
    const desdeValor = aFechaISO(inicio)
    const hastaValor = aFechaISO(fin)
    setDesde(desdeValor)
    setHasta(hastaValor)
    void cargar(desdeValor, hastaValor)
  }

  const handleFiltrar = (event: FormEvent) => {
    event.preventDefault()
    void cargar(desde, hasta)
  }

  const nombrePorUsuarioId = useMemo(() => {
    const mapa = new Map<string, string>()
    for (const usuario of personal) {
      mapa.set(usuario.id, usuario.nombreCompleto)
    }
    return mapa
  }, [personal])

  const nombreDeUsuario = (usuarioId: string | null) =>
    (usuarioId && nombrePorUsuarioId.get(usuarioId)) || 'Desconocido'

  const registrosFiltrados = useMemo(
    () =>
      registros
        .filter((registro) => filtroCategoria === FILTRO_TODOS || categoriaDe(registro) === filtroCategoria)
        .sort((a, b) => b.registradoEn.localeCompare(a.registradoEn)),
    [registros, filtroCategoria],
  )

  const esVistaLogin = filtroCategoria === 'Login'

  return (
    <DashboardLayout titulo="Actividad">
      <section className="actividad-filtros">
        <div className="actividad-presets">
          {PRESETS.map((preset) => (
            <button key={preset.etiqueta} type="button" onClick={() => aplicarPreset(preset)}>
              {preset.etiqueta}
            </button>
          ))}
        </div>
        <form className="actividad-rango" onSubmit={handleFiltrar}>
          <label>
            Desde
            <input type="date" value={desde} max={hasta} onChange={(event) => setDesde(event.target.value)} />
          </label>
          <label>
            Hasta
            <input type="date" value={hasta} min={desde} onChange={(event) => setHasta(event.target.value)} />
          </label>
          <button type="submit" disabled={cargando}>
            {cargando ? 'Filtrando…' : 'Filtrar'}
          </button>
        </form>
      </section>

      <div className="actividad-tabs" role="tablist">
        <button
          type="button"
          role="tab"
          aria-selected={filtroCategoria === FILTRO_TODOS}
          className={`actividad-tab${filtroCategoria === FILTRO_TODOS ? ' activo' : ''}`}
          onClick={() => setFiltroCategoria(FILTRO_TODOS)}
        >
          Todos
        </button>
        {CATEGORIAS.map((categoria) => (
          <button
            key={categoria}
            type="button"
            role="tab"
            aria-selected={filtroCategoria === categoria}
            className={`actividad-tab${filtroCategoria === categoria ? ' activo' : ''}`}
            onClick={() => setFiltroCategoria(categoria)}
          >
            {categoria}
          </button>
        ))}
      </div>

      <section className="actividad-tabla-card">
        {error && <p className="actividad-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : registrosFiltrados.length === 0 ? (
          <p className="text-secondary">Sin actividad registrada en este rango.</p>
        ) : (
          <div className="actividad-tabla-scroll">
            <table className="actividad-tabla">
              <thead>
                {esVistaLogin ? (
                  <tr>
                    <th>Usuario</th>
                    <th>Fecha</th>
                    <th>Hora</th>
                    <th>IP</th>
                    <th>Dispositivo</th>
                    <th>Resultado</th>
                  </tr>
                ) : (
                  <tr>
                    <th>Usuario</th>
                    <th>Fecha</th>
                    <th>Hora</th>
                    <th>Acción</th>
                    <th>Recurso</th>
                  </tr>
                )}
              </thead>
              <tbody>
                {registrosFiltrados.map((registro) => {
                  const fecha = new Date(registro.registradoEn)
                  if (esVistaLogin) {
                    const detalle = parsearDetalleLogin(registro.detalle)
                    const exitoso = registro.accion === 'auth.login.exitoso'
                    return (
                      <tr key={registro.id}>
                        <td>{nombreDeUsuario(registro.usuarioId)}</td>
                        <td className="text-muted">{formateadorFecha.format(fecha)}</td>
                        <td className="text-muted">{formateadorHora.format(fecha)}</td>
                        <td className="text-muted">{detalle?.Ip ?? '—'}</td>
                        <td className="text-muted">{detalle?.Dispositivo ?? '—'}</td>
                        <td>
                          <span
                            className={`actividad-badge ${exitoso ? 'actividad-badge-exito' : 'actividad-badge-fallido'}`}
                          >
                            {exitoso ? 'Éxito' : 'Fallido'}
                          </span>
                        </td>
                      </tr>
                    )
                  }
                  return (
                    <tr key={registro.id}>
                      <td>{nombreDeUsuario(registro.usuarioId)}</td>
                      <td className="text-muted">{formateadorFecha.format(fecha)}</td>
                      <td className="text-muted">{formateadorHora.format(fecha)}</td>
                      <td className="text-muted">{registro.accion}</td>
                      <td className="text-muted">{registro.recurso}</td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </DashboardLayout>
  )
}
