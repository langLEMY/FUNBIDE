import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { colorPorRol } from '../lib/colorPorRol'
import { exportarCsv } from '../lib/exportarCsv'
import { iniciales } from '../lib/iniciales'
import type { Usuario } from '../types/usuario'
import './MiPerfilPage.css'

interface RegistroAuditoria {
  id: string
  usuarioId: string | null
  accion: string
  recurso: string
  codigoRespuestaHttp: number | null
  detalle: string
  registradoEn: string
}

interface EstadoSistema {
  baseDeDatosOperativa: boolean
}

export function MiPerfilPage() {
  const { perfil, iniciarSesion, restablecerContrasena, recargarPerfil } = useAuth()

  const inputFotoRef = useRef<HTMLInputElement>(null)
  const [subiendoFoto, setSubiendoFoto] = useState(false)
  const [errorFoto, setErrorFoto] = useState<string | null>(null)

  const [contrasenaActual, setContrasenaActual] = useState('')
  const [nuevaContrasena, setNuevaContrasena] = useState('')
  const [confirmarContrasena, setConfirmarContrasena] = useState('')
  const [actualizandoContrasena, setActualizandoContrasena] = useState(false)
  const [errorContrasena, setErrorContrasena] = useState<string | null>(null)
  const [exitoContrasena, setExitoContrasena] = useState(false)

  const esLemy = perfil?.rol === 'Lemy'

  const [estadoSistema, setEstadoSistema] = useState<EstadoSistema | null>(null)
  const [verificandoEstado, setVerificandoEstado] = useState(false)
  const [errorEstado, setErrorEstado] = useState<string | null>(null)

  const [exportando, setExportando] = useState(false)
  const [errorExportar, setErrorExportar] = useState<string | null>(null)

  const verificarEstadoSistema = async () => {
    setVerificandoEstado(true)
    setErrorEstado(null)
    try {
      const estado = await api.get<EstadoSistema>('/api/sistema/estado')
      setEstadoSistema(estado)
    } catch (err) {
      setErrorEstado(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo verificar el estado.')
    } finally {
      setVerificandoEstado(false)
    }
  }

  useEffect(() => {
    if (esLemy) {
      void verificarEstadoSistema()
    }
    // Solo al montar (y cuando se confirma que el perfil es Lemy).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [esLemy])

  if (!perfil) {
    return null
  }

  const handleEditarPerfil = () => {
    setErrorFoto(null)
    inputFotoRef.current?.click()
  }

  const handleArchivoSeleccionado = async (event: ChangeEvent<HTMLInputElement>) => {
    const archivo = event.target.files?.[0]
    event.target.value = ''
    if (!archivo) {
      return
    }

    setErrorFoto(null)
    setSubiendoFoto(true)
    try {
      await api.postForm<Usuario>('/api/mi-perfil/foto', (() => {
        const form = new FormData()
        form.append('archivo', archivo)
        return form
      })())
      await recargarPerfil()
    } catch (err) {
      setErrorFoto(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo actualizar la foto.')
    } finally {
      setSubiendoFoto(false)
    }
  }

  const handleCambiarContrasena = async (event: FormEvent) => {
    event.preventDefault()
    setErrorContrasena(null)
    setExitoContrasena(false)

    if (nuevaContrasena.length < 8) {
      setErrorContrasena('La nueva contraseña debe tener al menos 8 caracteres.')
      return
    }
    if (nuevaContrasena !== confirmarContrasena) {
      setErrorContrasena('Las contraseñas nuevas no coinciden.')
      return
    }

    setActualizandoContrasena(true)
    try {
      await iniciarSesion(perfil.correo, contrasenaActual)
      await restablecerContrasena(nuevaContrasena)
      setExitoContrasena(true)
      setContrasenaActual('')
      setNuevaContrasena('')
      setConfirmarContrasena('')
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : undefined
      setErrorContrasena(traducirErrorAuth(mensaje, 'No se pudo actualizar la contraseña.'))
    } finally {
      setActualizandoContrasena(false)
    }
  }

  const handleExportarActividad = async () => {
    setExportando(true)
    setErrorExportar(null)
    try {
      const hasta = new Date()
      const desde = new Date(hasta)
      desde.setDate(desde.getDate() - 30)

      const registros = await api.get<RegistroAuditoria[]>(
        `/api/auditoria?desde=${encodeURIComponent(desde.toISOString())}&hasta=${encodeURIComponent(hasta.toISOString())}`,
      )

      exportarCsv(
        `actividad_${hasta.toISOString().slice(0, 10)}.csv`,
        registros.map((registro) => ({
          fecha: registro.registradoEn,
          accion: registro.accion,
          recurso: registro.recurso,
          codigoRespuestaHttp: registro.codigoRespuestaHttp ?? '',
          detalle: registro.detalle,
        })),
      )
    } catch (err) {
      setErrorExportar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo exportar la actividad.')
    } finally {
      setExportando(false)
    }
  }

  return (
    <DashboardLayout titulo="Mi perfil">
      <div className="mi-perfil-grilla">
        <div className="mi-perfil-columna">
          <section className="mi-perfil-card">
            {perfil.fotoPerfilUrl ? (
              <img src={perfil.fotoPerfilUrl} alt="" className="mi-perfil-avatar" />
            ) : (
              <span className="mi-perfil-avatar mi-perfil-avatar-iniciales" style={{ background: colorPorRol(perfil.rol) }}>
                {iniciales(perfil.nombreCompleto)}
              </span>
            )}

            <div className="mi-perfil-identidad">
              <p className="mi-perfil-nombre">{perfil.nombreCompleto}</p>
              <p className="mi-perfil-correo text-muted">{perfil.correo}</p>
              <span className="mi-perfil-badge-rol">{perfil.rol.toUpperCase()}</span>
            </div>

            <button type="button" className="mi-perfil-boton-editar" onClick={handleEditarPerfil} disabled={subiendoFoto}>
              {subiendoFoto ? 'Subiendo…' : 'Editar perfil'}
            </button>
            <input ref={inputFotoRef} type="file" accept="image/*" hidden onChange={handleArchivoSeleccionado} />
            {errorFoto && <p className="mi-perfil-error">{errorFoto}</p>}
          </section>

          <section className="mi-perfil-card">
            <h2>Cambiar contraseña</h2>
            <p className="text-secondary mi-perfil-card-subtitulo">Úsala si sospechas que tu cuenta fue comprometida.</p>

            <form className="mi-perfil-form-contrasena" onSubmit={(event) => void handleCambiarContrasena(event)}>
              <label className="mi-perfil-label">
                Contraseña actual
                <input
                  type="password"
                  value={contrasenaActual}
                  onChange={(event) => setContrasenaActual(event.target.value)}
                  required
                />
              </label>
              <label className="mi-perfil-label">
                Nueva contraseña
                <input
                  type="password"
                  placeholder="Mínimo 8 caracteres"
                  value={nuevaContrasena}
                  onChange={(event) => setNuevaContrasena(event.target.value)}
                  minLength={8}
                  required
                />
              </label>
              <label className="mi-perfil-label">
                Confirmar nueva contraseña
                <input
                  type="password"
                  value={confirmarContrasena}
                  onChange={(event) => setConfirmarContrasena(event.target.value)}
                  minLength={8}
                  required
                />
              </label>

              {errorContrasena && <p className="mi-perfil-error">{errorContrasena}</p>}
              {exitoContrasena && <p className="mi-perfil-exito">Contraseña actualizada correctamente.</p>}

              <button type="submit" className="mi-perfil-boton-primario" disabled={actualizandoContrasena}>
                {actualizandoContrasena ? 'Actualizando…' : 'Actualizar contraseña'}
              </button>
            </form>
          </section>
        </div>

        {esLemy && (
          <div className="mi-perfil-columna">
            <section className="mi-perfil-card">
              <h2>Estado del sistema</h2>
              <dl className="mi-perfil-estado-lista">
                <div className="mi-perfil-estado-fila">
                  <dt>Base de datos</dt>
                  <dd>
                    {verificandoEstado ? (
                      <span className="text-muted">Verificando…</span>
                    ) : errorEstado ? (
                      <span className="mi-perfil-estado-punto mi-perfil-estado-mal">Sin verificar</span>
                    ) : (
                      <span
                        className={`mi-perfil-estado-punto ${estadoSistema?.baseDeDatosOperativa ? 'mi-perfil-estado-bien' : 'mi-perfil-estado-mal'}`}
                      >
                        {estadoSistema?.baseDeDatosOperativa ? 'Operativo' : 'Con problemas'}
                      </span>
                    )}
                  </dd>
                </div>
                <div className="mi-perfil-estado-fila">
                  <dt>Respaldos automáticos</dt>
                  <dd>
                    <span className="mi-perfil-estado-punto mi-perfil-estado-desconocido">No disponible todavía</span>
                  </dd>
                </div>
              </dl>
            </section>

            <section className="mi-perfil-card">
              <h2>Herramientas de soporte</h2>
              <p className="text-secondary mi-perfil-card-subtitulo">Acciones rápidas para resolver incidencias comunes.</p>

              <ul className="mi-perfil-herramientas">
                <li>
                  <div>
                    <p className="mi-perfil-herramienta-titulo">Vaciar caché del sistema</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">
                      No aplica: la API no mantiene caché de servidor.
                    </p>
                  </div>
                  <button type="button" disabled>
                    Próximamente
                  </button>
                </li>
                <li>
                  <div>
                    <p className="mi-perfil-herramienta-titulo">Verificar conexión con la base de datos</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">Comprueba que los datos se estén guardando correctamente.</p>
                  </div>
                  <button type="button" onClick={() => void verificarEstadoSistema()} disabled={verificandoEstado}>
                    {verificandoEstado ? 'Verificando…' : 'Ejecutar'}
                  </button>
                </li>
                <li>
                  <div>
                    <p className="mi-perfil-herramienta-titulo">Exportar registro de actividad</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">Descarga los últimos 30 días de bitácora en CSV.</p>
                  </div>
                  <button type="button" onClick={() => void handleExportarActividad()} disabled={exportando}>
                    {exportando ? 'Exportando…' : 'Ejecutar'}
                  </button>
                </li>
                <li>
                  <div>
                    <p className="mi-perfil-herramienta-titulo">Forzar sincronización de datos</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">
                      No aplica: no hay un sistema externo con el que sincronizar.
                    </p>
                  </div>
                  <button type="button" disabled>
                    Próximamente
                  </button>
                </li>
              </ul>
              {errorExportar && <p className="mi-perfil-error">{errorExportar}</p>}
            </section>

            <section className="mi-perfil-card mi-perfil-card-riesgo">
              <h2>Zona de riesgo</h2>
              <p className="text-secondary mi-perfil-card-subtitulo">
                Cierra la sesión en todos los dispositivos conectados con esta cuenta.
              </p>
              <button type="button" className="mi-perfil-boton-peligro" disabled title="Próximamente">
                Cerrar todas las sesiones (próximamente)
              </button>
            </section>
          </div>
        )}
      </div>
    </DashboardLayout>
  )
}
