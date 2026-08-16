import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { api } from '../../lib/api'
import { iniciales } from '../../lib/iniciales'
import type { ModuloPermiso, RolUsuario } from '../../types/usuario'
import type { CitaAgenda } from '../../types/cita'
import { IconoNav, type NombreIconoNav } from './IconoNav'
import './Sidebar.css'

interface ItemNav {
  to: string
  etiqueta: string
  grupo: string
  icono: NombreIconoNav
  badge?: 'pacientesEnEspera'
  /** Si está presente, el ítem además exige que el usuario tenga este módulo en perfil.permisos (ver Gestionar Permisos). Ausente = no togglable (siempre visible para el rol). */
  modulo?: ModuloPermiso
}

const ITEMS_POR_ROL: Partial<Record<RolUsuario, ItemNav[]>> = {
  Admin: [
    { to: '/dashboard', etiqueta: 'Dashboard', grupo: 'Principal', icono: 'grid', modulo: 'Dashboard' },
    { to: '/resumen', etiqueta: 'Resumen', grupo: 'Principal', icono: 'chart', modulo: 'Resumen' },
    { to: '/finanzas', etiqueta: 'Finanzas', grupo: 'Finanzas', icono: 'dollar', modulo: 'Finanzas' },
    { to: '/gastos', etiqueta: 'Gastos', grupo: 'Finanzas', icono: 'minuscircle', modulo: 'Gastos' },
    { to: '/donaciones', etiqueta: 'Donaciones', grupo: 'Finanzas', icono: 'heart', modulo: 'Donaciones' },
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/permisos', etiqueta: 'Gestionar Permisos', grupo: 'Gestión', icono: 'lock' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical', modulo: 'Pacientes' },
    { to: '/operaciones', etiqueta: 'Operaciones', grupo: 'Operaciones', icono: 'activity', modulo: 'Operaciones' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield', modulo: 'Aseguradoras' },
    { to: '/actividad', etiqueta: 'Actividad', grupo: 'Sistema', icono: 'clock', modulo: 'Actividad' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/permisos', etiqueta: 'Gestionar Permisos', grupo: 'Gestión', icono: 'lock' },
    { to: '/directorio', etiqueta: 'Directorio', grupo: 'Gestión', icono: 'book', modulo: 'Directorio' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical', modulo: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield', modulo: 'Aseguradoras' },
    { to: '/actividad', etiqueta: 'Actividad', grupo: 'Sistema', icono: 'clock', modulo: 'Actividad' },
  ],
  Doctor: [
    { to: '/dashboard-doctor', etiqueta: 'Dashboard', grupo: 'Principal', icono: 'grid' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Clínico', icono: 'medical', modulo: 'Pacientes' },
    { to: '/citas', etiqueta: 'Citas', grupo: 'Clínico', icono: 'calendar' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
  ],
  Fondos: [
    { to: '/caja', etiqueta: 'Caja', grupo: 'Caja', icono: 'card', modulo: 'Caja' },
    { to: '/cobros', etiqueta: 'Cobros', grupo: 'Caja', icono: 'dollar', modulo: 'Cobros' },
    {
      to: '/recepcion',
      etiqueta: 'Recepción',
      grupo: 'Agenda',
      icono: 'inbox',
      badge: 'pacientesEnEspera',
      modulo: 'Recepcion',
    },
    { to: '/agenda', etiqueta: 'Agenda', grupo: 'Agenda', icono: 'calendar', modulo: 'Agenda' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Operaciones', icono: 'medical', modulo: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
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

  const items = ((perfil && ITEMS_POR_ROL[perfil.rol]) || []).filter(
    (item) => !item.modulo || (perfil?.permisos ?? []).includes(item.modulo),
  )

  let grupoAnterior: string | null = null

  return (
    <aside className="sidebar">
      <div className="sidebar-marca">
        <img className="sidebar-marca-icono" src="/logo-funbide.png" alt="FUNBIDE" />
        <span>FUNBIDE</span>
      </div>

      {perfil && (
        <p className="sidebar-saludo">
          Hola, <strong>{perfil.nombreCompleto.trim().split(/\s+/)[0]}</strong>
        </p>
      )}

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
