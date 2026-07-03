import { useAuth } from '../auth/AuthContext'
import './EstadoBloqueado.css'

interface EstadoBloqueadoProps {
  mensaje: string
}

/**
 * Pantalla de punto muerto (rol sin panel, error cargando el perfil, etc.).
 * Nunca debe mostrarse sin una salida: siempre trae un botón de cerrar sesión,
 * porque en estos casos el layout normal (con el menú de perfil) no aplica.
 */
export function EstadoBloqueado({ mensaje }: EstadoBloqueadoProps) {
  const { perfil, cerrarSesion } = useAuth()

  return (
    <div className="estado-bloqueado">
      <p className="estado-bloqueado-mensaje text-secondary">{mensaje}</p>
      {perfil && <p className="estado-bloqueado-sesion text-muted">Sesión iniciada como {perfil.correo}</p>}
      <button type="button" className="estado-bloqueado-boton" onClick={() => void cerrarSesion()}>
        Cerrar sesión
      </button>
    </div>
  )
}
