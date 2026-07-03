import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import type { Session } from '@supabase/supabase-js'
import { supabase } from '../lib/supabaseClient'
import { api, ApiError } from '../lib/api'
import type { Usuario } from '../types/usuario'

interface AuthContextValue {
  session: Session | null
  perfil: Usuario | null
  cargando: boolean
  perfilCargando: boolean
  perfilError: string | null
  iniciarSesion: (correo: string, contrasena: string) => Promise<void>
  cerrarSesion: () => Promise<void>
  recargarPerfil: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null)
  const [perfil, setPerfil] = useState<Usuario | null>(null)
  const [cargando, setCargando] = useState(true)
  const [perfilCargando, setPerfilCargando] = useState(false)
  const [perfilError, setPerfilError] = useState<string | null>(null)

  const cargarPerfil = async () => {
    setPerfilCargando(true)
    setPerfilError(null)
    try {
      const usuario = await api.get<Usuario>('/api/mi-perfil')
      setPerfil(usuario)
    } catch (err) {
      setPerfil(null)
      setPerfilError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar tu perfil.')
    } finally {
      setPerfilCargando(false)
    }
  }

  useEffect(() => {
    supabase.auth.getSession().then(({ data }) => {
      setSession(data.session)
      setCargando(false)
    })

    const { data: listener } = supabase.auth.onAuthStateChange((_event, nuevaSesion) => {
      setSession(nuevaSesion)
    })

    return () => listener.subscription.unsubscribe()
  }, [])

  useEffect(() => {
    if (session) {
      void cargarPerfil()
    } else {
      setPerfil(null)
      setPerfilError(null)
    }
  }, [session])

  const iniciarSesion = async (correo: string, contrasena: string) => {
    const { error } = await supabase.auth.signInWithPassword({ email: correo, password: contrasena })
    if (error) {
      throw error
    }
  }

  const cerrarSesion = async () => {
    await supabase.auth.signOut()
  }

  return (
    <AuthContext.Provider
      value={{
        session,
        perfil,
        cargando,
        perfilCargando,
        perfilError,
        iniciarSesion,
        cerrarSesion,
        recargarPerfil: cargarPerfil,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth debe usarse dentro de <AuthProvider>')
  }
  return context
}
