import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  PieChart, Pie, Cell, Tooltip, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Legend,
} from 'recharts'
import { transactionsApi, type Transaction } from '../api/transactions'
import { accountsApi, type Account } from '../api/accounts'
import { useMainCurrency } from '../MainCurrencyContext'
import { useTheme } from '../ThemeContext'
import { useTransactionEvents } from '../hooks/useTransactionEvents'

const PALETTE = ['#6366f1','#f59e0b','#10b981','#ef4444','#3b82f6','#8b5cf6','#ec4899','#14b8a6','#f97316','#84cc16']

function fmt(value: number, symbol: string) {
  return `${symbol}${value.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`
}

function getLastNMonths(n: number) {
  const now = new Date()
  return Array.from({ length: n }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - (n - 1 - i), 1)
    return { year: d.getFullYear(), month: d.getMonth(), label: d.toLocaleDateString('en-US', { month: 'short', year: n > 6 ? '2-digit' : undefined }) }
  })
}

function getMonthLabel(offset: number) {
  const d = new Date()
  d.setMonth(d.getMonth() + offset)
  return d.toLocaleDateString('en-US', { month: 'short', year: '2-digit' })
}

interface StatCardProps { label: string; value: string; sub?: string; valueColor?: string }
function StatCard({ label, value, sub, valueColor = 'text-gray-900 dark:text-gray-100' }: StatCardProps) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
      <p className="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">{label}</p>
      <p className={`mt-1 text-2xl font-bold ${valueColor}`}>{value}</p>
      {sub && <p className="mt-1 text-xs text-gray-400 dark:text-gray-500">{sub}</p>}
    </div>
  )
}

const MONTH_OPTIONS  = [1, 3, 6, 12]  as const
const DAY_OPTIONS    = [30, 60, 90, 180, 365] as const
const STAT_OFFSETS   = [0, -1, -2, -3] as const

type MonthOption  = typeof MONTH_OPTIONS[number]
type DayOption    = typeof DAY_OPTIONS[number]
type StatOffset   = typeof STAT_OFFSETS[number]

function monthLabel(n: MonthOption)    { return n === 12 ? '1Y' : `${n}M` }
function dayLabel(n: DayOption)        { return n === 365 ? '1Y' : `${n}d` }

interface PeriodTabsProps<T extends number> {
  options: readonly T[]
  value: T
  onChange: (v: T) => void
  format: (v: T) => string
}
function PeriodTabs<T extends number>({ options, value, onChange, format }: PeriodTabsProps<T>) {
  return (
    <div className="flex gap-0.5">
      {options.map(opt => (
        <button
          key={opt}
          onClick={() => onChange(opt)}
          className={`px-2 py-0.5 text-xs rounded transition-colors ${
            value === opt
              ? 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/60 dark:text-indigo-300 font-medium'
              : 'text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300'
          }`}
        >
          {format(opt)}
        </button>
      ))}
    </div>
  )
}

export function DashboardPage() {
  const { mainCurrency } = useMainCurrency()
  const { dark } = useTheme()
  const sym = mainCurrency?.symbol ?? '€'

  const [accounts, setAccounts]       = useState<Account[]>([])
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loading, setLoading]         = useState(true)
  const [error, setError]             = useState<string | null>(null)
  const [refreshKey, setRefreshKey]   = useState(0)

  const [cashFlowMonths, setCashFlowMonths] = useState<MonthOption>(6)
  const [categoryDays, setCategoryDays]     = useState<DayOption>(30)
  const [statOffset, setStatOffset]         = useState<StatOffset>(0)

  const reload = useCallback(() => setRefreshKey(k => k + 1), [])
  useTransactionEvents(reload)

  useEffect(() => {
    const twelveMonthsAgo = new Date()
    twelveMonthsAgo.setMonth(twelveMonthsAgo.getMonth() - 12)
    const dateFrom = twelveMonthsAgo.toISOString().split('T')[0]

    const fetchAllTransactions = async (df: string): Promise<Transaction[]> => {
      const first = await transactionsApi.getAll({ dateFrom: df, pageSize: 500, page: 1 })
      if (!first.hasNextPage) return first.items
      const rest = await Promise.all(
        Array.from({ length: first.totalPages - 1 }, (_, i) =>
          transactionsApi.getAll({ dateFrom: df, pageSize: 500, page: i + 2 })
        )
      )
      return [...first.items, ...rest.flatMap(r => r.items)]
    }

    Promise.all([
      accountsApi.getAll({ isActive: true, pageSize: 500 }),
      fetchAllTransactions(dateFrom),
    ]).then(([accs, txns]) => {
      setAccounts(accs.items)
      setTransactions(txns)
    }).catch(() => setError('Failed to load dashboard data.'))
      .finally(() => setLoading(false))
  }, [refreshKey])

  // Stat cards: transactions for the selected month
  const selectedMonthTx = useMemo(() => {
    const now = new Date()
    const start = new Date(now.getFullYear(), now.getMonth() + statOffset, 1)
    const end   = new Date(now.getFullYear(), now.getMonth() + statOffset + 1, 1)
    return transactions.filter(t => { const d = new Date(t.dateTime); return d >= start && d < end })
  }, [transactions, statOffset])

  const monthIncome   = selectedMonthTx.filter(t => t.category?.type === 'Income').reduce((s, t) => s + t.amountInCurrency, 0)
  const monthExpenses = selectedMonthTx.filter(t => t.category?.type === 'Expense').reduce((s, t) => s + t.amountInCurrency, 0)
  const monthNet      = monthIncome - monthExpenses

  const uniqueCurrencies = [...new Set(accounts.map(a => a.currency?.code).filter(Boolean))].join(' · ')

  const categoryData = useMemo(() => {
    const cutoff = new Date()
    cutoff.setDate(cutoff.getDate() - categoryDays)
    const map: Record<string, { name: string; value: number; color: string | null }> = {}
    transactions
      .filter(t => t.category?.type === 'Expense' && new Date(t.dateTime) >= cutoff)
      .forEach(t => {
        const key = t.category?.id ?? 'unknown'
        if (!map[key]) map[key] = { name: t.category?.name ?? 'Unknown', value: 0, color: t.category?.color ?? null }
        map[key].value += t.amountInCurrency
      })
    const sorted = Object.values(map).sort((a, b) => b.value - a.value)
    const TOP_N = 8
    if (sorted.length <= TOP_N) return sorted
    const top = sorted.slice(0, TOP_N)
    const otherValue = sorted.slice(TOP_N).reduce((s, c) => s + c.value, 0)
    return [...top, { name: 'Other', value: otherValue, color: '#6b7280' }]
  }, [transactions, categoryDays])

  const barData = useMemo(() => {
    const months = getLastNMonths(cashFlowMonths)
    return months.map(m => {
      const monthTx = transactions.filter(t => {
        const d = new Date(t.dateTime)
        return d.getFullYear() === m.year && d.getMonth() === m.month
      })
      return {
        month: m.label,
        Income:   monthTx.filter(t => t.category?.type === 'Income').reduce((s, t) => s + t.amountInCurrency, 0),
        Expenses: monthTx.filter(t => t.category?.type === 'Expense').reduce((s, t) => s + t.amountInCurrency, 0),
      }
    })
  }, [transactions, cashFlowMonths])

  const gridColor    = dark ? '#374151' : '#e5e7eb'
  const tickColor    = dark ? '#9ca3af' : '#6b7280'
  const tooltipStyle = {
    backgroundColor: dark ? '#1f2937' : '#fff',
    borderColor:     dark ? '#374151' : '#e5e7eb',
    color:           dark ? '#f3f4f6' : '#111827',
    borderRadius: 8,
    fontSize: 12,
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full text-gray-400 dark:text-gray-500 text-sm">
        Loading dashboard…
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-full text-red-500 dark:text-red-400 text-sm">
        {error}
      </div>
    )
  }

  return (
    <div className="p-6 space-y-6 overflow-y-auto h-full">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Dashboard</h1>

      {/* Stat cards */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <span className="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Monthly Summary</span>
          <PeriodTabs
            options={STAT_OFFSETS}
            value={statOffset}
            onChange={setStatOffset}
            format={getMonthLabel}
          />
        </div>
        <div className="grid grid-cols-4 gap-4">
          <StatCard
            label="Active Accounts"
            value={String(accounts.length)}
            sub={uniqueCurrencies || undefined}
          />
          <StatCard
            label="Income"
            value={fmt(monthIncome, sym)}
            valueColor="text-emerald-600 dark:text-emerald-400"
          />
          <StatCard
            label="Expenses"
            value={fmt(monthExpenses, sym)}
            valueColor="text-rose-600 dark:text-rose-400"
          />
          <StatCard
            label="Net"
            value={fmt(monthNet, sym)}
            valueColor={monthNet >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-600 dark:text-rose-400'}
            sub={monthNet >= 0 ? 'On track' : 'Over budget'}
          />
        </div>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-2 gap-6">

        {/* Donut: Spending by Category */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
          <div className="flex items-start justify-between mb-0.5">
            <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">Spending by Category</h2>
            <PeriodTabs options={DAY_OPTIONS} value={categoryDays} onChange={setCategoryDays} format={dayLabel} />
          </div>
          <p className="text-xs text-gray-400 dark:text-gray-500 mb-4">
            {categoryDays === 365 ? 'Last year' : `Last ${categoryDays} days`} — expenses only
          </p>
          {categoryData.length === 0 ? (
            <div className="flex items-center justify-center h-56 text-gray-400 dark:text-gray-500 text-sm">
              No expense data for this period
            </div>
          ) : (() => {
            const total = categoryData.reduce((s, c) => s + c.value, 0)
            return (
              <div className="flex gap-5 items-center">
                {/* Donut with center total */}
                <div className="relative flex-shrink-0" style={{ width: 190, height: 190 }}>
                  <PieChart width={190} height={190}>
                    <Pie
                      data={categoryData}
                      cx={95}
                      cy={95}
                      innerRadius={58}
                      outerRadius={90}
                      paddingAngle={2}
                      dataKey="value"
                      startAngle={90}
                      endAngle={-270}
                    >
                      {categoryData.map((entry, index) => (
                        <Cell key={entry.name} fill={entry.color ?? PALETTE[index % PALETTE.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      formatter={(v, _n, props) => {
                        const pct = total > 0 ? ((Number(v) / total) * 100).toFixed(1) : '0'
                        return [`${fmt(Number(v ?? 0), sym)}  (${pct}%)`, props.payload?.name ?? '']
                      }}
                      contentStyle={tooltipStyle}
                    />
                  </PieChart>
                  <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
                    <span className="text-[10px] text-gray-400 dark:text-gray-500 uppercase tracking-wide">Total</span>
                    <span className="text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{fmt(total, sym)}</span>
                  </div>
                </div>
                {/* Legend */}
                <div className="flex-1 min-w-0 space-y-2">
                  {categoryData.map((entry, index) => {
                    const pct = total > 0 ? Math.round((entry.value / total) * 100) : 0
                    return (
                      <div key={entry.name} className="flex items-center gap-2">
                        <span className="w-2.5 h-2.5 rounded-sm flex-shrink-0" style={{ backgroundColor: entry.color ?? PALETTE[index % PALETTE.length] }} />
                        <span className="flex-1 text-xs text-gray-600 dark:text-gray-400 truncate">{entry.name}</span>
                        <span className="text-xs text-gray-400 dark:text-gray-500 flex-shrink-0 w-8 text-right">{pct}%</span>
                        <span className="text-xs font-medium text-gray-700 dark:text-gray-300 flex-shrink-0 w-20 text-right">{fmt(entry.value, sym)}</span>
                      </div>
                    )
                  })}
                </div>
              </div>
            )
          })()}
        </div>

        {/* Bar: Monthly Cash Flow */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
          <div className="flex items-start justify-between mb-0.5">
            <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">Monthly Cash Flow</h2>
            <PeriodTabs options={MONTH_OPTIONS} value={cashFlowMonths} onChange={setCashFlowMonths} format={monthLabel} />
          </div>
          <p className="text-xs text-gray-400 dark:text-gray-500 mb-4">
            Income vs Expenses — last {cashFlowMonths === 12 ? 'year' : `${cashFlowMonths} month${cashFlowMonths > 1 ? 's' : ''}`}
          </p>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={barData} barCategoryGap="30%">
              <CartesianGrid strokeDasharray="3 3" stroke={gridColor} vertical={false} />
              <XAxis
                dataKey="month"
                tick={{ fontSize: 12, fill: tickColor }}
                axisLine={false}
                tickLine={false}
              />
              <YAxis
                tickFormatter={v => v >= 1000 ? `${sym}${(v / 1000).toFixed(0)}k` : `${sym}${v}`}
                tick={{ fontSize: 11, fill: tickColor }}
                axisLine={false}
                tickLine={false}
              />
              <Tooltip
                formatter={(v, name) => [fmt(Number(v ?? 0), sym), String(name)]}
                contentStyle={tooltipStyle}
                cursor={{ fill: dark ? 'rgba(255,255,255,0.04)' : 'rgba(0,0,0,0.04)' }}
              />
              <Legend
                formatter={value => <span style={{ color: tickColor, fontSize: 12 }}>{value}</span>}
              />
              <Bar dataKey="Income"   fill="#10b981" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Expenses" fill="#f43f5e" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

      </div>
    </div>
  )
}
