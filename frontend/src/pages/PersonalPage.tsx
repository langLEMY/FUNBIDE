import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { PersonalRow } from '../components/personal/PersonalRow'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import type { Usuario, RolUsuario, EspecialidadMedica } from '../types/usuario'
import { rolesAsignablesPara, ROLES_ASIGNABLES, ESPECIALIDADES, ETIQUETA_ESPECIALIDAD } from '../types/personal'
import './PersonalPage.css'

const FILTRO_TODOS = 'Todos' as const

export function PersonalPage() {
  const { perfil } = useAuth()
  const rolesAsignables = useMemo(() => rolesAsignablesPara(perfil?.rol), [perfil?.rol])
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [busqueda, setBusqueda] = useState('')
  const [filtroRol, setFiltroRol] = useState<RolUsuario | typeof FILTRO_TODOS>(FILTRO_TODOS)

  const [nombreCompleto, setNombreCompleto] = useState('')
  const [correo, setCorreo] = useState('')
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [contrasenaTemporal, setContrasenaTemporal] = useState('')
  const [rol, setRol] = useState<RolUsuario>('Doctor')
  const [especialidad, setEspecialidad] = useState<EspecialidadMedica>('Pediatria')
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

  const personalFiltrado = useMemo(() => {
    const q = busqueda.trim().toLowerCase()
    return personal
      .filter((u) => filtroRol === FILTRO_TODOS || u.rol === filtroRol)
      .filter((u) => !q || u.nombreCompleto.toLowerCase().includes(q) || u.correo.toLowerCase().includes(q))
  }, [personal, busqueda, filtroRol])

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
        nombreUsuario,
        contrasenaTemporal,
        rol,
        especialidad: rol === 'Doctor' ? especialidad : null,
      })
      setPersonal((actual) => [...actual, nuevo])
      setNombreCompleto('')
      setCorreo('')
      setNombreUsuario('')
      setContrasenaTemporal('')
      setRol('Doctor')
      setEspecialidad('Pediatria')
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
            placeholder="Nombre de usuario (para iniciar sesión)"
            value={nombreUsuario}
            onChange={(event) => setNombreUsuario(event.target.value)}
            minLength={3}
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
          {rol === 'Doctor' && (
            <select value={especialidad} onChange={(event) => setEspecialidad(event.target.value as EspecialidadMedica)}>
              {ESPECIALIDADES.map((opcion) => (
                <option key={opcion} value={opcion}>
                  {ETIQUETA_ESPECIALIDAD[opcion]}
                </option>
              ))}
            </select>
          )}
          <button type="submit" disabled={creando}>
            {creando ? 'Creando…' : 'Crear'}
          </button>
        </form>
        {errorCrear && <p className="personal-crear-error">{errorCrear}</p>}
      </section>

      {personal.length > 0 && (
        <div className="personal-filtros">
          <div className="personal-tabs">
            {[FILTRO_TODOS, ...ROLES_ASIGNABLES].map((opcion) => (
              <button
                key={opcion}
                type="button"
                className={filtroRol === opcion ? 'personal-tab activo' : 'personal-tab'}
                onClick={() => setFiltroRol(opcion)}
              >
                {opcion}
              </button>
            ))}
          </div>
          <input
            type="search"
            className="personal-buscador"
            placeholder="Buscar por nombre o correo…"
            value={busqueda}
            onChange={(event) => setBusqueda(event.target.value)}
          />
        </div>
      )}

      <section className="personal-tabla-card">
        {error && <p className="personal-crear-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando personal…</p>
        ) : personal.length === 0 ? (
          <p className="text-secondary">Todavía no hay perfiles creados.</p>
        ) : personalFiltrado.length === 0 ? (
          <p className="text-secondary">Nadie coincide con este filtro.</p>
        ) : (
          <table className="personal-tabla">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Rol</th>
                <th>Especialidad</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {personalFiltrado.map((usuario) => (
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
