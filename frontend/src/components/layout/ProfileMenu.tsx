import { useRef, useState, type ChangeEvent } from 'react'
import { useAuth } from '../../auth/AuthContext'
import { api, ApiError } from '../../lib/api'
import { iniciales } from '../../lib/iniciales'
import type { Usuario } from '../../types/usuario'
import './ProfileMenu.css'

export function ProfileMenu() {
  const { perfil, cerrarSesion, recargarPerfil } = useAuth()
  const [abierto, setAbierto] = useState(false)
  const [verPerfilAbierto, setVerPerfilAbierto] = useState(false)
  const [subiendo, setSubiendo] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const inputArchivoRef = useRef<HTMLInputElement>(null)

  if (!perfil) {
    return null
  }

  const handleCambiarFoto = () => {
    setAbierto(false)
    inputArchivoRef.current?.click()
  }

  const handleArchivoSeleccionado = async (event: ChangeEvent<HTMLInputElement>) => {
    const archivo = event.target.files?.[0]
    event.target.value = ''
    if (!archivo) {
      return
    }

    setError(null)
    setSubiendo(true)
    try {
      const form = new FormData()
      form.append('archivo', archivo)
      await api.postForm<Usuario>('/api/mi-perfil/foto', form)
      await recargarPerfil()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo actualizar la foto.')
    } finally {
      setSubiendo(false)
    }
  }

  return (
    <div className="profile-menu">
      <button type="button" className="profile-menu-trigger" onClick={() => setAbierto((valor) => !valor)}>
        {perfil.fotoPerfilUrl ? (
          <img src={perfil.fotoPerfilUrl} alt="" className="profile-menu-avatar" />
        ) : (
          <span className="profile-menu-avatar profile-menu-avatar-iniciales">{iniciales(perfil.nombreCompleto)}</span>
        )}
      </button>

      {abierto && (
        <>
          <button
            type="button"
            aria-label="Cerrar menú"
            className="profile-menu-overlay"
            onClick={() => setAbierto(false)}
          />
          <div className="profile-menu-dropdown">
            <div className="profile-menu-header">
              <p className="profile-menu-nombre">{perfil.nombreCompleto}</p>
              <p className="profile-menu-rol text-muted">{perfil.rol}</p>
            </div>

            <button
              type="button"
              className="profile-menu-item"
              onClick={() => {
                setAbierto(false)
                setVerPerfilAbierto(true)
              }}
            >
              Ver perfil
            </button>
            <button type="button" className="profile-menu-item" onClick={handleCambiarFoto} disabled={subiendo}>
              {subiendo ? 'Subiendo…' : 'Cambiar foto de perfil'}
            </button>
            <button
              type="button"
              className="profile-menu-item profile-menu-item-peligro"
              onClick={() => void cerrarSesion()}
            >
              Cerrar sesión
            </button>
          </div>
        </>
      )}

      <input ref={inputArchivoRef} type="file" accept="image/*" hidden onChange={handleArchivoSeleccionado} />

      {error && <p className="profile-menu-error">{error}</p>}

      {verPerfilAbierto && (
        <div className="profile-modal-overlay" onClick={() => setVerPerfilAbierto(false)}>
          <div className="profile-modal" onClick={(event) => event.stopPropagation()}>
            <h2>Mi perfil</h2>
            <dl>
              <dt className="text-muted">Nombre</dt>
              <dd>{perfil.nombreCompleto}</dd>
              <dt className="text-muted">Correo</dt>
              <dd>{perfil.correo}</dd>
              <dt className="text-muted">Rol</dt>
              <dd>{perfil.rol}</dd>
            </dl>
            <button type="button" className="profile-modal-cerrar" onClick={() => setVerPerfilAbierto(false)}>
              Cerrar
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
