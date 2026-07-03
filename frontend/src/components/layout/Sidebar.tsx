import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import type { RolUsuario } from '../../types/usuario'
import './Sidebar.css'

const ITEMS_POR_ROL: Partial<Record<RolUsuario, { to: string; etiqueta: string }[]>> = {
  Admin: [
    { to: '/dashboard', etiqueta: 'Dashboard' },
    { to: '/resumen', etiqueta: 'Resumen' },
    { to: '/directorio', etiqueta: 'Personal' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal' },
    { to: '/directorio', etiqueta: 'Directorio' },
    { to: '/pacientes', etiqueta: 'Pacientes' },
  ],
}

export function Sidebar() {
  const { perfil } = useAuth()
  const items = (perfil && ITEMS_POR_ROL[perfil.rol]) || []

  return (
    <aside className="sidebar">
      <div className="sidebar-marca">
        <span className="sidebar-marca-icono">F</span>
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
