import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { api } from '../../lib/api'
import type { RolUsuario } from '../../types/usuario'
import type { CitaAgenda } from '../../types/cita'
import './Sidebar.css'

interface ItemNav {
  to: string
  etiqueta: string
  badge?: 'pacientesEnEspera'
}

const ITEMS_POR_ROL: Partial<Record<RolUsuario, ItemNav[]>> = {
  Admin: [
    { to: '/dashboard', etiqueta: 'Dashboard' },
    { to: '/resumen', etiqueta: 'Resumen' },
    { to: '/finanzas', etiqueta: 'Finanzas' },
    { to: '/gastos', etiqueta: 'Gastos' },
    { to: '/personal', etiqueta: 'Personal' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal' },
    { to: '/directorio', etiqueta: 'Directorio' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras' },
    { to: '/actividad', etiqueta: 'Actividad' },
  ],
  Doctor: [
    { to: '/dashboard-doctor', etiqueta: 'Dashboard' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/citas', etiqueta: 'Citas' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
  Fondos: [
    { to: '/caja', etiqueta: 'Caja' },
    { to: '/cobros', etiqueta: 'Cobros' },
    { to: '/recepcion', etiqueta: 'Recepción', badge: 'pacientesEnEspera' },
    { to: '/agenda', etiqueta: 'Agenda' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
  Farmacia: [
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
}

const INTERVALO_BADGE_MS = 20000

export function Sidebar() {
  const { perfil } = useAuth()
  const [pacientesEnEspera, setPacientesEnEspera] = useState(0)

  useEffect(() => {
    if (perfil?.rol !== 'Fondos') {
      return
    }

    let cancelado = false

    const cargarConteo = () => {
      api
        .get<CitaAgenda[]>('/api/citas/sala-espera')
        .then((datos) => {
          if (!cancelado) setPacientesEnEspera(datos.length)
        })
        .catch(() => undefined)
    }

    cargarConteo()
    const intervalo = setInterval(cargarConteo, INTERVALO_BADGE_MS)

    return () => {
      cancelado = true
      clearInterval(intervalo)
    }
  }, [perfil?.rol])

  const itemsDelRol = (perfil && ITEMS_POR_ROL[perfil.rol]) || []
  const items = perfil ? [...itemsDelRol, { to: '/mi-perfil', etiqueta: 'Mi perfil' }] : itemsDelRol

  return (
    <aside className="sidebar">
      <div className="sidebar-marca">
        <img className="sidebar-marca-icono" src="/logo-funbide.png" alt="FUNBIDE" />
        <span>FUNBIDE</span>
      </div>

      <nav className="sidebar-nav">
        {items.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) => `sidebar-nav-item${isActive ? ' activo' : ''}`}
          >
            <span className="sidebar-nav-punto" />
            {item.etiqueta}
            {item.badge === 'pacientesEnEspera' && pacientesEnEspera > 0 && (
              <span className="sidebar-nav-badge">{pacientesEnEspera}</span>
            )}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
