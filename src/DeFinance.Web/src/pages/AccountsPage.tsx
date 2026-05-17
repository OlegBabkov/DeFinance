import { useEffect, useState } from 'react'
import { accountsApi, type Account } from '../api/accounts'

const ACCOUNT_TYPE_LABEL: Record<string, string> = {
  Checking: 'Checking',
  Savings: 'Savings',
  Credit: 'Credit',
  Cash: 'Cash',
  Investment: 'Investment',
}

export function AccountsPage() {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    accountsApi
      .getAll()
      .then(setAccounts)
      .catch(() => setError('Failed to load accounts'))
      .finally(() => setLoading(false))
  }, [])

  const toggle = async (account: Account) => {
    const updated = account.isActive
      ? await accountsApi.deactivate(account.id)
      : await accountsApi.activate(account.id)
    setAccounts(prev => prev.map(a => (a.id === updated.id ? updated : a)))
  }

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Accounts</h1>
      <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              {['Name', 'Type', 'Balance', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
            {accounts.map(account => (
              <tr key={account.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{account.name}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{ACCOUNT_TYPE_LABEL[account.type]}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono">
                  {account.balance.toFixed(2)}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      account.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                        : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                    }`}
                  >
                    {account.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => toggle(account)}
                    className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
                  >
                    {account.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </td>
              </tr>
            ))}
            {accounts.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">
                  No accounts yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
