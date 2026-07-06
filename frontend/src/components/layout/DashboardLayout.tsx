import type { ReactNode } from 'react'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'
import { FondoBlobs } from './FondoBlobs'
import './DashboardLayout.css'

interface DashboardLayoutProps {
  titulo: string
  children: ReactNode
}

export function DashboardLayout({ titulo, children }: DashboardLayoutProps) {
  return (
    <div className="dashboard-layout">
      <FondoBlobs />
      <Sidebar />
      <div className="dashboard-layout-main">
        <Topbar titulo={titulo} />
        <main className="dashboard-layout-content">{children}</main>
      </div>
    </div>
  )
}
