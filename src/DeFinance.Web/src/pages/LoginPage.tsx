import { useState } from 'react'
import { authApi, saveToken } from '../api/auth'

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

interface Props {
  onLogin: (username: string) => void
}

export function LoginPage({ onLogin }: Props) {
  const [mode, setMode] = useState<'login' | 'register'>('login')

  // login fields
  const [loginUsername, setLoginUsername] = useState('')
  const [loginPassword, setLoginPassword] = useState('')

  // register fields
  const [regUsername, setRegUsername] = useState('')
  const [regEmail, setRegEmail] = useState('')
  const [regPassword, setRegPassword] = useState('')
  const [regConfirm, setRegConfirm] = useState('')

  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const switchMode = (m: 'login' | 'register') => { setMode(m); setError(null) }

  const handleLogin = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const res = await authApi.login({ username: loginUsername, password: loginPassword })
      saveToken(res.token)
      onLogin(res.username)
    } catch {
      setError('Invalid username or password.')
    } finally {
      setLoading(false)
    }
  }

  const handleRegister = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setError(null)
    if (regPassword !== regConfirm) { setError('Passwords do not match.'); return }
    setLoading(true)
    try {
      await authApi.register({ username: regUsername, email: regEmail, password: regPassword, confirmPassword: regConfirm })
      const res = await authApi.login({ username: regUsername, password: regPassword })
      saveToken(res.token)
      onLogin(res.username)
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { errors?: Record<string, string[]>; message?: string } } })
        ?.response?.data
      if (msg?.errors) {
        const first = Object.values(msg.errors).flat()[0]
        setError(first ?? 'Registration failed.')
      } else {
        setError(msg?.message ?? 'Registration failed. Please check your input.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <span className="text-3xl font-bold tracking-tight text-gray-900 dark:text-gray-100">
            💰 DeFinance
          </span>
          <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
            {mode === 'login' ? 'Sign in to your account' : 'Create a new account'}
          </p>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-8 shadow-sm">
          {/* Tab switcher */}
          <div className="flex rounded-lg bg-gray-100 dark:bg-gray-700 p-1 mb-6">
            <button
              type="button"
              onClick={() => switchMode('login')}
              className={`flex-1 py-1.5 text-sm font-medium rounded-md transition-colors ${
                mode === 'login'
                  ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-gray-100 shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
            >
              Sign In
            </button>
            <button
              type="button"
              onClick={() => switchMode('register')}
              className={`flex-1 py-1.5 text-sm font-medium rounded-md transition-colors ${
                mode === 'register'
                  ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-gray-100 shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
            >
              Sign Up
            </button>
          </div>

          {mode === 'login' ? (
            <form onSubmit={handleLogin} className="space-y-5">
              <div>
                <label className={labelCls}>Username</label>
                <input
                  required autoFocus type="text"
                  value={loginUsername} onChange={e => setLoginUsername(e.target.value)}
                  className={inputCls} placeholder="Enter your username"
                />
              </div>
              <div>
                <label className={labelCls}>Password</label>
                <input
                  required type="password"
                  value={loginPassword} onChange={e => setLoginPassword(e.target.value)}
                  className={inputCls} placeholder="Enter your password"
                />
              </div>
              {error && <p className="text-sm text-red-500">{error}</p>}
              <button
                type="submit" disabled={loading}
                className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
              >
                {loading ? 'Signing in…' : 'Sign In'}
              </button>
            </form>
          ) : (
            <form onSubmit={handleRegister} className="space-y-5">
              <div>
                <label className={labelCls}>Username</label>
                <input
                  required autoFocus type="text"
                  value={regUsername} onChange={e => setRegUsername(e.target.value)}
                  className={inputCls} placeholder="Choose a username"
                />
              </div>
              <div>
                <label className={labelCls}>Email</label>
                <input
                  required type="email"
                  value={regEmail} onChange={e => setRegEmail(e.target.value)}
                  className={inputCls} placeholder="your@email.com"
                />
              </div>
              <div>
                <label className={labelCls}>Password</label>
                <input
                  required type="password" minLength={6}
                  value={regPassword} onChange={e => setRegPassword(e.target.value)}
                  className={inputCls} placeholder="Min. 6 characters"
                />
              </div>
              <div>
                <label className={labelCls}>Confirm Password</label>
                <input
                  required type="password"
                  value={regConfirm} onChange={e => setRegConfirm(e.target.value)}
                  className={inputCls} placeholder="Repeat your password"
                />
              </div>
              {error && <p className="text-sm text-red-500">{error}</p>}
              <button
                type="submit" disabled={loading}
                className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
              >
                {loading ? 'Creating account…' : 'Create Account'}
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
