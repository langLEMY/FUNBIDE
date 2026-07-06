import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { PacienteRow } from './PacienteRow'
import type { Paciente } from '../../types/paciente'

const paciente: Paciente = {
  id: 'paciente-1',
  nombre: 'Ana',
  apellido: 'Gómez',
  cedula: '00112345678',
  telefono: null,
  tieneFotoCedula: false,
  edad: 34,
  condicion: 'Hipertensión',
  estado: 'Activo',
  ultimaVisita: null,
}

function renderFila(props: Partial<React.ComponentProps<typeof PacienteRow>> = {}) {
  render(
    <MemoryRouter>
      <table>
        <tbody>
          <PacienteRow
            paciente={paciente}
            puedeEditar={false}
            puedeEliminar={false}
            puedeVerHistorial={false}
            onActualizado={vi.fn()}
            onEliminado={vi.fn()}
            {...props}
          />
        </tbody>
      </table>
    </MemoryRouter>,
  )
}

describe('PacienteRow', () => {
  it('no muestra ninguna acción cuando el rol no tiene permisos', () => {
    renderFila()

    expect(screen.queryByRole('button', { name: 'Editar' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Eliminar' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Historial' })).not.toBeInTheDocument()
  })

  it('Lemy ve Editar y Eliminar pero no Historial', () => {
    renderFila({ puedeEditar: true, puedeEliminar: true })

    expect(screen.getByRole('button', { name: 'Editar' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Eliminar' })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Historial' })).not.toBeInTheDocument()
  })

  it('Doctor ve Eliminar e Historial pero no Editar', () => {
    renderFila({ puedeEliminar: true, puedeVerHistorial: true })

    expect(screen.queryByRole('button', { name: 'Editar' })).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Eliminar' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Historial' })).toHaveAttribute(
      'href',
      '/pacientes/paciente-1/historial',
    )
  })
})
