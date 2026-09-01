import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { transactionsApi, type Transaction } from '../api/transactions'
import { Spinner } from './Spinner'
import { AddCalendarEventModal } from './AddCalendarEventModal'

interface Props {
  day: Date | null
  intlLocale: string
  onClose: () => void
}

function fmtAmount(n: number) {
  return n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function amountColor(type: string) {
  if (type === 'Income' || type === 'TransferIn') return 'text-emerald-600 dark:text-emerald-400'
  if (type === 'Expense' || type === 'TransferOut') return 'text-red-500 dark:text-red-400'
  return 'text-gray-500 dark:text-gray-400'
}

function amountSign(type: string) {
  if (type === 'Income' || type === 'TransferIn') return '+'
  if (type === 'Expense' || type === 'TransferOut') return '−'
  return ''
}

export function CalendarDayPanel({ day, intlLocale, onClose }: Props) {
  const { t } = useTranslation()
  const open = day !== null
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loading, setLoading] = useState(false)
  const [addEventOpen, setAddEventOpen] = useState(false)

  const dayStr = day ? day.toISOString().split('T')[0] : ''

  const reload = () => {
    if (!day) return
    setLoading(true)
    transactionsApi
      .getAll({ dateFrom: dayStr, dateTo: dayStr, pageSize: 200, sortBy: 'dateTime', sortDirection: 'Desc' })
      .then(r => setTransactions(r.items))
      .catch(() => setTransactions([]))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    if (!day) { setTransactions([]); return }
    reload()
  }, [day?.getTime()])

  const income   = transactions.filter(tx => tx.category?.type === 'Income'      || tx.category?.type === 'TransferIn').reduce((s, tx) => s + tx.sum, 0)
  const expenses = transactions.filter(tx => tx.category?.type === 'Expense'     || tx.category?.type === 'TransferOut').reduce((s, tx) => s + tx.sum, 0)
  const net = income - expenses

  const dateLabel = day
    ? (() => {
        const s = new Intl.DateTimeFormat(intlLocale, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).format(day)
        return s.charAt(0).toUpperCase() + s.slice(1)
      })()
    : ''

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
        <div className="flex items-start justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-700 shrink-0">
          <div className="flex-1 min-w-0">
            <span className="text-sm font-semibold text-gray-800 dark:text-gray-100 leading-snug">{dateLabel}</span>
            <div className="mt-2">
              <button
                onClick={() => setAddEventOpen(true)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-medium rounded-lg transition-colors"
              >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" className="w-3.5 h-3.5">
                  <path d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z" />
                </svg>
                {t('calendar.dayPanel.addEvent')}
              </button>
            </div>
          </div>
          <button
            onClick={onClose}
            className="ml-3 shrink-0 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          {loading && <Spinner size="sm" />}

          {!loading && transactions.length === 0 && (
            <p className="text-sm text-gray-400 dark:text-gray-500 text-center py-10">
              {t('calendar.dayPanel.noTransactions')}
            </p>
          )}

          {!loading && transactions.length > 0 && (
            <>
              {/* Day summary */}
              <div className="rounded-lg bg-gray-50 dark:bg-gray-700/50 p-3 space-y-1.5">
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-500 dark:text-gray-400">{t('accountPanel.monthSummary.income')}</span>
                  <span className="font-mono font-medium text-emerald-600 dark:text-emerald-400">
                    {income > 0 ? `+ ${fmtAmount(income)}` : fmtAmount(0)}
                  </span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-500 dark:text-gray-400">{t('accountPanel.monthSummary.losses')}</span>
                  <span className="font-mono font-medium text-red-500 dark:text-red-400">
                    {expenses > 0 ? `− ${fmtAmount(expenses)}` : fmtAmount(0)}
                  </span>
                </div>
                <div className="flex justify-between items-center text-sm border-t border-gray-200 dark:border-gray-600 pt-1.5">
                  <span className="font-medium text-gray-700 dark:text-gray-300">{t('accountPanel.monthSummary.net')}</span>
                  <span className={`font-mono font-semibold ${net > 0 ? 'text-emerald-600 dark:text-emerald-400' : net < 0 ? 'text-red-500 dark:text-red-400' : 'text-gray-500 dark:text-gray-400'}`}>
                    {net > 0 ? '+ ' : net < 0 ? '− ' : ''}{fmtAmount(Math.abs(net))}
                  </span>
                </div>
              </div>

              {/* Transaction list */}
              <p className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider">
                {t('transactions.title')}
              </p>
              <ul className="space-y-0">
                {transactions.map(tx => {
                  const type   = tx.category?.type ?? ''
                  const symbol = tx.account?.currency?.symbol ?? ''
                  const time   = new Date(tx.dateTime).toLocaleTimeString(intlLocale, { hour: '2-digit', minute: '2-digit' })
                  return (
                    <li
                      key={tx.id}
                      className="flex items-start justify-between gap-3 py-2.5 border-b border-gray-100 dark:border-gray-700 last:border-0"
                    >
                      <div className="flex items-start gap-2 min-w-0">
                        {tx.category?.color ? (
                          <span className="w-2 h-2 rounded-full shrink-0 mt-1.5" style={{ backgroundColor: tx.category.color }} />
                        ) : (
                          <span className="w-2 h-2 shrink-0" />
                        )}
                        <div className="min-w-0">
                          <p className="text-sm text-gray-800 dark:text-gray-200 truncate">
                            {tx.category?.icon && <span className="mr-1">{tx.category.icon}</span>}
                            {tx.category?.name ?? '—'}
                          </p>
                          <p className="text-xs text-gray-400 dark:text-gray-500 truncate">{tx.account?.name ?? ''} · {time}</p>
                          {tx.counterparty && (
                            <p className="text-xs text-gray-400 dark:text-gray-500 truncate">{tx.counterparty.name}</p>
                          )}
                          {tx.notes && (
                            <p className="text-xs text-gray-400 dark:text-gray-500 italic truncate">{tx.notes}</p>
                          )}
                        </div>
                      </div>
                      <div className="text-right shrink-0">
                        <p className={`text-sm font-mono font-medium ${amountColor(type)}`}>
                          {amountSign(type)}{symbol} {fmtAmount(tx.sum)}
                        </p>
                        {tx.paymentStatus && (
                          <p
                            className="text-xs mt-0.5 font-medium"
                            style={{ color: tx.paymentStatus.color ?? undefined }}
                          >
                            {tx.paymentStatus.name}
                          </p>
                        )}
                      </div>
                    </li>
                  )
                })}
              </ul>
            </>
          )}
        </div>
      </div>

      {addEventOpen && day && (
        <AddCalendarEventModal
          initialDate={dayStr}
          onClose={() => setAddEventOpen(false)}
          onCreated={reload}
        />
      )}
    </>
  )
}
