import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { api } from '../../lib/api'
import { iniciales } from '../../lib/iniciales'
import type { RolUsuario } from '../../types/usuario'
import type { CitaAgenda } from '../../types/cita'
import { IconoNav, type NombreIconoNav } from './IconoNav'
import './Sidebar.css'

interface ItemNav {
  to: string
  etiqueta: string
  grupo: string
  icono: NombreIconoNav
  badge?: 'pacientesEnEspera'
}

const ITEMS_POR_ROL: Partial<Record<RolUsuario, ItemNav[]>> = {
  Admin: [
    { to: '/dashboard', etiqueta: 'Dashboard', grupo: 'Principal', icono: 'grid' },
    { to: '/resumen', etiqueta: 'Resumen', grupo: 'Principal', icono: 'chart' },
    { to: '/finanzas', etiqueta: 'Finanzas', grupo: 'Finanzas', icono: 'dollar' },
    { to: '/gastos', etiqueta: 'Gastos', grupo: 'Finanzas', icono: 'minuscircle' },
    { to: '/donaciones', etiqueta: 'Donaciones', grupo: 'Finanzas', icono: 'heart' },
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical' },
    { to: '/operaciones', etiqueta: 'Operaciones', grupo: 'Operaciones', icono: 'activity' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield' },
    { to: '/actividad', etiqueta: 'Actividad', grupo: 'Sistema', icono: 'clock' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/directorio', etiqueta: 'Directorio', grupo: 'Gestión', icono: 'book' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield' },
    { to: '/actividad', etiqueta: 'Actividad', grupo: 'Sistema', icono: 'clock' },
  ],
  Doctor: [
    { to: '/dashboard-doctor', etiqueta: 'Dashboard', grupo: 'Principal', icono: 'grid' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Clínico', icono: 'medical' },
    { to: '/citas', etiqueta: 'Citas', grupo: 'Clínico', icono: 'calendar' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box' },
  ],
  Fondos: [
    { to: '/caja', etiqueta: 'Caja', grupo: 'Caja', icono: 'card' },
    { to: '/cobros', etiqueta: 'Cobros', grupo: 'Caja', icono: 'dollar' },
    { to: '/recepcion', etiqueta: 'Recepción', grupo: 'Agenda', icono: 'inbox', badge: 'pacientesEnEspera' },
    { to: '/agenda', etiqueta: 'Agenda', grupo: 'Agenda', icono: 'calendar' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Operaciones', icono: 'medical' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box' },
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

  const items = (perfil && ITEMS_POR_ROL[perfil.rol]) || []

  let grupoAnterior: string | null = null

  return (
    <aside className="sidebar">
      <div className="sidebar-marca">
        <img className="sidebar-marca-icono" src="/logo-funbide.png" alt="FUNBIDE" />
        <span>FUNBIDE</span>
      </div>

      <nav className="sidebar-nav">
        {items.map((item) => {
          const esInicioDeGrupo = item.grupo !== grupoAnterior
          grupoAnterior = item.grupo

          return (
            <div key={item.to} className="sidebar-nav-grupo-item">
              {esInicioDeGrupo && <div className="sidebar-nav-grupo-titulo">{item.grupo}</div>}
              <NavLink to={item.to} className={({ isActive }) => `sidebar-nav-item${isActive ? ' activo' : ''}`}>
                <span className="sidebar-nav-icono">
                  <IconoNav nombre={item.icono} />
                </span>
                <span style={{ flex: 1 }}>{item.etiqueta}</span>
                {item.badge === 'pacientesEnEspera' && pacientesEnEspera > 0 && (
                  <span className="sidebar-nav-badge">{pacientesEnEspera}</span>
                )}
              </NavLink>
            </div>
          )
        })}
      </nav>

      {perfil && (
        <NavLink to="/mi-perfil" className={({ isActive }) => `sidebar-perfil${isActive ? ' activo' : ''}`}>
          {perfil.fotoPerfilUrl ? (
            <img className="sidebar-perfil-avatar" src={perfil.fotoPerfilUrl} alt="" />
          ) : (
            <span className="sidebar-perfil-avatar sidebar-perfil-avatar-iniciales">
              {iniciales(perfil.nombreCompleto)}
            </span>
          )}
          <span className="sidebar-perfil-texto">
            <span className="sidebar-perfil-nombre">{perfil.nombreCompleto}</span>
            <span className="sidebar-perfil-subtitulo">Ver perfil</span>
          </span>
        </NavLink>
      )}
    </aside>
  )
}
