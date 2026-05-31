import { NavLink, Outlet } from 'react-router-dom'

const tabs = [
  { to: '/',                       label: 'Overview',      end: true },
  { to: '/transactions-dashboard', label: 'Transactions',  end: false },
]

export function DashboardsLayout() {
  return (
    <div className="h-full flex flex-col overflow-hidden">
      <div className="px-6 pt-4 pb-0 shrink-0 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <nav className="flex gap-1">
          {tabs.map(tab => (
            <NavLink
              key={tab.to}
              to={tab.to}
              end={tab.end}
              className={({ isActive }) =>
                `px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors ${
                  isActive
                    ? 'border-indigo-600 text-indigo-600 dark:border-indigo-400 dark:text-indigo-400'
                    : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
                }`
              }
            >
              {tab.label}
            </NavLink>
          ))}
        </nav>
      </div>
      <div className="flex-1 overflow-hidden">
        <Outlet />
      </div>
    </div>
  )
}
