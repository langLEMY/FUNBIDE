import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { HomeRedirect } from './pages/HomeRedirect'

const RecuperarContrasenaPage = lazy(() =>
  import('./pages/RecuperarContrasenaPage').then((m) => ({ default: m.RecuperarContrasenaPage })),
)
const RestablecerContrasenaPage = lazy(() =>
  import('./pages/RestablecerContrasenaPage').then((m) => ({ default: m.RestablecerContrasenaPage })),
)
const DashboardPage = lazy(() => import('./pages/DashboardPage').then((m) => ({ default: m.DashboardPage })))
const ResumenPage = lazy(() => import('./pages/ResumenPage').then((m) => ({ default: m.ResumenPage })))
const ActividadPage = lazy(() => import('./pages/ActividadPage').then((m) => ({ default: m.ActividadPage })))
const FinanzasPage = lazy(() => import('./pages/FinanzasPage').then((m) => ({ default: m.FinanzasPage })))
const CitasPage = lazy(() => import('./pages/CitasPage').then((m) => ({ default: m.CitasPage })))
const PacienteHistorialPage = lazy(() =>
  import('./pages/PacienteHistorialPage').then((m) => ({ default: m.PacienteHistorialPage })),
)
const PersonalPage = lazy(() => import('./pages/PersonalPage').then((m) => ({ default: m.PersonalPage })))
const DirectorioPage = lazy(() => import('./pages/DirectorioPage').then((m) => ({ default: m.DirectorioPage })))
const PacientesPage = lazy(() => import('./pages/PacientesPage').then((m) => ({ default: m.PacientesPage })))
const InventarioPage = lazy(() => import('./pages/InventarioPage').then((m) => ({ default: m.InventarioPage })))
const DashboardDoctorPage = lazy(() =>
  import('./pages/DashboardDoctorPage').then((m) => ({ default: m.DashboardDoctorPage })),
)
const EquipoPage = lazy(() => import('./pages/EquipoPage').then((m) => ({ default: m.EquipoPage })))
const EquipoDetallePage = lazy(() =>
  import('./pages/EquipoDetallePage').then((m) => ({ default: m.EquipoDetallePage })),
)
const MiPerfilPage = lazy(() => import('./pages/MiPerfilPage').then((m) => ({ default: m.MiPerfilPage })))

function CargandoPagina() {
  return <div className="pantalla-centrada text-secondary">Cargando…</div>
}

function App() {
  return (
    <Suspense fallback={<CargandoPagina />}>
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
    </Suspense>
  )
}

export default App
