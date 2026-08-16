import { useEffect, useMemo, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { EditarPermisosModal } from '../components/permisos/EditarPermisosModal'
import { iniciales } from '../lib/iniciales'
import { api, ApiError } from '../lib/api'
import type { Usuario } from '../types/usuario'
import './PermisosPage.css'

export function PermisosPage() {
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busqueda, setBusqueda] = useState('')
  const [usuarioSeleccionado, setUsuarioSeleccionado] = useState<Usuario | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<Usuario[]>('/api/personal')
      .then((datos) => {
        if (!cancelado) {
          setPersonal(datos)
        }
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el personal.')
        }
      })
      .finally(() => {
        if (!cancelado) {
          setCargando(false)
        }
      })

    return () => {
      cancelado = true
    }
  }, [])

  const personalFiltrado = useMemo(() => {
    const q = busqueda.trim().toLowerCase()
    if (!q) return personal
    return personal.filter(
      (u) => u.nombreCompleto.toLowerCase().includes(q) || u.correo.toLowerCase().includes(q),
    )
  }, [personal, busqueda])

  return (
    <DashboardLayout titulo="Gestionar Permisos">
      <section className="permisos-intro-card">
        <p>
          Elegí a quién concederle o revocarle acceso a módulos específicos del sistema, más allá de lo que su
          rol permite por defecto.
        </p>
      </section>

      {personal.length > 0 && (
        <div className="permisos-filtros">
          <input
            type="search"
            className="permisos-buscador"
            placeholder="Buscar por nombre o correo…"
            value={busqueda}
            onChange={(event) => setBusqueda(event.target.value)}
          />
        </div>
      )}

      <section className="permisos-tabla-card">
        {error && <p className="permisos-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando personal…</p>
        ) : personal.length === 0 ? (
          <p className="text-secondary">Todavía no hay perfiles creados.</p>
        ) : personalFiltrado.length === 0 ? (
          <p className="text-secondary">Nadie coincide con esta búsqueda.</p>
        ) : (
          <table className="permisos-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Rol</th>
                <th>Estado</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {personalFiltrado.map((usuario) => (
                <tr key={usuario.id} className={usuario.activo ? '' : 'permisos-row-inactivo'}>
                  <td>
                    <div className="permisos-row-identidad">
                      {usuario.fotoPerfilUrl ? (
                        <img src={usuario.fotoPerfilUrl} alt="" className="permisos-row-avatar" />
                      ) : (
                        <span className="permisos-row-avatar permisos-row-avatar-iniciales">
                          {iniciales(usuario.nombreCompleto)}
                        </span>
                      )}
                      <div>
                        <p className="permisos-row-nombre">{usuario.nombreCompleto}</p>
                        <p className="permisos-row-correo text-muted">{usuario.correo}</p>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="permisos-row-rol">{usuario.rol}</span>
                  </td>
                  <td>
                    <span className={`permisos-row-estado ${usuario.activo ? 'activo' : 'inactivo'}`}>
                      {usuario.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="permisos-row-acciones">
                    <button type="button" onClick={() => setUsuarioSeleccionado(usuario)}>
                      Gestionar permisos
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      {usuarioSeleccionado && (
        <EditarPermisosModal usuario={usuarioSeleccionado} onCerrar={() => setUsuarioSeleccionado(null)} />
      )}
    </DashboardLayout>
  )
}
