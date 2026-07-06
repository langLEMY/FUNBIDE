import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { supabase } from '../lib/supabaseClient'
import { useAuth } from '../auth/AuthContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import { FondoBlobs } from '../components/layout/FondoBlobs'
import './LoginPage.css'

/**
 * El enlace de recuperación de Supabase trae el token en el propio URL y, al
 * cargar, dispara el evento "PASSWORD_RECOVERY" con una sesión temporal — no es
 * un login normal, así que esta página no pasa por ProtectedRoute/session.
 */
export function RestablecerContrasenaPage() {
  const { restablecerContrasena } = useAuth()
  const navigate = useNavigate()

  const [verificando, setVerificando] = useState(true)
  const [listoParaRestablecer, setListoParaRestablecer] = useState(false)
  const [nuevaContrasena, setNuevaContrasena] = useState('')
  const [confirmarContrasena, setConfirmarContrasena] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [completado, setCompletado] = useState(false)

  useEffect(() => {
    const { data: listener } = supabase.auth.onAuthStateChange((event) => {
      if (event === 'PASSWORD_RECOVERY') {
        setListoParaRestablecer(true)
        setVerificando(false)
      }
    })

    supabase.auth.getSession().then(({ data }) => {
      if (data.session) {
        setListoParaRestablecer(true)
      }
      setVerificando(false)
    })

    return () => listener.subscription.unsubscribe()
  }, [])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (nuevaContrasena.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.')
      return
    }
    if (nuevaContrasena !== confirmarContrasena) {
      setError('Las contraseñas no coinciden.')
      return
    }

    setEnviando(true)
    try {
      await restablecerContrasena(nuevaContrasena)
      setCompletado(true)
      setTimeout(() => navigate('/login', { replace: true }), 2000)
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : undefined
      setError(traducirErrorAuth(mensaje, 'No se pudo restablecer la contraseña.'))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-page">
      <FondoBlobs />

      <form className="login-card" onSubmit={(event) => void handleSubmit(event)}>
        <div className="login-ventana-barra">
          <div className="login-ventana-puntos">
            <span className="login-punto login-punto-rojo" />
            <span className="login-punto login-punto-amarillo" />
            <span className="login-punto login-punto-verde" />
          </div>
          <span className="login-ventana-titulo">FUNBIDE Admin</span>
          <span className="login-ventana-espaciador" />
        </div>

        <div className="login-contenido">
          <img className="login-logo" src="/logo-funbide.png" alt="FUNBIDE" />
          <h1 className="login-titulo">Restablecer contraseña</h1>

          {verificando ? (
            <p className="login-subtitulo">Verificando enlace…</p>
          ) : completado ? (
            <p className="login-subtitulo">Contraseña actualizada. Redirigiendo a iniciar sesión…</p>
          ) : !listoParaRestablecer ? (
            <>
              <p className="login-subtitulo">Este enlace no es válido o ya expiró. Solicita uno nuevo.</p>
              <Link className="login-boton login-boton-enlace" to="/recuperar-contrasena">
                Solicitar un enlace nuevo
              </Link>
            </>
          ) : (
            <>
              <p className="login-subtitulo">Ingresa tu nueva contraseña.</p>

              <label className="login-label" htmlFor="nueva-contrasena">
                Nueva contraseña
              </label>
              <input
                id="nueva-contrasena"
                className="login-input"
                type="password"
                autoComplete="new-password"
                value={nuevaContrasena}
                onChange={(event) => setNuevaContrasena(event.target.value)}
              />

              <label className="login-label" htmlFor="confirmar-contrasena">
                Confirmar contraseña
              </label>
              <input
                id="confirmar-contrasena"
                className="login-input"
                type="password"
                autoComplete="new-password"
                value={confirmarContrasena}
                onChange={(event) => setConfirmarContrasena(event.target.value)}
              />

              {error && <p className="login-error">{error}</p>}

              <button className="login-boton" type="submit" disabled={enviando}>
                {enviando && <span className="login-spinner" aria-hidden="true" />}
                {enviando ? 'Guardando…' : 'Guardar nueva contraseña'}
              </button>
            </>
          )}
        </div>
      </form>

      <p className="login-footer">© {new Date().getFullYear()} Fundación Bienestar y Desarrollo</p>
    </div>
  )
}
