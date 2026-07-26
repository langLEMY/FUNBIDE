import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { colorPorRol } from '../lib/colorPorRol'
import { exportarCsv } from '../lib/exportarCsv'
import { iniciales } from '../lib/iniciales'
import { esModoLocal } from '../lib/supabaseClient'
import type { Usuario } from '../types/usuario'
import './MiPerfilPage.css'

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

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
  ultimoBackupUtc: string | null
  ultimoBackupExitoso: boolean | null
  almacenamientoOperativo: boolean
  modoMantenimientoActivo: boolean
  modoMantenimientoMensaje: string | null
}

export function MiPerfilPage() {
  const { perfil, iniciarSesion, restablecerContrasena, recargarPerfil, cerrarSesion } = useAuth()
  const navigate = useNavigate()

  const inputFotoRef = useRef<HTMLInputElement>(null)
  const [subiendoFoto, setSubiendoFoto] = useState(false)
  const [errorFoto, setErrorFoto] = useState<string | null>(null)

  const [nombreCompleto, setNombreCompleto] = useState(perfil?.nombreCompleto ?? '')
  const [correoEditar, setCorreoEditar] = useState(perfil?.correo ?? '')
  const [contrasenaActualCorreo, setContrasenaActualCorreo] = useState('')
  const [guardandoPerfil, setGuardandoPerfil] = useState(false)
  const [errorPerfil, setErrorPerfil] = useState<string | null>(null)
  const [exitoPerfil, setExitoPerfil] = useState(false)

  const [cerrandoSesiones, setCerrandoSesiones] = useState(false)
  const [errorCerrarSesiones, setErrorCerrarSesiones] = useState<string | null>(null)

  useEffect(() => {
    if (perfil) {
      setNombreCompleto(perfil.nombreCompleto)
      setCorreoEditar(perfil.correo)
    }
  }, [perfil?.nombreCompleto, perfil?.correo])

  const [contrasenaActual, setContrasenaActual] = useState('')
  const [nuevaContrasena, setNuevaContrasena] = useState('')
  const [confirmarContrasena, setConfirmarContrasena] = useState('')
  const [actualizandoContrasena, setActualizandoContrasena] = useState(false)
  const [errorContrasena, setErrorContrasena] = useState<string | null>(null)
  const [exitoContrasena, setExitoContrasena] = useState(false)

  const esLemy = perfil?.rol === 'Lemy'
  const esAdmin = perfil?.rol === 'Admin'

  const [estadoSistema, setEstadoSistema] = useState<EstadoSistema | null>(null)
  const [verificandoEstado, setVerificandoEstado] = useState(false)
  const [errorEstado, setErrorEstado] = useState<string | null>(null)

  const [exportando, setExportando] = useState(false)
  const [errorExportar, setErrorExportar] = useState<string | null>(null)

  const [cambiandoMantenimiento, setCambiandoMantenimiento] = useState(false)
  const [errorMantenimiento, setErrorMantenimiento] = useState<string | null>(null)

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
    if (esLemy || esAdmin) {
      void verificarEstadoSistema()
    }
    // Solo al montar (y cuando se confirma que el perfil es Lemy o Admin).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [esLemy, esAdmin])

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

  const cambiandoCorreo = perfil !== null && correoEditar.trim().toLowerCase() !== perfil.correo.toLowerCase()

  const handleGuardarPerfil = async (event: FormEvent) => {
    event.preventDefault()
    setErrorPerfil(null)
    setExitoPerfil(false)

    if (!nombreCompleto.trim()) {
      setErrorPerfil('El nombre completo es obligatorio.')
      return
    }
    if (!correoEditar.trim()) {
      setErrorPerfil('El correo es obligatorio.')
      return
    }
    if (cambiandoCorreo && !contrasenaActualCorreo) {
      setErrorPerfil('Ingresa tu contraseña actual para cambiar el correo de inicio de sesión.')
      return
    }

    setGuardandoPerfil(true)
    try {
      // Cambiar el correo de login exige re-verificar la contraseña actual (igual que
      // cambiar la contraseña, más abajo): sin esto, un JWT robado alcanzaba para
      // secuestrar la cuenta permanentemente (cambiar el correo y después usar
      // "olvidé mi contraseña" sobre el correo nuevo).
      if (cambiandoCorreo && perfil) {
        await iniciarSesion(perfil.correo, contrasenaActualCorreo)
      }

      await api.patch<Usuario>('/api/mi-perfil', {
        nombreCompleto: nombreCompleto.trim(),
        correo: correoEditar.trim(),
      })
      await recargarPerfil()
      setExitoPerfil(true)
      setContrasenaActualCorreo('')
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : undefined
      setErrorPerfil(
        err instanceof ApiError
          ? (err.detalle ?? err.message)
          : traducirErrorAuth(mensaje, 'No se pudo actualizar el perfil.'),
      )
    } finally {
      setGuardandoPerfil(false)
    }
  }

  const handleCerrarTodasLasSesiones = async () => {
    // En modo Local el JWT no tiene revocación server-side (ver localAuthClient.signOut):
    // esto solo borra la sesión de ESTE navegador, no las de otros dispositivos ya
    // logueados. Avisamos antes de que alguien confíe en el botón como si fuera un
    // "kill switch" real tras perder/prestar un equipo.
    const confirmado = window.confirm(
      esModoLocal
        ? 'En esta instalación (modo local) esto solo cierra la sesión de este navegador — no puede revocar sesiones ya abiertas en otros dispositivos. ¿Continuar?'
        : 'Esto cierra la sesión en todos los dispositivos conectados con esta cuenta, incluida esta. ¿Continuar?',
    )
    if (!confirmado) {
      return
    }

    setErrorCerrarSesiones(null)
    setCerrandoSesiones(true)
    try {
      await cerrarSesion({ scope: 'global' })
      navigate('/login', { replace: true })
    } catch (err) {
      setErrorCerrarSesiones(err instanceof Error ? err.message : 'No se pudo cerrar la sesión.')
      setCerrandoSesiones(false)
    }
  }

  const handleToggleMantenimiento = async () => {
    const activandolo = !estadoSistema?.modoMantenimientoActivo

    if (activandolo) {
      const confirmado = window.confirm(
        'Esto bloquea el acceso a TODO el personal (Admin, Doctor, Fondos) hasta que lo apagues — ' +
          'solo tu cuenta Lemy sigue pudiendo entrar. ¿Continuar?',
      )
      if (!confirmado) {
        return
      }
    } else {
      const confirmado = window.confirm('¿Quitar el modo mantenimiento y devolver el acceso a todo el personal?')
      if (!confirmado) {
        return
      }
    }

    const mensaje = activandolo ? window.prompt('Mensaje para quien intente entrar (opcional):', '') : null

    setErrorMantenimiento(null)
    setCambiandoMantenimiento(true)
    try {
      await api.patch('/api/mantenimiento', { activo: activandolo, mensaje: mensaje || null })
      await verificarEstadoSistema()
    } catch (err) {
      setErrorMantenimiento(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cambiar el modo mantenimiento.')
    } finally {
      setCambiandoMantenimiento(false)
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
            <h2>Editar nombre y correo</h2>
            <p className="text-secondary mi-perfil-card-subtitulo">Se sincroniza con tu correo de inicio de sesión.</p>

            <form className="mi-perfil-form-contrasena" onSubmit={(event) => void handleGuardarPerfil(event)}>
              <label className="mi-perfil-label">
                Nombre completo
                <input
                  value={nombreCompleto}
                  onChange={(event) => setNombreCompleto(event.target.value)}
                  required
                />
              </label>
              <label className="mi-perfil-label">
                Correo
                <input
                  type="email"
                  value={correoEditar}
                  onChange={(event) => setCorreoEditar(event.target.value)}
                  required
                />
              </label>
              {cambiandoCorreo && (
                <label className="mi-perfil-label">
                  Contraseña actual (para confirmar el cambio de correo)
                  <input
                    type="password"
                    value={contrasenaActualCorreo}
                    onChange={(event) => setContrasenaActualCorreo(event.target.value)}
                    required
                  />
                </label>
              )}

              {errorPerfil && <p className="mi-perfil-error">{errorPerfil}</p>}
              {exitoPerfil && <p className="mi-perfil-exito">Perfil actualizado correctamente.</p>}

              <button type="submit" className="mi-perfil-boton-primario" disabled={guardandoPerfil}>
                {guardandoPerfil ? 'Guardando…' : 'Guardar cambios'}
              </button>
            </form>
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

          <section className="mi-perfil-card mi-perfil-card-riesgo">
            <h2>Zona de riesgo</h2>
            <p className="text-secondary mi-perfil-card-subtitulo">
              {esModoLocal
                ? 'Cierra la sesión de este navegador (esta instalación no puede revocar sesiones de otros dispositivos).'
                : 'Cierra la sesión en todos los dispositivos conectados con esta cuenta.'}
            </p>
            <button
              type="button"
              className="mi-perfil-boton-peligro"
              onClick={() => void handleCerrarTodasLasSesiones()}
              disabled={cerrandoSesiones}
            >
              {cerrandoSesiones ? 'Cerrando…' : esModoLocal ? 'Cerrar sesión' : 'Cerrar todas las sesiones'}
            </button>
            {errorCerrarSesiones && <p className="mi-perfil-error">{errorCerrarSesiones}</p>}
          </section>
        </div>

        {(esLemy || esAdmin) && (
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
                    {verificandoEstado ? (
                      <span className="text-muted">Verificando…</span>
                    ) : !estadoSistema?.ultimoBackupUtc ? (
                      <span className="mi-perfil-estado-punto mi-perfil-estado-desconocido">
                        Nunca corrió (o deshabilitado en esta instalación)
                      </span>
                    ) : (
                      <span
                        className={`mi-perfil-estado-punto ${estadoSistema.ultimoBackupExitoso ? 'mi-perfil-estado-bien' : 'mi-perfil-estado-mal'}`}
                      >
                        {estadoSistema.ultimoBackupExitoso ? 'Exitoso' : 'Con problemas'} —{' '}
                        {formateadorFechaHora.format(new Date(estadoSistema.ultimoBackupUtc))}
                      </span>
                    )}
                  </dd>
                </div>
                <div className="mi-perfil-estado-fila">
                  <dt>Almacenamiento de archivos</dt>
                  <dd>
                    {verificandoEstado ? (
                      <span className="text-muted">Verificando…</span>
                    ) : errorEstado ? (
                      <span className="mi-perfil-estado-punto mi-perfil-estado-mal">Sin verificar</span>
                    ) : (
                      <span
                        className={`mi-perfil-estado-punto ${estadoSistema?.almacenamientoOperativo ? 'mi-perfil-estado-bien' : 'mi-perfil-estado-mal'}`}
                      >
                        {estadoSistema?.almacenamientoOperativo ? 'Operativo' : 'Con problemas'}
                      </span>
                    )}
                  </dd>
                </div>
                {estadoSistema?.modoMantenimientoActivo && (
                  <div className="mi-perfil-estado-fila">
                    <dt>Modo mantenimiento</dt>
                    <dd>
                      <span className="mi-perfil-estado-punto mi-perfil-estado-mal">Activo</span>
                    </dd>
                  </div>
                )}
              </dl>
            </section>

            {esLemy && (
            <section className="mi-perfil-card">
              <h2>Herramientas de soporte</h2>
              <p className="text-secondary mi-perfil-card-subtitulo">Acciones rápidas para resolver incidencias comunes.</p>

              <div className="mi-perfil-herramientas-grilla">
                <article className="mi-perfil-herramienta">
                  <span className="mi-perfil-herramienta-icono" aria-hidden="true">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M4 7h16M9 7V4h6v3m-8 0 1 13a2 2 0 0 0 2 2h4a2 2 0 0 0 2-2l1-13"
                        stroke="currentColor"
                        strokeWidth="1.6"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </span>
                  <div className="mi-perfil-herramienta-texto">
                    <p className="mi-perfil-herramienta-titulo">Vaciar caché del sistema</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">No aplica: la API no mantiene caché de servidor.</p>
                  </div>
                  <button type="button" disabled>
                    No aplica
                  </button>
                </article>

                <article className="mi-perfil-herramienta">
                  <span className="mi-perfil-herramienta-icono" aria-hidden="true">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <ellipse cx="12" cy="5.5" rx="7" ry="2.5" stroke="currentColor" strokeWidth="1.6" />
                      <path
                        d="M5 5.5V18c0 1.4 3.1 2.5 7 2.5s7-1.1 7-2.5V5.5M5 12c0 1.4 3.1 2.5 7 2.5s7-1.1 7-2.5"
                        stroke="currentColor"
                        strokeWidth="1.6"
                        strokeLinecap="round"
                      />
                    </svg>
                  </span>
                  <div className="mi-perfil-herramienta-texto">
                    <p className="mi-perfil-herramienta-titulo">Verificar conexiones (base de datos y almacenamiento)</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">Comprueba que los datos y las fotos se estén guardando correctamente.</p>
                  </div>
                  <button type="button" onClick={() => void verificarEstadoSistema()} disabled={verificandoEstado}>
                    {verificandoEstado ? 'Verificando…' : 'Ejecutar'}
                  </button>
                </article>

                <article className="mi-perfil-herramienta">
                  <span className="mi-perfil-herramienta-icono" aria-hidden="true">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M12 3v13m0 0 4-4m-4 4-4-4M5 19h14"
                        stroke="currentColor"
                        strokeWidth="1.6"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </span>
                  <div className="mi-perfil-herramienta-texto">
                    <p className="mi-perfil-herramienta-titulo">Exportar registro de actividad</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">Descarga los últimos 30 días de bitácora en CSV.</p>
                  </div>
                  <button type="button" onClick={() => void handleExportarActividad()} disabled={exportando}>
                    {exportando ? 'Exportando…' : 'Ejecutar'}
                  </button>
                </article>

                <article className="mi-perfil-herramienta">
                  <span className="mi-perfil-herramienta-icono" aria-hidden="true">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M4 12a8 8 0 0 1 13.66-5.66M20 12a8 8 0 0 1-13.66 5.66M17 3v4h-4M7 21v-4h4"
                        stroke="currentColor"
                        strokeWidth="1.6"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </span>
                  <div className="mi-perfil-herramienta-texto">
                    <p className="mi-perfil-herramienta-titulo">Forzar sincronización de datos</p>
                    <p className="text-muted mi-perfil-herramienta-detalle">No aplica: no hay un sistema externo con el que sincronizar.</p>
                  </div>
                  <button type="button" disabled>
                    No aplica
                  </button>
                </article>
              </div>
              {errorExportar && <p className="mi-perfil-error">{errorExportar}</p>}
              {errorEstado && <p className="mi-perfil-error">{errorEstado}</p>}
            </section>
            )}

            {esLemy && (
              <section className="mi-perfil-card mi-perfil-card-riesgo">
                <h2>Modo mantenimiento</h2>
                <p className="text-secondary mi-perfil-card-subtitulo">
                  Bloquea el acceso a todo el personal (menos Lemy) mientras resuelves una incidencia.
                </p>

                <div className="mi-perfil-mantenimiento-fila">
                  <div>
                    <p className="mi-perfil-herramienta-titulo">
                      {estadoSistema?.modoMantenimientoActivo ? 'Activo' : 'Apagado'}
                    </p>
                    {estadoSistema?.modoMantenimientoActivo && estadoSistema.modoMantenimientoMensaje && (
                      <p className="text-muted mi-perfil-herramienta-detalle">
                        Mensaje actual: "{estadoSistema.modoMantenimientoMensaje}"
                      </p>
                    )}
                  </div>
                  <button
                    type="button"
                    role="switch"
                    aria-checked={estadoSistema?.modoMantenimientoActivo ?? false}
                    className={`mi-perfil-interruptor ${estadoSistema?.modoMantenimientoActivo ? 'activo' : ''}`}
                    onClick={() => void handleToggleMantenimiento()}
                    disabled={cambiandoMantenimiento || verificandoEstado}
                  >
                    <span className="mi-perfil-interruptor-perilla" />
                  </button>
                </div>
                {errorMantenimiento && <p className="mi-perfil-error">{errorMantenimiento}</p>}
              </section>
            )}
          </div>
        )}
      </div>
    </DashboardLayout>
  )
}
