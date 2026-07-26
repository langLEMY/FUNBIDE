import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { SeguroMedico } from '../types/seguroMedico'
import './AseguradorasPage.css'

export function AseguradorasPage() {
  const [seguros, setSeguros] = useState<SeguroMedico[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [nombre, setNombre] = useState('')
  const [porcentaje, setPorcentaje] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  const [editandoId, setEditandoId] = useState<string | null>(null)
  const [nombreEdit, setNombreEdit] = useState('')
  const [porcentajeEdit, setPorcentajeEdit] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [errorFila, setErrorFila] = useState<string | null>(null)
  const [procesandoId, setProcesandoId] = useState<string | null>(null)

  const cargar = () => {
    setCargando(true)
    api
      .get<SeguroMedico[]>('/api/seguros-medicos?incluirInactivos=true')
      .then((datos) => setSeguros(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el catálogo de aseguradoras.')
      })
      .finally(() => setCargando(false))
  }

  useEffect(cargar, [])

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)

    const porcentajeNumero = Number(porcentaje)
    if (!nombre.trim()) {
      setErrorCrear('El nombre es obligatorio.')
      return
    }
    if (!porcentaje.trim() || !Number.isFinite(porcentajeNumero) || porcentajeNumero <= 0 || porcentajeNumero > 100) {
      setErrorCrear('Ingresa un porcentaje de cobertura entre 1 y 100.')
      return
    }

    setCreando(true)
    try {
      const nueva = await api.post<SeguroMedico>('/api/seguros-medicos', {
        nombre: nombre.trim(),
        porcentajeCobertura: porcentajeNumero,
      })
      setSeguros((actual) => [...actual, nueva].sort((a, b) => a.nombre.localeCompare(b.nombre)))
      setNombre('')
      setPorcentaje('')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo crear la aseguradora.')
    } finally {
      setCreando(false)
    }
  }

  const iniciarEdicion = (seguro: SeguroMedico) => {
    setEditandoId(seguro.id)
    setNombreEdit(seguro.nombre)
    setPorcentajeEdit(String(seguro.porcentajeCobertura))
    setErrorFila(null)
  }

  const guardarEdicion = async (seguroMedicoId: string) => {
    const porcentajeNumero = Number(porcentajeEdit)
    if (!nombreEdit.trim() || !Number.isFinite(porcentajeNumero) || porcentajeNumero <= 0 || porcentajeNumero > 100) {
      setErrorFila('Ingresa un nombre y un porcentaje entre 1 y 100.')
      return
    }

    setGuardando(true)
    setErrorFila(null)
    try {
      const actualizado = await api.patch<SeguroMedico>('/api/seguros-medicos', {
        seguroMedicoId,
        nombre: nombreEdit.trim(),
        porcentajeCobertura: porcentajeNumero,
      })
      setSeguros((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
      setEditandoId(null)
    } catch (err) {
      setErrorFila(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar la aseguradora.')
    } finally {
      setGuardando(false)
    }
  }

  const alternarActivo = async (seguro: SeguroMedico) => {
    setProcesandoId(seguro.id)
    try {
      const actualizado = seguro.activo
        ? await api.patch<SeguroMedico>(`/api/seguros-medicos/${seguro.id}/desactivar`)
        : await api.patch<SeguroMedico>(`/api/seguros-medicos/${seguro.id}/reactivar`)
      setSeguros((actual) => actual.map((s) => (s.id === actualizado.id ? actualizado : s)))
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo actualizar la aseguradora.')
    } finally {
      setProcesandoId(null)
    }
  }

  return (
    <DashboardLayout titulo="Aseguradoras">
      <section className="aseguradoras-crear-card">
        <h2>Agregar aseguradora (ARS)</h2>
        <form className="aseguradoras-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <input placeholder="Nombre" value={nombre} onChange={(event) => setNombre(event.target.value)} required />
          <input
            type="number"
            min={1}
            max={100}
            step="0.01"
            placeholder="% de cobertura"
            value={porcentaje}
            onChange={(event) => setPorcentaje(event.target.value)}
            required
          />
          <button type="submit" disabled={creando}>
            {creando ? 'Agregando…' : 'Agregar'}
          </button>
        </form>
        {errorCrear && <p className="aseguradoras-error">{errorCrear}</p>}
      </section>

      <section className="aseguradoras-tabla-card">
        {error && <p className="aseguradoras-error">{error}</p>}
        {errorFila && <p className="aseguradoras-error">{errorFila}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando aseguradoras…</p>
        ) : seguros.length === 0 ? (
          <p className="text-secondary">Todavía no hay aseguradoras registradas.</p>
        ) : (
          <table className="aseguradoras-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>% Cobertura</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {seguros.map((seguro) =>
                editandoId === seguro.id ? (
                  <tr key={seguro.id}>
                    <td>
                      <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={1}
                        max={100}
                        step="0.01"
                        value={porcentajeEdit}
                        onChange={(event) => setPorcentajeEdit(event.target.value)}
                      />
                    </td>
                    <td className="text-muted">{seguro.activo ? 'Activo' : 'Inactivo'}</td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => void guardarEdicion(seguro.id)} disabled={guardando}>
                        {guardando ? 'Guardando…' : 'Guardar'}
                      </button>
                      <button type="button" onClick={() => setEditandoId(null)} disabled={guardando}>
                        Cancelar
                      </button>
                    </td>
                  </tr>
                ) : (
                  <tr key={seguro.id} className={seguro.activo ? '' : 'aseguradoras-fila-inactiva'}>
                    <td>{seguro.nombre}</td>
                    <td>{seguro.porcentajeCobertura}%</td>
                    <td>
                      <span className={`aseguradoras-estado ${seguro.activo ? 'activo' : 'inactivo'}`}>
                        {seguro.activo ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td className="aseguradoras-acciones">
                      <button type="button" onClick={() => iniciarEdicion(seguro)}>
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => void alternarActivo(seguro)}
                        disabled={procesandoId === seguro.id}
                      >
                        {seguro.activo ? 'Desactivar' : 'Reactivar'}
                      </button>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
