import { zodResolver } from '@hookform/resolvers/zod'
import { Eye, EyeOff, TriangleAlert } from 'lucide-react'
import { useEffect, useRef, useState, type KeyboardEvent } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useTheme } from '../theme/ThemeContext'
import { traducirErrorAuth } from '../auth/mensajesError'
import { ThemeToggleButton } from '../components/layout/ThemeToggleButton'
import { esquemaLogin, type DatosLogin } from '../schemas/login'
import './LoginPage.css'

const CLAVE_USUARIO_RECORDADO = 'funbide-usuario-recordado'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string

/**
 * El login ocurre directamente contra Supabase Auth desde el navegador, sin pasar
 * por esta API, así que el backend nunca se entera de un intento a menos que se lo
 * digamos explícitamente. Se envía "mejor esfuerzo": si esta llamada falla no debe
 * bloquear ni afectar el flujo de inicio de sesión del usuario.
 */
function registrarEventoLogin(nombreUsuario: string, exitoso: boolean) {
  fetch(`${apiBaseUrl}/api/auth/eventos-login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nombreUsuario, exitoso }),
  }).catch(() => {
    // Best-effort: un fallo aquí no debe afectar la experiencia de inicio de sesión.
  })
}

export function LoginPage() {
  const { iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const { tema } = useTheme()

  const [mostrarContrasena, setMostrarContrasena] = useState(false)
  const [recordarme, setRecordarme] = useState(false)
  const [errorServidor, setErrorServidor] = useState<string | null>(null)
  const [capsLockActivo, setCapsLockActivo] = useState(false)
  // Se activa solo en el catch de onSubmit (credenciales rechazadas por el servidor) —
  // un error de validación de zod (campo vacío) no amerita este gesto, es demasiado.
  const [agitarTarjeta, setAgitarTarjeta] = useState(false)
  const timeoutAgitarRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<DatosLogin>({
    resolver: zodResolver(esquemaLogin),
    defaultValues: { nombreUsuario: '', contrasena: '' },
  })

  useEffect(() => {
    const guardado = window.localStorage.getItem(CLAVE_USUARIO_RECORDADO)
    if (guardado) {
      setValue('nombreUsuario', guardado)
      setRecordarme(true)
    }
  }, [setValue])

  const onSubmit = async (datos: DatosLogin) => {
    setErrorServidor(null)
    try {
      await iniciarSesion(datos.nombreUsuario, datos.contrasena)
      registrarEventoLogin(datos.nombreUsuario, true)
      if (recordarme) {
        window.localStorage.setItem(CLAVE_USUARIO_RECORDADO, datos.nombreUsuario)
      } else {
        window.localStorage.removeItem(CLAVE_USUARIO_RECORDADO)
      }
      navigate('/', { replace: true })
    } catch (err) {
      registrarEventoLogin(datos.nombreUsuario, false)
      const mensaje = err instanceof Error ? err.message : undefined
      setErrorServidor(traducirErrorAuth(mensaje, 'No se pudo iniciar sesión.'))

      if (timeoutAgitarRef.current) clearTimeout(timeoutAgitarRef.current)
      setAgitarTarjeta(true)
      timeoutAgitarRef.current = setTimeout(() => setAgitarTarjeta(false), 420)
    }
  }

  useEffect(() => {
    return () => {
      if (timeoutAgitarRef.current) clearTimeout(timeoutAgitarRef.current)
    }
  }, [])

  const handleTecladoContrasena = (event: KeyboardEvent<HTMLInputElement>) => {
    setCapsLockActivo(event.getModifierState('CapsLock'))
  }

  const errorMostrado = errors.nombreUsuario?.message ?? errors.contrasena?.message ?? errorServidor
  const registroContrasena = register('contrasena')

  return (
    <div className="login-page">
      <ThemeToggleButton className="login-tema-boton" />

      <form
        className={`login-card${agitarTarjeta ? ' login-card-agitar' : ''}${errorServidor ? ' login-card-error' : ''}`}
        onSubmit={handleSubmit(onSubmit)}
      >
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

          <div className="login-cuerpo">
            <h1 className="login-titulo">Bienvenido de nuevo</h1>
            <p className="login-subtitulo">Ingresa tus credenciales para acceder al panel.</p>

            <label className="login-label" htmlFor="nombreUsuario">
              Usuario
            </label>
            <input
              id="nombreUsuario"
              className="login-input"
              type="text"
              placeholder="usuario123"
              autoComplete="username"
              {...register('nombreUsuario')}
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
                {...registroContrasena}
                onKeyDown={handleTecladoContrasena}
                onKeyUp={handleTecladoContrasena}
                onBlur={(event) => {
                  void registroContrasena.onBlur(event)
                  setCapsLockActivo(false)
                }}
              />
              <button
                type="button"
                className="login-mostrar-contrasena"
                onClick={() => setMostrarContrasena((valor) => !valor)}
                aria-label={mostrarContrasena ? 'Ocultar contraseña' : 'Mostrar contraseña'}
              >
                {mostrarContrasena ? (
                  <EyeOff size={16} aria-hidden="true" />
                ) : (
                  <Eye size={16} aria-hidden="true" />
                )}
              </button>
            </div>

            {capsLockActivo && (
              <p className="login-caps-lock">
                <TriangleAlert size={13} aria-hidden="true" />
                Bloq Mayús está activado.
              </p>
            )}

            <div className="login-fila-opciones">
              <span className="login-recordarme">
                <button
                  type="button"
                  role="switch"
                  aria-checked={recordarme}
                  className={`login-interruptor${recordarme ? ' activo' : ''}`}
                  onClick={() => setRecordarme((valor) => !valor)}
                >
                  <span className="login-interruptor-perilla" />
                </button>
                <label onClick={() => setRecordarme((valor) => !valor)}>Recordarme</label>
              </span>
              <Link className="login-olvido" to="/recuperar-contrasena">
                ¿Olvidaste tu contraseña?
              </Link>
            </div>

            {errorMostrado && <p className="login-error">{errorMostrado}</p>}

            <button className="login-boton" type="submit" disabled={isSubmitting}>
              {isSubmitting && <span className="login-spinner" aria-hidden="true" />}
              {isSubmitting ? 'Verificando…' : 'Iniciar sesión'}
            </button>
          </div>
        </div>
      </form>

      <p className="login-footer">© {new Date().getFullYear()} Fundación Bienestar y Desarrollo</p>
      <p className="login-footer login-footer-autor">© By Lemy</p>
    </div>
  )
}
