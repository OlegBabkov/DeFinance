import { useState } from 'react'

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

interface Props {
  onLogin: (username: string) => void
}

export function LoginPage({ onLogin }: Props) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onLogin(username)
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <span className="text-3xl font-bold tracking-tight text-gray-900 dark:text-gray-100">
            💰 DeFinance
          </span>
          <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">Sign in to your account</p>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-8 shadow-sm">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className={labelCls}>Username</label>
              <input
                required
                autoFocus
                type="text"
                value={username}
                onChange={e => setUsername(e.target.value)}
                className={inputCls}
                placeholder="Enter your username"
              />
            </div>
            <div>
              <label className={labelCls}>Password</label>
              <input
                required
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                className={inputCls}
                placeholder="Enter your password"
              />
            </div>
            <button
              type="submit"
              className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors mt-2"
            >
              Login
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
