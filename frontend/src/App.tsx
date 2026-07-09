import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { RecuperarContrasenaPage } from './pages/RecuperarContrasenaPage'
import { RestablecerContrasenaPage } from './pages/RestablecerContrasenaPage'
import { DashboardPage } from './pages/DashboardPage'
import { ResumenPage } from './pages/ResumenPage'
import { ActividadPage } from './pages/ActividadPage'
import { FinanzasPage } from './pages/FinanzasPage'
import { CitasPage } from './pages/CitasPage'
import { PacienteHistorialPage } from './pages/PacienteHistorialPage'
import { PersonalPage } from './pages/PersonalPage'
import { DirectorioPage } from './pages/DirectorioPage'
import { PacientesPage } from './pages/PacientesPage'
import { InventarioPage } from './pages/InventarioPage'
import { DashboardDoctorPage } from './pages/DashboardDoctorPage'
import { EquipoPage } from './pages/EquipoPage'
import { EquipoDetallePage } from './pages/EquipoDetallePage'
import { MiPerfilPage } from './pages/MiPerfilPage'
import { HomeRedirect } from './pages/HomeRedirect'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/recuperar-contrasena" element={<RecuperarContrasenaPage />} />
      <Route path="/restablecer-contrasena" element={<RestablecerContrasenaPage />} />
      <Route path="/" element={<HomeRedirect />} />
      <Route
        path="/mi-perfil"
        element={
          <ProtectedRoute>
            <MiPerfilPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute rolesPermitidos="Admin">
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/resumen"
        element={
          <ProtectedRoute rolesPermitidos="Admin">
            <ResumenPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/personal"
        element={
          <ProtectedRoute rolesPermitidos="Lemy">
            <PersonalPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/actividad"
        element={
          <ProtectedRoute rolesPermitidos="Lemy">
            <ActividadPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/finanzas"
        element={
          <ProtectedRoute rolesPermitidos="Fondos">
            <FinanzasPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/citas"
        element={
          <ProtectedRoute rolesPermitidos="Doctor">
            <CitasPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/dashboard-doctor"
        element={
          <ProtectedRoute rolesPermitidos="Doctor">
            <DashboardDoctorPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/pacientes/:id/historial"
        element={
          <ProtectedRoute rolesPermitidos="Doctor">
            <PacienteHistorialPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/directorio"
        element={
          <ProtectedRoute rolesPermitidos={['Admin', 'Lemy']}>
            <DirectorioPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/pacientes"
        element={
          <ProtectedRoute rolesPermitidos={['Admin', 'Doctor', 'Fondos', 'Farmacia', 'Lemy']}>
            <PacientesPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/equipo"
        element={
          <ProtectedRoute rolesPermitidos="Admin">
            <EquipoPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/equipo/:id"
        element={
          <ProtectedRoute rolesPermitidos="Admin">
            <EquipoDetallePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/inventario"
        element={
          <ProtectedRoute rolesPermitidos={['Admin', 'Doctor', 'Fondos', 'Farmacia', 'Lemy']}>
            <InventarioPage />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App
