import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PersonalRow } from '../components/personal/PersonalRow'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Usuario, RolUsuario } from '../types/usuario'
import { rolesAsignablesPara } from '../types/personal'
import './PersonalPage.css'

export function PersonalPage() {
  const { perfil } = useAuth()
  const rolesAsignables = useMemo(() => rolesAsignablesPara(perfil?.rol), [perfil?.rol])
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [nombreCompleto, setNombreCompleto] = useState('')
  const [correo, setCorreo] = useState('')
  const [contrasenaTemporal, setContrasenaTemporal] = useState('')
  const [rol, setRol] = useState<RolUsuario>('Doctor')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

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

  const actualizarEnLista = (usuario: Usuario) => {
    setPersonal((actual) => actual.map((u) => (u.id === usuario.id ? usuario : u)))
  }

  const quitarDeLista = (usuarioId: string) => {
    setPersonal((actual) => actual.filter((u) => u.id !== usuarioId))
  }

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)
    setCreando(true)

    try {
      const nuevo = await api.post<Usuario>('/api/personal', {
        nombreCompleto,
        correo,
        contrasenaTemporal,
        rol,
      })
      setPersonal((actual) => [...actual, nuevo])
      setNombreCompleto('')
      setCorreo('')
      setContrasenaTemporal('')
      setRol('Doctor')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo crear el perfil.')
    } finally {
      setCreando(false)
    }
  }

  return (
    <DashboardLayout titulo="Personal">
      <section className="personal-crear-card">
        <h2>Nuevo perfil</h2>
        <form className="personal-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <input
            placeholder="Nombre completo"
            value={nombreCompleto}
            onChange={(event) => setNombreCompleto(event.target.value)}
            required
          />
          <input
            type="email"
            placeholder="Correo"
            value={correo}
            onChange={(event) => setCorreo(event.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Contraseña temporal"
            value={contrasenaTemporal}
            onChange={(event) => setContrasenaTemporal(event.target.value)}
            minLength={8}
            required
          />
          <select value={rol} onChange={(event) => setRol(event.target.value as RolUsuario)}>
            {rolesAsignables.map((opcion) => (
              <option key={opcion} value={opcion}>
                {opcion}
              </option>
            ))}
          </select>
          <button type="submit" disabled={creando}>
            {creando ? 'Creando…' : 'Crear'}
          </button>
        </form>
        {errorCrear && <p className="personal-crear-error">{errorCrear}</p>}
      </section>

      <section className="personal-tabla-card">
        {error && <p className="personal-crear-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando personal…</p>
        ) : personal.length === 0 ? (
          <p className="text-secondary">Todavía no hay perfiles creados.</p>
        ) : (
          <table className="personal-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {personal.map((usuario) => (
                <PersonalRow
                  key={usuario.id}
                  usuario={usuario}
                  onActualizado={actualizarEnLista}
                  onEliminadoPermanentemente={quitarDeLista}
                />
              ))}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
