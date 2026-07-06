import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { LoginPage } from './LoginPage'

const iniciarSesionMock = vi.fn()

vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({ iniciarSesion: iniciarSesionMock }),
}))

vi.mock('../theme/ThemeContext', () => ({
  useTheme: () => ({ tema: 'oscuro', alternarTema: vi.fn() }),
}))

function renderLoginPage() {
  render(
    <MemoryRouter>
      <LoginPage />
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  it('renderiza el formulario de inicio de sesión', () => {
    renderLoginPage()

    expect(screen.getByRole('heading', { name: 'Bienvenido de nuevo' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Iniciar sesión' })).toBeInTheDocument()
  })

  it('muestra un mensaje traducido cuando las credenciales son inválidas', async () => {
    iniciarSesionMock.mockRejectedValueOnce(new Error('Invalid login credentials'))
    const usuario = userEvent.setup()
    renderLoginPage()

    await usuario.type(screen.getByPlaceholderText('correo@funbide.org'), 'doctor@funbide.org')
    await usuario.type(screen.getByPlaceholderText('Ingresa tu contraseña'), 'clave-incorrecta')
    await usuario.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

    expect(await screen.findByText('Correo o contraseña incorrectos.')).toBeInTheDocument()
  })
})
