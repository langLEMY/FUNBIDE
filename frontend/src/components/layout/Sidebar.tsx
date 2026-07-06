import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import type { RolUsuario } from '../../types/usuario'
import './Sidebar.css'

const ITEMS_POR_ROL: Partial<Record<RolUsuario, { to: string; etiqueta: string }[]>> = {
  Admin: [
    { to: '/dashboard', etiqueta: 'Dashboard' },
    { to: '/resumen', etiqueta: 'Resumen' },
    { to: '/equipo', etiqueta: 'Personal' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal' },
    { to: '/directorio', etiqueta: 'Directorio' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
    { to: '/actividad', etiqueta: 'Actividad' },
  ],
  Doctor: [
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/citas', etiqueta: 'Citas' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
  Fondos: [
    { to: '/finanzas', etiqueta: 'Finanzas' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
  Farmacia: [
    { to: '/pacientes', etiqueta: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario' },
  ],
}

export function Sidebar() {
  const { perfil } = useAuth()
  const items = (perfil && ITEMS_POR_ROL[perfil.rol]) || []

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
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
