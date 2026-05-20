import { useEffect, useMemo, useState } from 'react'
import {
  PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
} from 'recharts'
import { transactionsApi, type Transaction } from '../api/transactions'
import { accountsApi, type Account } from '../api/accounts'
import { useMainCurrency } from '../MainCurrencyContext'
import { useTheme } from '../ThemeContext'

const PALETTE = ['#6366f1','#f59e0b','#10b981','#ef4444','#3b82f6','#8b5cf6','#ec4899','#14b8a6','#f97316','#84cc16']

function fmt(value: number, symbol: string) {
  return `${symbol}${value.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`
}

function getLast6Months() {
  const now = new Date()
  return Array.from({ length: 6 }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - (5 - i), 1)
    return { year: d.getFullYear(), month: d.getMonth(), label: d.toLocaleDateString('en-US', { month: 'short' }) }
  })
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

export function DashboardPage() {
  const { mainCurrency } = useMainCurrency()
  const { dark } = useTheme()
  const sym = mainCurrency?.symbol ?? '€'

  const [accounts, setAccounts] = useState<Account[]>([])
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const sixMonthsAgo = new Date()
    sixMonthsAgo.setMonth(sixMonthsAgo.getMonth() - 6)
    const dateFrom = sixMonthsAgo.toISOString().split('T')[0]

    Promise.all([
      accountsApi.getAll({ isActive: true, pageSize: 100 }),
      transactionsApi.getAll({ dateFrom, pageSize: 500 }),
    ]).then(([accs, txns]) => {
      setAccounts(accs.items)
      setTransactions(txns.items)
    }).catch(() => setError('Failed to load dashboard data.'))
      .finally(() => setLoading(false))
  }, [])

  const now = new Date()
  const thisMonthStart = new Date(now.getFullYear(), now.getMonth(), 1)

  const thisMonthTx = transactions.filter(t => new Date(t.dateTime) >= thisMonthStart)
  const monthIncome   = thisMonthTx.filter(t => t.category?.type === 'Income').reduce((s, t) => s + t.amountInCurrency, 0)
  const monthExpenses = thisMonthTx.filter(t => t.category?.type === 'Expense').reduce((s, t) => s + t.amountInCurrency, 0)
  const monthNet      = monthIncome - monthExpenses

  const uniqueCurrencies = [...new Set(accounts.map(a => a.currency?.code).filter(Boolean))].join(' · ')

  const categoryData = useMemo(() => {
    const thirtyDaysAgo = new Date()
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30)
    const map: Record<string, { name: string; value: number; color: string | null }> = {}
    transactions
      .filter(t => t.category?.type === 'Expense' && new Date(t.dateTime) >= thirtyDaysAgo)
      .forEach(t => {
        const key = t.category?.id ?? 'unknown'
        if (!map[key]) map[key] = { name: t.category?.name ?? 'Unknown', value: 0, color: t.category?.color ?? null }
        map[key].value += t.amountInCurrency
      })
    return Object.values(map).sort((a, b) => b.value - a.value)
  }, [transactions])

  const barData = useMemo(() => {
    const months = getLast6Months()
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
  }, [transactions])

  const gridColor = dark ? '#374151' : '#e5e7eb'
  const tickColor  = dark ? '#9ca3af' : '#6b7280'
  const tooltipStyle = {
    backgroundColor: dark ? '#1f2937' : '#fff',
    borderColor: dark ? '#374151' : '#e5e7eb',
    color: dark ? '#f3f4f6' : '#111827',
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
      <div className="grid grid-cols-4 gap-4">
        <StatCard
          label="Active Accounts"
          value={String(accounts.length)}
          sub={uniqueCurrencies || undefined}
        />
        <StatCard
          label="Income This Month"
          value={fmt(monthIncome, sym)}
          valueColor="text-emerald-600 dark:text-emerald-400"
        />
        <StatCard
          label="Expenses This Month"
          value={fmt(monthExpenses, sym)}
          valueColor="text-rose-600 dark:text-rose-400"
        />
        <StatCard
          label="Net This Month"
          value={fmt(monthNet, sym)}
          valueColor={monthNet >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-600 dark:text-rose-400'}
          sub={monthNet >= 0 ? 'On track' : 'Over budget'}
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-2 gap-6">

        {/* Donut: Spending by Category */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
          <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-0.5">Spending by Category</h2>
          <p className="text-xs text-gray-400 dark:text-gray-500 mb-4">Last 30 days — expenses only</p>
          {categoryData.length === 0 ? (
            <div className="flex items-center justify-center h-64 text-gray-400 dark:text-gray-500 text-sm">
              No expense data for the last 30 days
            </div>
          ) : (
            <ResponsiveContainer width="100%" height={280}>
              <PieChart>
                <Pie
                  data={categoryData}
                  cx="50%"
                  cy="50%"
                  innerRadius={75}
                  outerRadius={115}
                  paddingAngle={2}
                  dataKey="value"
                >
                  {categoryData.map((entry, index) => (
                    <Cell key={entry.name} fill={entry.color ?? PALETTE[index % PALETTE.length]} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(v) => [fmt(Number(v ?? 0), sym), 'Amount']}
                  contentStyle={tooltipStyle}
                />
                <Legend
                  formatter={value => <span style={{ color: tickColor, fontSize: 12 }}>{value}</span>}
                />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>

        {/* Bar: Monthly Cash Flow */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
          <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-0.5">Monthly Cash Flow</h2>
          <p className="text-xs text-gray-400 dark:text-gray-500 mb-4">Income vs Expenses — last 6 months</p>
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
