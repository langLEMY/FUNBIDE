import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { limpiarMantenimiento } from '../lib/mantenimientoBus'
import './ModoMantenimientoScreen.css'

interface ModoMantenimientoScreenProps {
  mensaje: string | null
}

/**
 * Pantalla de bloqueo total mientras MantenimientoMiddleware devuelve 503 a todo el
 * personal (menos Lemy, que nunca la ve) — ver mantenimientoBus.ts, que la dispara
 * desde api.ts.
 */
export function ModoMantenimientoScreen({ mensaje }: ModoMantenimientoScreenProps) {
  const { cerrarSesion } = useAuth()
  const navigate = useNavigate()

  const handleVolverAlLogin = () => {
    // Saca primero la pantalla de bloqueo (si no, App.tsx la sigue mostrando encima de
    // cualquier ruta aunque cambie la URL). cerrarSesion es best-effort: si falla (ej.
    // ya no hay sesión), igual navegamos a /login.
    limpiarMantenimiento()
    void cerrarSesion().finally(() => navigate('/login', { replace: true }))
  }

  return (
    <div className="modo-mantenimiento-pantalla">
      <p className="modo-mantenimiento-marca">FUNBIDE</p>

      <svg
        className="modo-mantenimiento-icono"
        width="140"
        height="120"
        viewBox="0 0 140 120"
        fill="none"
        aria-hidden="true"
      >
        <rect x="34" y="10" width="72" height="88" rx="8" className="modo-mantenimiento-icono-hoja" />
        <path
          d="M8 34c0-4.4 3.6-8 8-8h26l10 12h64c4.4 0 8 3.6 8 8v56c0 4.4-3.6 8-8 8H16c-4.4 0-8-3.6-8-8V34Z"
          className="modo-mantenimiento-icono-carpeta"
        />
        <circle cx="52" cy="72" r="4.5" className="modo-mantenimiento-icono-cara" />
        <circle cx="88" cy="72" r="4.5" className="modo-mantenimiento-icono-cara" />
        <path d="M56 88h28" strokeWidth="4" strokeLinecap="round" className="modo-mantenimiento-icono-cara-linea" />
      </svg>

      <p className="modo-mantenimiento-subtitulo">
        Oops... La página se encuentra en mantenimiento. Volveremos lo antes posible.
      </p>
      {mensaje && <p className="modo-mantenimiento-detalle">{mensaje}</p>}

      <div className="modo-mantenimiento-acciones">
        <button type="button" className="modo-mantenimiento-boton" onClick={() => window.location.reload()}>
          Reintentar
        </button>
        <button
          type="button"
          className="modo-mantenimiento-boton modo-mantenimiento-boton-secundario"
          onClick={handleVolverAlLogin}
        >
          Volver al inicio de sesión
        </button>
      </div>
    </div>
  )
}
