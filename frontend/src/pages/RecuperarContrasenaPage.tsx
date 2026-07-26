import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import './LoginPage.css'

export function RecuperarContrasenaPage() {
  const { recuperarContrasena } = useAuth()

  const [correo, setCorreo] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [enviado, setEnviado] = useState(false)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (!correo.trim()) {
      setError('Ingresa tu correo.')
      return
    }

    setEnviando(true)
    try {
      await recuperarContrasena(correo.trim())
      setEnviado(true)
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : undefined
      setError(traducirErrorAuth(mensaje, 'No se pudo enviar el enlace. Inténtalo de nuevo.'))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-page">
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
          <h1 className="login-titulo">Recuperar contraseña</h1>

          {enviado ? (
            <>
              <p className="login-subtitulo">
                Si el correo <strong>{correo}</strong> existe en nuestro sistema, te enviamos un enlace para
                restablecer tu contraseña.
              </p>
              <Link className="login-boton login-boton-enlace" to="/login">
                Volver a iniciar sesión
              </Link>
            </>
          ) : (
            <>
              <p className="login-subtitulo">Ingresa tu correo y te enviaremos un enlace para restablecerla.</p>

              <label className="login-label" htmlFor="correo">
                Correo
              </label>
              <input
                id="correo"
                className="login-input"
                type="email"
                placeholder="correo@funbide.org"
                autoComplete="username"
                value={correo}
                onChange={(event) => setCorreo(event.target.value)}
              />

              {error && <p className="login-error">{error}</p>}

              <button className="login-boton" type="submit" disabled={enviando}>
                {enviando && <span className="login-spinner" aria-hidden="true" />}
                {enviando ? 'Enviando…' : 'Enviar enlace'}
              </button>

              <Link className="login-olvido login-olvido-centrado" to="/login">
                Volver a iniciar sesión
              </Link>
            </>
          )}
        </div>
      </form>

      <p className="login-footer">© {new Date().getFullYear()} Fundación Bienestar y Desarrollo</p>
    </div>
  )
}
