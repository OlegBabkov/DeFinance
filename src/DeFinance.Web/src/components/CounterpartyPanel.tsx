import { useEffect, useState } from 'react'
import { type Counterparty } from '../api/counterparties'
import { transactionsApi } from '../api/transactions'

interface Props {
  counterparty: Counterparty | null
  onClose: () => void
}

interface MonthStat {
  label: string
  total: number
}

function getLastSixMonths(): { year: number; month: number; label: string }[] {
  const now = new Date()
  return Array.from({ length: 6 }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - (5 - i), 1)
    return {
      year: d.getFullYear(),
      month: d.getMonth(),
      label: d.toLocaleDateString('en-US', { month: 'long', year: 'numeric' }),
    }
  })
}

function fmt(value: number) {
  return value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function CounterpartyPanel({ counterparty, onClose }: Props) {
  const open = counterparty !== null
  const [stats, setStats] = useState<MonthStat[]>([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!counterparty) { setStats([]); return }

    setLoading(true)
    const sixMonthsAgo = new Date()
    sixMonthsAgo.setMonth(sixMonthsAgo.getMonth() - 6)
    sixMonthsAgo.setDate(1)
    const dateFrom = sixMonthsAgo.toISOString().split('T')[0]

    transactionsApi.getAll({
      counterpartyId: counterparty.id,
      dateFrom,
      pageSize: 500,
    })
      .then(r => {
        const months = getLastSixMonths()
        const result: MonthStat[] = months.map(m => {
          const total = r.items
            .filter(tx => {
              if (tx.category?.type !== 'Expense') return false
              const d = new Date(tx.dateTime)
              return d.getFullYear() === m.year && d.getMonth() === m.month
            })
            .reduce((s, tx) => s + tx.amountInCurrency, 0)
          return { label: m.label, total }
        })
        setStats(result)
      })
      .catch(() => setStats([]))
      .finally(() => setLoading(false))
  }, [counterparty?.id])

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
          <div className="min-w-0">
            <span className="text-sm font-semibold text-gray-800 dark:text-gray-100 truncate block">
              {counterparty?.name ?? 'Counterparty Details'}
            </span>
            {counterparty?.type && (
              <span className="text-xs text-gray-400 dark:text-gray-500">{counterparty.type}</span>
            )}
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none ml-3 shrink-0"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          <p className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider">
            Expenses — Last 6 Months
          </p>

          {loading && (
            <p className="text-xs text-gray-400 dark:text-gray-500">Loading…</p>
          )}

          {!loading && (
            <ul className="space-y-2">
              {stats.map(({ label, total }) => (
                <li
                  key={label}
                  className="flex items-center justify-between py-2 border-b border-gray-100 dark:border-gray-700 last:border-0"
                >
                  <span className="text-sm text-gray-600 dark:text-gray-400">{label}</span>
                  <span className={`text-sm font-medium font-mono ${total > 0 ? 'text-red-500 dark:text-red-400' : 'text-gray-400 dark:text-gray-500'}`}>
                    {total > 0 ? `− ${fmt(total)}` : '—'}
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
