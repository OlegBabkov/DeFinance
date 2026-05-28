import { useEffect, useState } from 'react'
import { type Account } from '../api/accounts'
import { transactionsApi, type Transaction } from '../api/transactions'

interface Props {
  account: Account | null
  onClose: () => void
}

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

function fmtNum(n: number, symbol: string) {
  return `${symbol} ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function amountColor(type: string) {
  if (type === 'Income') return 'text-emerald-600 dark:text-emerald-400'
  if (type === 'Expense') return 'text-red-500 dark:text-red-400'
  return 'text-gray-500 dark:text-gray-400'
}

function amountSign(type: string) {
  if (type === 'Income') return '+'
  if (type === 'Expense') return '−'
  return ''
}

interface MonthSummary { income: number; losses: number }

export function AccountPanel({ account, onClose }: Props) {
  const open = account !== null
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [monthSummary, setMonthSummary] = useState<MonthSummary | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!account) { setTransactions([]); setMonthSummary(null); return }
    setLoading(true)

    const now = new Date()
    const monthFrom = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0]
    const monthTo = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().split('T')[0]

    Promise.all([
      transactionsApi.getAll({ accountId: account.id, pageSize: 10, sortBy: 'dateTime', sortDirection: 'Desc' }),
      transactionsApi.getAll({ accountId: account.id, dateFrom: monthFrom, dateTo: monthTo, pageSize: 500 }),
    ])
      .then(([recent, monthly]) => {
        setTransactions(recent.items)
        const income = monthly.items.filter(t => t.category?.type === 'Income').reduce((s, t) => s + t.sum, 0)
        const losses = monthly.items.filter(t => t.category?.type === 'Expense').reduce((s, t) => s + t.sum, 0)
        setMonthSummary({ income, losses })
      })
      .catch(() => { setTransactions([]); setMonthSummary(null) })
      .finally(() => setLoading(false))
  }, [account?.id])

  const symbol = account?.currency?.symbol ?? ''

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        className={`fixed inset-0 z-30 bg-black/20 dark:bg-black/40 transition-opacity duration-300 ${open ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'}`}
      />

      {/* Panel */}
      <div
        className={`fixed top-12 right-0 bottom-0 w-96 z-40 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 shadow-xl flex flex-col transform transition-transform duration-300 ease-in-out ${open ? 'translate-x-0' : 'translate-x-full'}`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-700 shrink-0">
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-100">Account Details</span>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          {/* This month summary */}
          <p className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider">This Month</p>
          {monthSummary ? (
            <ul className="space-y-0">
              <li className="flex items-center justify-between py-2 border-b border-gray-100 dark:border-gray-700">
                <span className="text-sm text-gray-600 dark:text-gray-400">Income</span>
                <span className="text-sm font-mono font-medium text-emerald-600 dark:text-emerald-400">
                  {monthSummary.income > 0 ? `+ ${fmtNum(monthSummary.income, symbol)}` : `${fmtNum(0, symbol)}`}
                </span>
              </li>
              <li className="flex items-center justify-between py-2">
                <span className="text-sm text-gray-600 dark:text-gray-400">Losses</span>
                <span className="text-sm font-mono font-medium text-red-500 dark:text-red-400">
                  {monthSummary.losses > 0 ? `− ${fmtNum(monthSummary.losses, symbol)}` : `${fmtNum(0, symbol)}`}
                </span>
              </li>
            </ul>
          ) : (
            <p className="text-xs text-gray-400 dark:text-gray-500">Loading…</p>
          )}

          {/* Last transactions */}
          <p className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider">
            Last 10 Transactions
          </p>

          {loading && (
            <p className="text-xs text-gray-400 dark:text-gray-500">Loading…</p>
          )}

          {!loading && transactions.length === 0 && (
            <p className="text-xs text-gray-400 dark:text-gray-500">No transactions found.</p>
          )}

          {!loading && transactions.length > 0 && (
            <ul className="space-y-2">
              {transactions.map(tx => (
                <li key={tx.id} className="flex items-start justify-between gap-3 py-2 border-b border-gray-100 dark:border-gray-700 last:border-0">
                  <div className="flex items-center gap-2 min-w-0">
                    {tx.category?.color && (
                      <span className="w-2 h-2 rounded-full shrink-0 mt-0.5" style={{ backgroundColor: tx.category.color }} />
                    )}
                    <div className="min-w-0">
                      <p className="text-sm text-gray-800 dark:text-gray-200 truncate">
                        {tx.category?.icon && <span className="mr-1">{tx.category.icon}</span>}
                        {tx.category?.name ?? '—'}
                      </p>
                      <p className="text-xs text-gray-400 dark:text-gray-500">{fmtDate(tx.dateTime)}</p>
                      {tx.notes && (
                        <p className="text-xs text-gray-400 dark:text-gray-500 italic truncate">{tx.notes}</p>
                      )}
                    </div>
                  </div>
                  <span className={`text-sm font-mono font-medium shrink-0 ${amountColor(tx.category?.type ?? '')}`}>
                    {amountSign(tx.category?.type ?? '')}{fmtNum(tx.sum, symbol)}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </>
  )
}
