import { lazy, Suspense, useEffect, useState } from 'react'
import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { ModoMantenimientoScreen } from './components/ModoMantenimientoScreen'
import { suscribirseAMantenimiento } from './lib/mantenimientoBus'
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
const FinanzasPage = lazy(() => import('./pages/FinanzasPage').then((m) => ({ default: m.FinanzasPage })))
const GastosPage = lazy(() => import('./pages/GastosPage').then((m) => ({ default: m.GastosPage })))
const DonacionesPage = lazy(() => import('./pages/DonacionesPage').then((m) => ({ default: m.DonacionesPage })))
const OperacionesPage = lazy(() => import('./pages/OperacionesPage').then((m) => ({ default: m.OperacionesPage })))
const ActividadPage = lazy(() => import('./pages/ActividadPage').then((m) => ({ default: m.ActividadPage })))
const CajaPage = lazy(() => import('./pages/CajaPage').then((m) => ({ default: m.CajaPage })))
const CobrosPage = lazy(() => import('./pages/CobrosPage').then((m) => ({ default: m.CobrosPage })))
const RecepcionPage = lazy(() => import('./pages/RecepcionPage').then((m) => ({ default: m.RecepcionPage })))
const AgendaPage = lazy(() => import('./pages/AgendaPage').then((m) => ({ default: m.AgendaPage })))
const AseguradorasPage = lazy(() => import('./pages/AseguradorasPage').then((m) => ({ default: m.AseguradorasPage })))
const CitasPage = lazy(() => import('./pages/CitasPage').then((m) => ({ default: m.CitasPage })))
const PacienteHistorialPage = lazy(() =>
  import('./pages/PacienteHistorialPage').then((m) => ({ default: m.PacienteHistorialPage })),
)
const PersonalPage = lazy(() => import('./pages/PersonalPage').then((m) => ({ default: m.PersonalPage })))
const PermisosPage = lazy(() => import('./pages/PermisosPage').then((m) => ({ default: m.PermisosPage })))
const DirectorioPage = lazy(() => import('./pages/DirectorioPage').then((m) => ({ default: m.DirectorioPage })))
const PacientesPage = lazy(() => import('./pages/PacientesPage').then((m) => ({ default: m.PacientesPage })))
const InventarioPage = lazy(() => import('./pages/InventarioPage').then((m) => ({ default: m.InventarioPage })))
const DashboardDoctorPage = lazy(() =>
  import('./pages/DashboardDoctorPage').then((m) => ({ default: m.DashboardDoctorPage })),
)
const MiPerfilPage = lazy(() => import('./pages/MiPerfilPage').then((m) => ({ default: m.MiPerfilPage })))

function CargandoPagina() {
  return <div className="pantalla-centrada text-secondary cargando-pulso">Cargando…</div>
}

function App() {
  const [mensajeMantenimiento, setMensajeMantenimiento] = useState<string | null | undefined>(undefined)

  useEffect(() => suscribirseAMantenimiento(setMensajeMantenimiento), [])

  if (mensajeMantenimiento !== undefined) {
    return <ModoMantenimientoScreen mensaje={mensajeMantenimiento} />
  }

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
            <ProtectedRoute rolesPermitidos="Admin" modulo="Dashboard">
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/resumen"
          element={
            <ProtectedRoute rolesPermitidos="Admin" modulo="Resumen">
              <ResumenPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/finanzas"
          element={
            <ProtectedRoute rolesPermitidos="Admin" modulo="Finanzas">
              <FinanzasPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/gastos"
          element={
            <ProtectedRoute rolesPermitidos="Admin" modulo="Gastos">
              <GastosPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/donaciones"
          element={
            <ProtectedRoute rolesPermitidos="Admin" modulo="Donaciones">
              <DonacionesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/personal"
          element={
            <ProtectedRoute rolesPermitidos={['Lemy', 'Admin']}>
              <PersonalPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/permisos"
          element={
            <ProtectedRoute rolesPermitidos={['Lemy', 'Admin']}>
              <PermisosPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/operaciones"
          element={
            <ProtectedRoute rolesPermitidos="Admin" modulo="Operaciones">
              <OperacionesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/actividad"
          element={
            <ProtectedRoute rolesPermitidos={['Lemy', 'Admin']} modulo="Actividad">
              <ActividadPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/caja"
          element={
            <ProtectedRoute rolesPermitidos="Fondos" modulo="Caja">
              <CajaPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/cobros"
          element={
            <ProtectedRoute rolesPermitidos="Fondos" modulo="Cobros">
              <CobrosPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/recepcion"
          element={
            <ProtectedRoute rolesPermitidos="Fondos" modulo="Recepcion">
              <RecepcionPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/agenda"
          element={
            <ProtectedRoute rolesPermitidos="Fondos" modulo="Agenda">
              <AgendaPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/aseguradoras"
          element={
            <ProtectedRoute rolesPermitidos={['Admin', 'Lemy']} modulo="Aseguradoras">
              <AseguradorasPage />
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
            <ProtectedRoute rolesPermitidos={['Doctor', 'Admin']} modulo="HistorialClinico">
              <PacienteHistorialPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/directorio"
          element={
            <ProtectedRoute rolesPermitidos={['Admin', 'Lemy']} modulo="Directorio">
              <DirectorioPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/pacientes"
          element={
            <ProtectedRoute rolesPermitidos={['Admin', 'Doctor', 'Fondos', 'Lemy']} modulo="Pacientes">
              <PacientesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/inventario"
          element={
            <ProtectedRoute rolesPermitidos={['Admin', 'Doctor', 'Fondos', 'Lemy']} modulo="Inventario">
              <InventarioPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Suspense>
  )
}

export default App
