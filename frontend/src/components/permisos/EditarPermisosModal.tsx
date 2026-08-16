import { useEffect, useState } from 'react'
import { api, ApiError } from '../../lib/api'
import type { ModuloPermiso, Usuario } from '../../types/usuario'
import { ETIQUETA_MODULO, GRUPOS_MODULOS } from '../../types/permisos'
import type { ModuloPermisoEstado, PermisoDeseado, PermisosUsuario } from '../../types/permisos'
import './EditarPermisosModal.css'

interface EditarPermisosModalProps {
  usuario: Usuario
  onCerrar: () => void
}

export function EditarPermisosModal({ usuario, onCerrar }: EditarPermisosModalProps) {
  const [modulos, setModulos] = useState<ModuloPermisoEstado[] | null>(null)
  const [estado, setEstado] = useState<Partial<Record<ModuloPermiso, boolean>>>({})
  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<PermisosUsuario>(`/api/permisos/${usuario.id}`)
      .then((datos) => {
        if (cancelado) return
        setModulos(datos.modulos)
        setEstado(Object.fromEntries(datos.modulos.map((m) => [m.modulo, m.concedido])))
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudieron cargar los permisos.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [usuario.id])

  const handleGuardar = async () => {
    if (!modulos) return
    setError(null)
    setGuardando(true)

    try {
      const permisos: PermisoDeseado[] = modulos.map((m) => ({
        modulo: m.modulo,
        concedido: estado[m.modulo] ?? m.concedido,
      }))
      await api.put(`/api/permisos`, { usuarioId: usuario.id, permisos })
      onCerrar()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className="editar-permisos-overlay" onClick={onCerrar}>
      <div className="editar-permisos-modal" onClick={(event) => event.stopPropagation()}>
        <h2>Permisos de {usuario.nombreCompleto}</h2>
        <p className="editar-permisos-subtitulo">
          Rol: {usuario.rol} — los módulos marcados como "Personalizado" ya no siguen el default de su rol.
        </p>

        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando permisos…</p>
        ) : modulos ? (
          <div className="editar-permisos-grupos">
            {GRUPOS_MODULOS.map(({ grupo, modulos: modulosDelGrupo }) => {
              const items = modulos.filter((m) => modulosDelGrupo.includes(m.modulo))
              if (items.length === 0) {
                return null
              }

              return (
                <div key={grupo} className="editar-permisos-grupo">
                  <h3>{grupo}</h3>
                  {items.map((item) => {
                    const concedido = estado[item.modulo] ?? item.concedido
                    const personalizado = concedido !== item.defaultDelRol

                    return (
                      <div key={item.modulo} className="editar-permisos-fila">
                        <div className="editar-permisos-fila-texto">
                          <span>{ETIQUETA_MODULO[item.modulo]}</span>
                          {personalizado && <span className="editar-permisos-badge">Personalizado</span>}
                        </div>
                        <button
                          type="button"
                          role="switch"
                          aria-checked={concedido}
                          className={`editar-permisos-interruptor ${concedido ? 'activo' : ''}`}
                          onClick={() => setEstado((actual) => ({ ...actual, [item.modulo]: !concedido }))}
                          disabled={guardando}
                        >
                          <span className="editar-permisos-interruptor-perilla" />
                        </button>
                      </div>
                    )
                  })}
                </div>
              )
            })}
          </div>
        ) : null}

        {error && <p className="editar-permisos-error">{error}</p>}

        <div className="editar-permisos-acciones">
          <button type="button" className="editar-permisos-cancelar" onClick={onCerrar} disabled={guardando}>
            Cerrar
          </button>
          <button
            type="button"
            className="editar-permisos-guardar"
            onClick={() => void handleGuardar()}
            disabled={guardando || cargando || !modulos}
          >
            {guardando ? 'Guardando…' : 'Guardar cambios'}
          </button>
        </div>
      </div>
    </div>
  )
}
