import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { iniciales } from '../lib/iniciales'
import type { Usuario } from '../types/usuario'
import './EquipoDetallePage.css'

export function EquipoDetallePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<Usuario[]>('/api/personal')
      .then((datos) => {
        if (cancelado) return
        const encontrado = datos.find((u) => u.id === id) ?? null
        setUsuario(encontrado)
        if (!encontrado) setError('No se encontró a esa persona.')
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
  }, [id])

  return (
    <DashboardLayout titulo="Personal">
      <button type="button" className="equipo-detalle-volver" onClick={() => navigate('/equipo')}>
        ← Volver
      </button>

      {cargando ? (
        <p className="text-secondary">Cargando…</p>
      ) : error ? (
        <p className="equipo-error">{error}</p>
      ) : usuario ? (
        <section className="equipo-detalle-card">
          {usuario.fotoPerfilUrl ? (
            <img src={usuario.fotoPerfilUrl} alt="" className="equipo-detalle-avatar" />
          ) : (
            <span className="equipo-detalle-avatar equipo-detalle-avatar-iniciales">
              {iniciales(usuario.nombreCompleto)}
            </span>
          )}

          <h2 className="equipo-detalle-nombre">{usuario.nombreCompleto}</h2>

          <dl className="equipo-detalle-datos">
            <dt className="text-muted">Correo</dt>
            <dd>{usuario.correo}</dd>
            <dt className="text-muted">Rol</dt>
            <dd>{usuario.rol}</dd>
            <dt className="text-muted">Estado</dt>
            <dd>{usuario.activo ? 'Activo' : 'Inactivo'}</dd>
          </dl>
        </section>
      ) : null}
    </DashboardLayout>
  )
}
