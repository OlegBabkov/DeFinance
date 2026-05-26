import { NavLink } from 'react-router-dom'
import { useTheme } from '../ThemeContext'

const links = [
  { to: '/', label: 'Dashboard', icon: '📊' },
  { to: '/transactions', label: 'Transactions', icon: '💳' },
  { to: '/mandatory', label: 'Mandatory', icon: '📋' },
  { to: '/accounts', label: 'Accounts', icon: '🏦' },
  { to: '/categories', label: 'Categories', icon: '🏷️' },
  { to: '/currencies', label: 'Currencies', icon: '💱' },
  { to: '/counterparties', label: 'Counterparties', icon: '🤝' },
  { to: '/administration', label: 'Administration', icon: '⚙️' },
]

export function Sidebar() {
  const { dark, toggle } = useTheme()

  return (
    <aside className="w-56 min-h-screen bg-gray-900 dark:bg-gray-950 text-white flex flex-col">
      <div className="px-6 py-5 border-b border-gray-700 dark:border-gray-800">
        <span className="text-xl font-bold tracking-tight">💰 DeFinance</span>
      </div>
      <nav className="flex-1 py-4">
        {links.map(({ to, label, icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              `flex items-center gap-3 px-6 py-3 text-sm transition-colors ${
                isActive
                  ? 'bg-indigo-600 text-white'
                  : 'text-gray-400 hover:bg-gray-800 hover:text-white'
              }`
            }
          >
            <span>{icon}</span>
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="px-6 py-4 border-t border-gray-700 dark:border-gray-800">
        <button
          onClick={toggle}
          className="flex items-center gap-2 text-sm text-gray-400 hover:text-white transition-colors w-full"
        >
          <span>{dark ? '☀️' : '🌙'}</span>
          {dark ? 'Light mode' : 'Dark mode'}
        </button>
      </div>
    </aside>
  )
}
