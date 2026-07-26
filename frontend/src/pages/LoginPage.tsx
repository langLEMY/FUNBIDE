import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useTheme } from '../theme/ThemeContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import { ThemeToggleButton } from '../components/layout/ThemeToggleButton'
import './LoginPage.css'

const CLAVE_USUARIO_RECORDADO = 'funbide-usuario-recordado'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string

/**
 * El login ocurre directamente contra Supabase Auth desde el navegador, sin pasar
 * por esta API, así que el backend nunca se entera de un intento a menos que se lo
 * digamos explícitamente. Se envía "mejor esfuerzo": si esta llamada falla no debe
 * bloquear ni afectar el flujo de inicio de sesión del usuario.
 */
function registrarEventoLogin(correo: string, exitoso: boolean) {
  fetch(`${apiBaseUrl}/api/auth/eventos-login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ correo, exitoso }),
  }).catch(() => {
    // Best-effort: un fallo aquí no debe afectar la experiencia de inicio de sesión.
  })
}

export function LoginPage() {
  const { iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const { tema } = useTheme()

  const [correo, setCorreo] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [mostrarContrasena, setMostrarContrasena] = useState(false)
  const [recordarme, setRecordarme] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  useEffect(() => {
    const guardado = window.localStorage.getItem(CLAVE_USUARIO_RECORDADO)
    if (guardado) {
      setCorreo(guardado)
      setRecordarme(true)
    }
  }, [])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (!correo.trim() || !contrasena.trim()) {
      setError('Ingresa tu usuario y tu contraseña.')
      return
    }

    setEnviando(true)
    try {
      await iniciarSesion(correo, contrasena)
      registrarEventoLogin(correo, true)
      if (recordarme) {
        window.localStorage.setItem(CLAVE_USUARIO_RECORDADO, correo)
      } else {
        window.localStorage.removeItem(CLAVE_USUARIO_RECORDADO)
      }
      navigate('/', { replace: true })
    } catch (err) {
      registrarEventoLogin(correo, false)
      const mensaje = err instanceof Error ? err.message : undefined
      setError(traducirErrorAuth(mensaje, 'No se pudo iniciar sesión.'))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-page">
      <ThemeToggleButton className="login-tema-boton" />

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
          <img
            className="login-logo"
            src={tema === 'oscuro' ? '/logo-funbide-wordmark-oscuro.png' : '/logo-funbide-wordmark-claro.png'}
            alt="FUNBIDE"
          />
          <h1 className="login-titulo">Bienvenido de nuevo</h1>
          <p className="login-subtitulo">Ingresa tus credenciales para acceder al panel.</p>

          <label className="login-label" htmlFor="correo">
            Usuario
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

          <label className="login-label" htmlFor="contrasena">
            Contraseña
          </label>
          <div className="login-input-contrasena">
            <input
              id="contrasena"
              className="login-input"
              type={mostrarContrasena ? 'text' : 'password'}
              placeholder="Ingresa tu contraseña"
              autoComplete="current-password"
              value={contrasena}
              onChange={(event) => setContrasena(event.target.value)}
            />
            <button
              type="button"
              className="login-mostrar-contrasena"
              onClick={() => setMostrarContrasena((valor) => !valor)}
              aria-label={mostrarContrasena ? 'Ocultar contraseña' : 'Mostrar contraseña'}
            >
              {mostrarContrasena ? (
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                  <path
                    d="M3 3l18 18M10.6 10.6a2 2 0 0 0 2.8 2.8M9.4 5.5A9.7 9.7 0 0 1 12 5c5 0 9 4 10 7-.4 1.2-1.3 2.6-2.5 3.9M6.3 6.9C4.2 8.2 2.6 10 2 12c1 3 5 7 10 7 1.1 0 2.1-.2 3.1-.5"
                    stroke="currentColor"
                    strokeWidth="1.6"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              ) : (
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                  <path
                    d="M2 12c1-3 5-7 10-7s9 4 10 7c-1 3-5 7-10 7s-9-4-10-7z"
                    stroke="currentColor"
                    strokeWidth="1.6"
                    strokeLinejoin="round"
                  />
                  <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="1.6" />
                </svg>
              )}
            </button>
          </div>

          <div className="login-fila-opciones">
            <label className="login-recordarme">
              <input
                type="checkbox"
                checked={recordarme}
                onChange={(event) => setRecordarme(event.target.checked)}
              />
              Recordarme
            </label>
            <Link className="login-olvido" to="/recuperar-contrasena">
              ¿Olvidaste tu contraseña?
            </Link>
          </div>

          {error && <p className="login-error">{error}</p>}

          <button className="login-boton" type="submit" disabled={enviando}>
            {enviando && <span className="login-spinner" aria-hidden="true" />}
            {enviando ? 'Verificando…' : 'Iniciar sesión'}
          </button>
        </div>
      </form>

      <p className="login-footer">© {new Date().getFullYear()} Fundación Bienestar y Desarrollo</p>
    </div>
  )
}
