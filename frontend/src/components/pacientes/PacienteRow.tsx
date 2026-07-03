import { useRef, useState, type ChangeEvent } from 'react'
import { api, ApiError } from '../../lib/api'
import type { Paciente, UrlFotoCedula } from '../../types/paciente'

interface PacienteRowProps {
  paciente: Paciente
  puedeAdministrar: boolean
  onActualizado: (paciente: Paciente) => void
  onEliminado: (pacienteId: string) => void
}

export function PacienteRow({ paciente, puedeAdministrar, onActualizado, onEliminado }: PacienteRowProps) {
  const [editando, setEditando] = useState(false)
  const [nombreEdit, setNombreEdit] = useState(paciente.nombre)
  const [apellidoEdit, setApellidoEdit] = useState(paciente.apellido)
  const [cedulaEdit, setCedulaEdit] = useState(paciente.cedula)
  const [telefonoEdit, setTelefonoEdit] = useState(paciente.telefono ?? '')
  const [guardando, setGuardando] = useState(false)

  const [subiendoFoto, setSubiendoFoto] = useState(false)
  const [viendoFoto, setViendoFoto] = useState(false)
  const [eliminando, setEliminando] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const inputArchivoRef = useRef<HTMLInputElement>(null)

  const guardarEdicion = async () => {
    setError(null)
    setGuardando(true)
    try {
      const actualizado = await api.patch<Paciente>('/api/pacientes', {
        pacienteId: paciente.id,
        nombre: nombreEdit,
        apellido: apellidoEdit,
        cedula: cedulaEdit,
        telefono: telefonoEdit.trim() || null,
      })
      onActualizado(actualizado)
      setEditando(false)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar el cambio.')
    } finally {
      setGuardando(false)
    }
  }

  const cancelarEdicion = () => {
    setNombreEdit(paciente.nombre)
    setApellidoEdit(paciente.apellido)
    setCedulaEdit(paciente.cedula)
    setTelefonoEdit(paciente.telefono ?? '')
    setEditando(false)
    setError(null)
  }

  const handleEliminar = async () => {
    if (!window.confirm(`¿Eliminar a ${paciente.nombre} ${paciente.apellido} de la base de datos de pacientes?`)) {
      return
    }

    setError(null)
    setEliminando(true)
    try {
      await api.delete(`/api/pacientes/${paciente.id}`)
      onEliminado(paciente.id)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo eliminar al paciente.')
      setEliminando(false)
    }
  }

  const handleVerFoto = async () => {
    setError(null)
    setViendoFoto(true)
    try {
      const { url } = await api.get<UrlFotoCedula>(`/api/pacientes/${paciente.id}/foto-cedula`)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo abrir la foto de la cédula.')
    } finally {
      setViendoFoto(false)
    }
  }

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
      const actualizado = await api.postForm<Paciente>(`/api/pacientes/${paciente.id}/foto-cedula`, form)
      onActualizado(actualizado)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo subir la foto de la cédula.')
    } finally {
      setSubiendoFoto(false)
    }
  }

  if (editando) {
    return (
      <tr>
        <td>
          <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} placeholder="Nombre" />
        </td>
        <td>
          <input
            value={apellidoEdit}
            onChange={(event) => setApellidoEdit(event.target.value)}
            placeholder="Apellido"
          />
        </td>
        <td>
          <input value={cedulaEdit} onChange={(event) => setCedulaEdit(event.target.value)} placeholder="Cédula" />
        </td>
        <td>
          <input
            value={telefonoEdit}
            onChange={(event) => setTelefonoEdit(event.target.value)}
            placeholder="Teléfono (opcional)"
          />
        </td>
        <td className="text-muted">—</td>
        <td className="pacientes-acciones">
          <button type="button" onClick={() => void guardarEdicion()} disabled={guardando}>
            {guardando ? 'Guardando…' : 'Guardar'}
          </button>
          <button type="button" onClick={cancelarEdicion} disabled={guardando}>
            Cancelar
          </button>
          {error && <p className="pacientes-row-error">{error}</p>}
        </td>
      </tr>
    )
  }

  return (
    <>
      <tr>
        <td>{paciente.nombre}</td>
        <td>{paciente.apellido}</td>
        <td>{paciente.cedula}</td>
        <td className="text-muted">{paciente.telefono ?? '—'}</td>
        <td>
          {paciente.tieneFotoCedula ? (
            <button type="button" onClick={() => void handleVerFoto()} disabled={viendoFoto}>
              {viendoFoto ? 'Abriendo…' : 'Ver foto'}
            </button>
          ) : (
            <span className="text-muted">Sin foto</span>
          )}
        </td>
        <td className="pacientes-acciones">
          {puedeAdministrar && (
            <>
              <button type="button" onClick={() => setEditando(true)}>
                Editar
              </button>
              <button type="button" onClick={() => inputArchivoRef.current?.click()} disabled={subiendoFoto}>
                {subiendoFoto ? 'Subiendo…' : paciente.tieneFotoCedula ? 'Reemplazar foto' : 'Subir foto'}
              </button>
              <button
                type="button"
                className="pacientes-boton-peligro"
                onClick={() => void handleEliminar()}
                disabled={eliminando}
              >
                {eliminando ? 'Eliminando…' : 'Eliminar'}
              </button>
              <input
                ref={inputArchivoRef}
                type="file"
                accept="image/*"
                hidden
                onChange={handleArchivoSeleccionado}
              />
            </>
          )}
        </td>
      </tr>
      {error && (
        <tr>
          <td colSpan={6} className="pacientes-row-error">
            {error}
          </td>
        </tr>
      )}
    </>
  )
}
