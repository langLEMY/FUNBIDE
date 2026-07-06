import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

// Evita que las pruebas disparen peticiones de red reales (p. ej. el "mejor
// esfuerzo" de registrar el intento de login) contra un backend que no existe aquí.
vi.stubGlobal(
  'fetch',
  vi.fn(() => Promise.resolve(new Response(null, { status: 204 }))),
)
