import { useState } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useTheme } from '../ThemeContext'

const links: { to: string; labelKey: string; icon: string; match?: string[] }[] = [
  { to: '/', labelKey: 'sidebar.nav.dashboards', icon: '📊', match: ['/', '/transactions-dashboard'] },
  { to: '/transactions', labelKey: 'sidebar.nav.transactions', icon: '💳' },
  { to: '/plan-fact', labelKey: 'sidebar.nav.planFact', icon: '📈' },
  { to: '/mandatory', labelKey: 'sidebar.nav.mandatory', icon: '📋' },
  { to: '/accounts', labelKey: 'sidebar.nav.accounts', icon: '🏦' },
  { to: '/categories', labelKey: 'sidebar.nav.categories', icon: '🏷️' },
  { to: '/currencies', labelKey: 'sidebar.nav.currencies', icon: '💱' },
  { to: '/counterparties', labelKey: 'sidebar.nav.counterparties', icon: '🤝' },
  { to: '/administration', labelKey: 'sidebar.nav.administration', icon: '⚙️' },
]

const ChevronLeft = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" width={13} height={13}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
  </svg>
)

const ChevronRight = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" width={13} height={13}>
    <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
  </svg>
)

function SideTooltip({ label }: { label: string }) {
  return (
    <span className="pointer-events-none absolute left-full top-1/2 -translate-y-1/2 ml-3 px-2.5 py-1 rounded-md bg-gray-800 border border-gray-700 text-white text-xs whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity z-50 shadow-lg">
      {label}
    </span>
  )
}

export function Sidebar() {
  const { t } = useTranslation()
  const { dark, toggle } = useTheme()
  const { pathname } = useLocation()
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem('sidebar_collapsed') === 'true')

  const toggleCollapsed = () => {
    setCollapsed(c => {
      localStorage.setItem('sidebar_collapsed', String(!c))
      return !c
    })
  }

  return (
    <aside
      className={`${collapsed ? 'w-14' : 'w-56'} shrink-0 min-h-screen bg-gray-600 dark:bg-gray-950 text-white flex flex-col transition-[width] duration-200`}
    >
      {/* Header */}
      <div className="flex items-center border-b border-gray-500 dark:border-gray-800 h-[62px] px-3 gap-2">
        {collapsed ? (
          <button
            onClick={toggleCollapsed}
            className="flex items-center justify-center w-full py-1 text-gray-300 hover:text-white transition-colors rounded hover:bg-gray-500 dark:hover:bg-gray-800"
            title={t('sidebar.expand')}
          >
            <ChevronRight />
          </button>
        ) : (
          <>
            <span className="text-xl flex-shrink-0">💰</span>
            <span className="flex-1 text-xl font-bold tracking-tight whitespace-nowrap overflow-hidden">DeFinance</span>
            <button
              onClick={toggleCollapsed}
              className="flex-shrink-0 p-1.5 rounded text-gray-300 hover:text-white hover:bg-gray-500 dark:hover:bg-gray-800 transition-colors"
              title={t('sidebar.collapse')}
            >
              <ChevronLeft />
            </button>
          </>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 py-4">
        {links.map(({ to, labelKey, icon, match }) => (
          <div key={to} className="relative group">
            <NavLink
              to={to}
              end={!match}
              className={({ isActive }) => {
                const active = match ? match.includes(pathname) : isActive
                return `flex items-center gap-3 py-3 text-sm transition-colors ${collapsed ? 'justify-center px-0' : 'px-6'} ${
                  active
                    ? 'bg-indigo-600 text-white'
                    : 'text-gray-200 hover:bg-gray-500 dark:hover:bg-gray-800 hover:text-white'
                }`
              }}
            >
              <span className="flex-shrink-0 leading-none">{icon}</span>
              {!collapsed && <span className="truncate">{t(labelKey)}</span>}
            </NavLink>
            {collapsed && <SideTooltip label={t(labelKey)} />}
          </div>
        ))}
      </nav>

      {/* Footer */}
      <div className="border-t border-gray-500 dark:border-gray-800 px-3 py-4">
        <div className="relative group">
          <button
            onClick={toggle}
            className={`flex items-center gap-2 text-sm text-gray-200 hover:text-white transition-colors w-full ${collapsed ? 'justify-center' : ''}`}
          >
            <span className="flex-shrink-0">{dark ? '☀️' : '🌙'}</span>
            {!collapsed && <span>{dark ? t('sidebar.lightMode') : t('sidebar.darkMode')}</span>}
          </button>
          {collapsed && <SideTooltip label={dark ? t('sidebar.lightMode') : t('sidebar.darkMode')} />}
        </div>
      </div>
    </aside>
  )
}
