import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

export type Tema = 'oscuro' | 'claro'

const CLAVE_ALMACENAMIENTO = 'funbide-tema'

interface ThemeContextValue {
  tema: Tema
  alternarTema: () => void
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined)

function leerTemaGuardado(): Tema {
  if (typeof window === 'undefined') {
    return 'oscuro'
  }
  return window.localStorage.getItem(CLAVE_ALMACENAMIENTO) === 'claro' ? 'claro' : 'oscuro'
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [tema, setTema] = useState<Tema>(leerTemaGuardado)

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', tema)
    window.localStorage.setItem(CLAVE_ALMACENAMIENTO, tema)
  }, [tema])

  const alternarTema = () => setTema((actual) => (actual === 'oscuro' ? 'claro' : 'oscuro'))

  return <ThemeContext.Provider value={{ tema, alternarTema }}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const context = useContext(ThemeContext)
  if (!context) {
    throw new Error('useTheme debe usarse dentro de <ThemeProvider>')
  }
  return context
}
