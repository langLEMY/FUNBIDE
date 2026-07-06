import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, ApiError } from '../../lib/api'
import type { EstadoPaciente, Paciente } from '../../types/paciente'
import { ESTADOS_PACIENTE } from '../../types/paciente'

interface PacienteRowProps {
  paciente: Paciente
  puedeEditar: boolean
  puedeEliminar: boolean
  puedeVerHistorial: boolean
  onActualizado: (paciente: Paciente) => void
  onEliminado: (pacienteId: string) => void
}

const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' })

function formatearUltimaVisita(valor: string | null): string {
  if (!valor) return '—'
  return formateadorFecha.format(new Date(`${valor}T00:00:00`))
}

function claseBadgeEstado(estado: EstadoPaciente): string {
  switch (estado) {
    case 'Activo':
      return 'pacientes-badge pacientes-badge-activo'
    case 'Seguimiento':
      return 'pacientes-badge pacientes-badge-seguimiento'
  }
}

export function PacienteRow({
  paciente,
  puedeEditar,
  puedeEliminar,
  puedeVerHistorial,
  onActualizado,
  onEliminado,
}: PacienteRowProps) {
  const [editando, setEditando] = useState(false)
  const [nombreEdit, setNombreEdit] = useState(paciente.nombre)
  const [apellidoEdit, setApellidoEdit] = useState(paciente.apellido)
  const [cedulaEdit, setCedulaEdit] = useState(paciente.cedula)
  const [telefonoEdit, setTelefonoEdit] = useState(paciente.telefono ?? '')
  const [edadEdit, setEdadEdit] = useState(paciente.edad?.toString() ?? '')
  const [condicionEdit, setCondicionEdit] = useState(paciente.condicion ?? '')
  const [estadoEdit, setEstadoEdit] = useState<EstadoPaciente>(paciente.estado)
  const [guardando, setGuardando] = useState(false)

  const [eliminando, setEliminando] = useState(false)
  const [error, setError] = useState<string | null>(null)

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
        edad: edadEdit.trim() ? Number(edadEdit) : null,
        condicion: condicionEdit.trim() || null,
        estado: estadoEdit,
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
    setEdadEdit(paciente.edad?.toString() ?? '')
    setCondicionEdit(paciente.condicion ?? '')
    setEstadoEdit(paciente.estado)
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

  const columnas = puedeEditar || puedeEliminar || puedeVerHistorial ? 8 : 7

  if (editando) {
    return (
      <tr>
        <td className="pacientes-celda-doble">
          <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} placeholder="Nombre" />
          <input
            value={apellidoEdit}
            onChange={(event) => setApellidoEdit(event.target.value)}
            placeholder="Apellido"
          />
        </td>
        <td>
          <input
            type="number"
            value={edadEdit}
            onChange={(event) => setEdadEdit(event.target.value)}
            placeholder="Edad"
            min={0}
          />
        </td>
        <td>
          <input
            value={condicionEdit}
            onChange={(event) => setCondicionEdit(event.target.value)}
            placeholder="Condición"
          />
        </td>
        <td className="text-muted">{formatearUltimaVisita(paciente.ultimaVisita)}</td>
        <td>
          <select value={estadoEdit} onChange={(event) => setEstadoEdit(event.target.value as EstadoPaciente)}>
            {ESTADOS_PACIENTE.map((estado) => (
              <option key={estado} value={estado}>
                {estado}
              </option>
            ))}
          </select>
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
        <td>{`${paciente.nombre} ${paciente.apellido}`}</td>
        <td className="text-muted">{paciente.edad ?? '—'}</td>
        <td className="text-muted">{paciente.condicion ?? '—'}</td>
        <td className="text-muted">{formatearUltimaVisita(paciente.ultimaVisita)}</td>
        <td>
          <span className={claseBadgeEstado(paciente.estado)}>{paciente.estado}</span>
        </td>
        <td>{paciente.cedula}</td>
        <td className="text-muted">{paciente.telefono ?? '—'}</td>
        {(puedeEditar || puedeEliminar || puedeVerHistorial) && (
          <td className="pacientes-acciones">
            {puedeVerHistorial && <Link to={`/pacientes/${paciente.id}/historial`}>Historial</Link>}
            {puedeEditar && (
              <button type="button" onClick={() => setEditando(true)}>
                Editar
              </button>
            )}
            {puedeEliminar && (
              <button
                type="button"
                className="pacientes-boton-peligro"
                onClick={() => void handleEliminar()}
                disabled={eliminando}
              >
                {eliminando ? 'Eliminando…' : 'Eliminar'}
              </button>
            )}
          </td>
        )}
      </tr>
      {error && (
        <tr>
          <td colSpan={columnas} className="pacientes-row-error">
            {error}
          </td>
        </tr>
      )}
    </>
  )
}
