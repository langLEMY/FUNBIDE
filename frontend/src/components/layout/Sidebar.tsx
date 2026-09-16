import { ChevronsLeft, ChevronsRight } from 'lucide-react'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { api } from '../../lib/api'
import { iniciales } from '../../lib/iniciales'
import type { ModuloPermiso, RolUsuario } from '../../types/usuario'
import type { CitaAgenda } from '../../types/cita'
import { IconoNav, type NombreIconoNav } from './IconoNav'
import './Sidebar.css'

const CLAVE_SIDEBAR_COLAPSADO = 'funbide-sidebar-colapsado'

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
    { to: '/caja', etiqueta: 'Caja', grupo: 'Finanzas', icono: 'card', modulo: 'Caja' },
    { to: '/cobros', etiqueta: 'Cobros', grupo: 'Finanzas', icono: 'dollar', modulo: 'Cobros' },
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/permisos', etiqueta: 'Gestionar Permisos', grupo: 'Gestión', icono: 'lock' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical', modulo: 'Pacientes' },
    { to: '/operaciones', etiqueta: 'Operaciones', grupo: 'Operaciones', icono: 'activity', modulo: 'Operaciones' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield', modulo: 'Aseguradoras' },
    { to: '/servicios', etiqueta: 'Precios privados', grupo: 'Operaciones', icono: 'dollar', modulo: 'Servicios' },
    {
      to: '/sala-espera',
      etiqueta: 'Sala de espera',
      grupo: 'Operaciones',
      icono: 'inbox',
      modulo: 'Recepcion',
    },
    { to: '/actividad', etiqueta: 'Actividad', grupo: 'Sistema', icono: 'clock', modulo: 'Actividad' },
  ],
  Lemy: [
    { to: '/personal', etiqueta: 'Personal', grupo: 'Gestión', icono: 'users' },
    { to: '/permisos', etiqueta: 'Gestionar Permisos', grupo: 'Gestión', icono: 'lock' },
    { to: '/directorio', etiqueta: 'Directorio', grupo: 'Gestión', icono: 'book', modulo: 'Directorio' },
    { to: '/pacientes', etiqueta: 'Pacientes', grupo: 'Gestión', icono: 'medical', modulo: 'Pacientes' },
    { to: '/inventario', etiqueta: 'Inventario', grupo: 'Operaciones', icono: 'box', modulo: 'Inventario' },
    { to: '/aseguradoras', etiqueta: 'Aseguradoras', grupo: 'Operaciones', icono: 'shield', modulo: 'Aseguradoras' },
    { to: '/servicios', etiqueta: 'Precios privados', grupo: 'Operaciones', icono: 'dollar', modulo: 'Servicios' },
    {
      to: '/sala-espera',
      etiqueta: 'Sala de espera',
      grupo: 'Operaciones',
      icono: 'inbox',
      modulo: 'Recepcion',
    },
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
  const [colapsado, setColapsado] = useState(
    () => window.localStorage.getItem(CLAVE_SIDEBAR_COLAPSADO) === '1',
  )

  const alternarColapsado = () => {
    setColapsado((actual) => {
      const nuevo = !actual
      window.localStorage.setItem(CLAVE_SIDEBAR_COLAPSADO, nuevo ? '1' : '0')
      return nuevo
    })
  }

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
    <aside className={`sidebar${colapsado ? ' sidebar-colapsado' : ''}`}>
      <div className="sidebar-marca">
        <img className="sidebar-marca-icono" src="/logo-funbide.png" alt="FUNBIDE" />
        <span className="sidebar-texto-colapsable">FUNBIDE</span>
      </div>

      {perfil && (
        <p className="sidebar-saludo sidebar-texto-colapsable">
          Hola, <strong>{perfil.nombreCompleto.trim().split(/\s+/)[0]}</strong>
        </p>
      )}

      <nav className="sidebar-nav">
        {items.map((item) => {
          const esInicioDeGrupo = item.grupo !== grupoAnterior
          grupoAnterior = item.grupo

          return (
            <div key={item.to} className="sidebar-nav-grupo-item">
              {esInicioDeGrupo && <div className="sidebar-nav-grupo-titulo sidebar-texto-colapsable">{item.grupo}</div>}
              <NavLink
                to={item.to}
                className={({ isActive }) => `sidebar-nav-item${isActive ? ' activo' : ''}`}
                title={colapsado ? item.etiqueta : undefined}
              >
                <span className="sidebar-nav-icono">
                  <IconoNav nombre={item.icono} />
                </span>
                <span className="sidebar-nav-etiqueta sidebar-texto-colapsable">{item.etiqueta}</span>
                {item.badge === 'pacientesEnEspera' && pacientesEnEspera > 0 && (
                  <span className="sidebar-nav-badge sidebar-texto-colapsable">{pacientesEnEspera}</span>
                )}
              </NavLink>
            </div>
          )
        })}
      </nav>

      {perfil && (
        <NavLink
          to="/mi-perfil"
          className={({ isActive }) => `sidebar-perfil${isActive ? ' activo' : ''}`}
          title={colapsado ? 'Ver perfil' : undefined}
        >
          {perfil.fotoPerfilUrl ? (
            <img className="sidebar-perfil-avatar" src={perfil.fotoPerfilUrl} alt="" />
          ) : (
            <span className="sidebar-perfil-avatar sidebar-perfil-avatar-iniciales">
              {iniciales(perfil.nombreCompleto)}
            </span>
          )}
          <span className="sidebar-perfil-texto sidebar-texto-colapsable">
            <span className="sidebar-perfil-nombre">{perfil.nombreCompleto}</span>
            <span className="sidebar-perfil-subtitulo">Ver perfil</span>
          </span>
        </NavLink>
      )}

      <button
        type="button"
        className="sidebar-colapsar-boton"
        onClick={alternarColapsado}
        aria-label={colapsado ? 'Expandir menú' : 'Colapsar menú'}
      >
        {colapsado ? <ChevronsRight size={16} aria-hidden="true" /> : <ChevronsLeft size={16} aria-hidden="true" />}
      </button>
    </aside>
  )
}
