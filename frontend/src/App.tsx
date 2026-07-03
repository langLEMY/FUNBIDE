import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { ResumenPage } from './pages/ResumenPage'
import { PersonalPage } from './pages/PersonalPage'
import { DirectorioPage } from './pages/DirectorioPage'
import { PacientesPage } from './pages/PacientesPage'
import { HomeRedirect } from './pages/HomeRedirect'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<HomeRedirect />} />
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
          <ProtectedRoute rolesPermitidos={['Admin', 'Lemy']}>
            <PacientesPage />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App
