import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { iniciales } from '../lib/iniciales'
import { colorPorRol } from '../lib/colorPorRol'
import { ROLES_ASIGNABLES } from '../types/personal'
import type { RolUsuario, Usuario } from '../types/usuario'
import './EquipoPage.css'

const FILTRO_TODOS = 'Todos'

export function EquipoPage() {
  const navigate = useNavigate()
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filtroRol, setFiltroRol] = useState<RolUsuario | typeof FILTRO_TODOS>(FILTRO_TODOS)

  useEffect(() => {
    let cancelado = false

    api
      .get<Usuario[]>('/api/personal')
      .then((datos) => {
        if (!cancelado) setPersonal(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el personal.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const personalFiltrado =
    filtroRol === FILTRO_TODOS ? personal : personal.filter((usuario) => usuario.rol === filtroRol)

  return (
    <DashboardLayout titulo="Personal">
      <div className="equipo-tabs" role="tablist">
        <button
          type="button"
          role="tab"
          aria-selected={filtroRol === FILTRO_TODOS}
          className={`equipo-tab${filtroRol === FILTRO_TODOS ? ' activo' : ''}`}
          onClick={() => setFiltroRol(FILTRO_TODOS)}
        >
          Todos
        </button>
        {ROLES_ASIGNABLES.map((rolOpcion) => (
          <button
            key={rolOpcion}
            type="button"
            role="tab"
            aria-selected={filtroRol === rolOpcion}
            className={`equipo-tab${filtroRol === rolOpcion ? ' activo' : ''}`}
            onClick={() => setFiltroRol(rolOpcion)}
          >
            {rolOpcion}
          </button>
        ))}
      </div>

      {error && <p className="equipo-error">{error}</p>}

      {cargando ? (
        <p className="text-secondary">Cargando personal…</p>
      ) : personalFiltrado.length === 0 ? (
        <p className="text-secondary">No hay nadie con ese rol.</p>
      ) : (
        <div className="equipo-grilla">
          {personalFiltrado.map((usuario) => (
            <button
              type="button"
              key={usuario.id}
              className="equipo-tarjeta"
              onClick={() => navigate(`/equipo/${usuario.id}`)}
            >
              {usuario.fotoPerfilUrl ? (
                <img src={usuario.fotoPerfilUrl} alt="" className="equipo-tarjeta-avatar" />
              ) : (
                <span
                  className="equipo-tarjeta-avatar equipo-tarjeta-avatar-iniciales"
                  style={{ background: colorPorRol(usuario.rol) }}
                >
                  {iniciales(usuario.nombreCompleto)}
                </span>
              )}
              <p className="equipo-tarjeta-nombre">{usuario.nombreCompleto}</p>
              <span className="equipo-tarjeta-rol">{usuario.rol.toUpperCase()}</span>
              {!usuario.activo && <span className="equipo-tarjeta-inactivo">Inactivo</span>}
            </button>
          ))}
        </div>
      )}
    </DashboardLayout>
  )
}
