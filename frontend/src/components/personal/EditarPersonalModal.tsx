import { useMemo, useState, type FormEvent } from 'react'
import { useAuth } from '../../auth/AuthContext'
import { api, ApiError } from '../../lib/api'
import type { Usuario, RolUsuario, EspecialidadMedica } from '../../types/usuario'
import { rolesAsignablesPara, ESPECIALIDADES, ETIQUETA_ESPECIALIDAD } from '../../types/personal'
import { Modal } from '../ui/Modal'
import { Boton } from '../ui/Boton'
import './EditarPersonalModal.css'

interface EditarPersonalModalProps {
  usuario: Usuario
  onCerrar: () => void
  onGuardado: (usuario: Usuario) => void
}

export function EditarPersonalModal({ usuario, onCerrar, onGuardado }: EditarPersonalModalProps) {
  const { perfil } = useAuth()
  const rolesAsignables = useMemo(() => rolesAsignablesPara(perfil?.rol), [perfil?.rol])
  const [nombreCompleto, setNombreCompleto] = useState(usuario.nombreCompleto)
  const [correo, setCorreo] = useState(usuario.correo)
  const [nombreUsuario, setNombreUsuario] = useState(usuario.nombreUsuario)
  const [rol, setRol] = useState<RolUsuario>(usuario.rol)
  // null cuando el doctor todavia no tiene especialidad asignada: por eso NO se
  // usa un default como 'Pediatria' aca — con ese default, guardar cualquier otro
  // campo (nombre, correo, contrasena) mandaba especialidad !== null y pisaba
  // silenciosamente la especialidad real del doctor con el valor por defecto.
  const [especialidad, setEspecialidad] = useState<EspecialidadMedica | null>(usuario.especialidad)
  const [exequatur, setExequatur] = useState(usuario.exequatur ?? '')
  const [nuevaContrasena, setNuevaContrasena] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)
    setGuardando(true)

    try {
      let actual = usuario

      if (
        nombreCompleto !== usuario.nombreCompleto ||
        correo !== usuario.correo ||
        nombreUsuario !== usuario.nombreUsuario
      ) {
        actual = await api.patch<Usuario>('/api/personal/datos', {
          usuarioId: usuario.id,
          nombreCompleto,
          correo,
          nombreUsuario,
        })
      }

      if (rol !== usuario.rol) {
        actual = await api.patch<Usuario>('/api/personal/rol', { usuarioId: usuario.id, nuevoRol: rol })
      }

      if (rol === 'Doctor' && especialidad !== null && especialidad !== actual.especialidad) {
        actual = await api.patch<Usuario>('/api/personal/especialidad', { usuarioId: usuario.id, especialidad })
      }

      if (rol === 'Doctor' && exequatur.trim() !== (actual.exequatur ?? '')) {
        actual = await api.patch<Usuario>('/api/personal/exequatur', {
          usuarioId: usuario.id,
          exequatur: exequatur.trim() || null,
        })
      }

      if (nuevaContrasena.trim().length > 0) {
        actual = await api.patch<Usuario>('/api/personal/contrasena', {
          usuarioId: usuario.id,
          nuevaContrasena: nuevaContrasena.trim(),
        })
      }

      onGuardado(actual)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <Modal abierto onCerrar={onCerrar} titulo="Editar perfil" ancho={340}>
      <form className="editar-personal-formulario" onSubmit={(event) => void handleSubmit(event)}>
        <label htmlFor="ep-nombre">Nombre completo</label>
        <input
          id="ep-nombre"
          value={nombreCompleto}
          onChange={(event) => setNombreCompleto(event.target.value)}
          required
        />

        <label htmlFor="ep-correo">Correo</label>
        <input
          id="ep-correo"
          type="email"
          value={correo}
          onChange={(event) => setCorreo(event.target.value)}
          required
        />

        <label htmlFor="ep-usuario">Nombre de usuario</label>
        <input
          id="ep-usuario"
          value={nombreUsuario}
          onChange={(event) => setNombreUsuario(event.target.value)}
          minLength={3}
          required
        />

        <label htmlFor="ep-rol">Rol</label>
        <select id="ep-rol" value={rol} onChange={(event) => setRol(event.target.value as RolUsuario)}>
          {rolesAsignables.map((opcion) => (
            <option key={opcion} value={opcion}>
              {opcion}
            </option>
          ))}
        </select>

        {rol === 'Doctor' && (
          <>
            <label htmlFor="ep-especialidad">Especialidad</label>
            <select
              id="ep-especialidad"
              value={especialidad ?? ''}
              onChange={(event) => setEspecialidad((event.target.value || null) as EspecialidadMedica | null)}
            >
              <option value="">Sin asignar</option>
              {ESPECIALIDADES.map((opcion) => (
                <option key={opcion} value={opcion}>
                  {ETIQUETA_ESPECIALIDAD[opcion]}
                </option>
              ))}
            </select>

            <label htmlFor="ep-exequatur">Exequátur (colegiatura médica)</label>
            <input
              id="ep-exequatur"
              placeholder="Ej. 12345-67"
              value={exequatur}
              onChange={(event) => setExequatur(event.target.value)}
            />
          </>
        )}

        <label htmlFor="ep-contrasena">Nueva contraseña (opcional)</label>
        <input
          id="ep-contrasena"
          type="password"
          placeholder="Dejar en blanco para no cambiarla"
          value={nuevaContrasena}
          onChange={(event) => setNuevaContrasena(event.target.value)}
          minLength={8}
        />

        {error && <p className="editar-personal-error">{error}</p>}

        <div className="editar-personal-acciones">
          <Boton type="button" variante="secundario" onClick={onCerrar} disabled={guardando}>
            Cancelar
          </Boton>
          <Boton type="submit" variante="primario" cargando={guardando}>
            Guardar
          </Boton>
        </div>
      </form>
    </Modal>
  )
}
