import { useRef, useState, type ChangeEvent } from 'react'
import { api, ApiError } from '../../lib/api'
import { iniciales } from '../../lib/iniciales'
import { EditarPersonalModal } from './EditarPersonalModal'
import type { Usuario } from '../../types/usuario'

interface PersonalRowProps {
  usuario: Usuario
  onActualizado: (usuario: Usuario) => void
  onEliminadoPermanentemente: (usuarioId: string) => void
}

export function PersonalRow({ usuario, onActualizado, onEliminadoPermanentemente }: PersonalRowProps) {
  const [editando, setEditando] = useState(false)
  const [subiendoFoto, setSubiendoFoto] = useState(false)
  const [procesando, setProcesando] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const inputArchivoRef = useRef<HTMLInputElement>(null)

  const handleArchivoSeleccionado = async (event: ChangeEvent<HTMLInputElement>) => {
    const archivo = event.target.files?.[0]
    event.target.value = ''
    if (!archivo) {
      return
    }

    setError(null)
    setSubiendoFoto(true)
    try {
      const form = new FormData()
      form.append('archivo', archivo)
      const actualizado = await api.postForm<Usuario>(`/api/personal/${usuario.id}/foto`, form)
      onActualizado(actualizado)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo subir la foto.')
    } finally {
      setSubiendoFoto(false)
    }
  }

  const handleEliminarOReactivar = async () => {
    const accion = usuario.activo ? 'eliminar' : 'reactivar'
    if (usuario.activo && !window.confirm(`¿Eliminar el perfil de ${usuario.nombreCompleto}?`)) {
      return
    }

    setError(null)
    setProcesando(true)
    try {
      const actualizado = usuario.activo
        ? await api.delete<Usuario>(`/api/personal/${usuario.id}`)
        : await api.patch<Usuario>(`/api/personal/${usuario.id}/reactivar`)
      onActualizado(actualizado)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : `No se pudo ${accion} el perfil.`)
    } finally {
      setProcesando(false)
    }
  }

  const handleBorrarPermanente = async () => {
    const confirmado = window.confirm(
      `Esto borra a ${usuario.nombreCompleto} de Supabase Auth de forma PERMANENTE: no se puede deshacer ` +
        '(a diferencia de "Eliminar", ya no habrá "Reactivar" posible). ¿Seguro que quieres continuar?',
    )
    if (!confirmado) {
      return
    }

    setError(null)
    setProcesando(true)
    try {
      await api.delete<Usuario>(`/api/personal/${usuario.id}/permanente`)
      // A diferencia de eliminar/reactivar (reversible), esto ya no debe seguir
      // apareciendo en la lista — no lo actualizamos en su lugar, lo quitamos.
      onEliminadoPermanentemente(usuario.id)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo borrar permanentemente.')
      setProcesando(false)
    }
  }

  return (
    <>
      <tr className={usuario.activo ? '' : 'personal-row-inactivo'}>
        <td>
          <div className="personal-row-identidad">
            {usuario.fotoPerfilUrl ? (
              <img src={usuario.fotoPerfilUrl} alt="" className="personal-row-avatar" />
            ) : (
              <span className="personal-row-avatar personal-row-avatar-iniciales">
                {iniciales(usuario.nombreCompleto)}
              </span>
            )}
            <div>
              <p className="personal-row-nombre">{usuario.nombreCompleto}</p>
              <p className="personal-row-correo text-muted">{usuario.correo}</p>
            </div>
          </div>
        </td>
        <td>
          <span className="personal-row-rol">{usuario.rol}</span>
        </td>
        <td>
          <span className={`personal-row-estado ${usuario.activo ? 'activo' : 'inactivo'}`}>
            {usuario.activo ? 'Activo' : 'Inactivo'}
          </span>
        </td>
        <td className="personal-row-acciones">
          <button type="button" onClick={() => setEditando(true)}>
            Editar
          </button>
          <button type="button" onClick={() => inputArchivoRef.current?.click()} disabled={subiendoFoto}>
            {subiendoFoto ? 'Subiendo…' : 'Foto'}
          </button>
          <button
            type="button"
            className={usuario.activo ? 'personal-row-boton-peligro' : ''}
            onClick={() => void handleEliminarOReactivar()}
            disabled={procesando}
          >
            {usuario.activo ? 'Eliminar' : 'Reactivar'}
          </button>
          <button
            type="button"
            className="personal-row-boton-peligro"
            onClick={() => void handleBorrarPermanente()}
            disabled={procesando}
            title="Borra el acceso de forma permanente e irreversible"
          >
            Borrar
          </button>
          <input ref={inputArchivoRef} type="file" accept="image/*" hidden onChange={handleArchivoSeleccionado} />
        </td>
      </tr>
      {error && (
        <tr>
          <td colSpan={4} className="personal-row-error">
            {error}
          </td>
        </tr>
      )}
      {editando && (
        <EditarPersonalModal
          usuario={usuario}
          onCerrar={() => setEditando(false)}
          onGuardado={(actualizado) => {
            onActualizado(actualizado)
            setEditando(false)
          }}
        />
      )}
    </>
  )
}
