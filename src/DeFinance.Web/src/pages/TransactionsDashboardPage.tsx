import { useEffect, useMemo, useRef, useState } from 'react'
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
} from 'recharts'
import { transactionsApi, type Transaction } from '../api/transactions'
import { categoriesApi, type Category } from '../api/categories'
import { useMainCurrency } from '../MainCurrencyContext'
import { useTheme } from '../ThemeContext'
import { Spinner } from '../components/Spinner'

const PALETTE = ['#6366f1','#f59e0b','#10b981','#ef4444','#3b82f6','#8b5cf6','#ec4899','#14b8a6','#f97316','#84cc16']

const MONTH_OPTIONS = [3, 6, 12, 24] as const
type MonthOption = typeof MONTH_OPTIONS[number]

function getLastNMonths(n: number) {
  const now = new Date()
  return Array.from({ length: n }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - (n - 1 - i), 1)
    return {
      year: d.getFullYear(),
      month: d.getMonth(),
      label: d.toLocaleDateString('en-US', { month: 'short', year: n > 6 ? '2-digit' : undefined }),
    }
  })
}

function fmt(value: number, symbol: string) {
  return `${symbol}${value.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`
}

function getCatColor(cat: Category, allCats: Category[]) {
  if (cat.color) return cat.color
  const idx = allCats.findIndex(c => c.id === cat.id)
  return PALETTE[idx % PALETTE.length]
}

export function TransactionsDashboardPage() {
  const { mainCurrency } = useMainCurrency()
  const { dark } = useTheme()
  const sym = mainCurrency?.symbol ?? '€'

  const [categories, setCategories] = useState<Category[]>([])
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loading, setLoading] = useState(false)
  const [catsLoading, setCatsLoading] = useState(true)
  const [months, setMonths] = useState<MonthOption>(6)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [pickerOpen, setPickerOpen] = useState(false)
  const pickerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    categoriesApi.getAll({ pageSize: 500 })
      .then(r => setCategories(r.items.filter(c => c.type === 'Income' || c.type === 'Expense')))
      .finally(() => setCatsLoading(false))
  }, [])

  useEffect(() => {
    const d = new Date()
    d.setMonth(d.getMonth() - months)
    const dateFrom = d.toISOString().split('T')[0]
    setLoading(true)

    const fetchAll = async (): Promise<Transaction[]> => {
      const first = await transactionsApi.getAll({ dateFrom, pageSize: 500, page: 1 })
      if (!first.hasNextPage) return first.items
      const rest = await Promise.all(
        Array.from({ length: first.totalPages - 1 }, (_, i) =>
          transactionsApi.getAll({ dateFrom, pageSize: 500, page: i + 2 })
        )
      )
      return [...first.items, ...rest.flatMap(r => r.items)]
    }

    fetchAll().then(setTransactions).catch(() => {}).finally(() => setLoading(false))
  }, [months])

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (pickerRef.current && !pickerRef.current.contains(e.target as Node))
        setPickerOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  const monthSlots = useMemo(() => getLastNMonths(months), [months])

  const barData = useMemo(() => {
    return monthSlots.map(slot => {
      const row: Record<string, number | string> = { month: slot.label }
      for (const id of selectedIds) {
        const cat = categories.find(c => c.id === id)
        if (!cat) continue
        row[cat.name] = transactions
          .filter(t => {
            const d = new Date(t.dateTime)
            return t.categoryId === id && d.getFullYear() === slot.year && d.getMonth() === slot.month
          })
          .reduce((s, t) => s + t.amountInCurrency, 0)
      }
      return row
    })
  }, [transactions, selectedIds, monthSlots, categories])

  const incomeCategories = categories.filter(c => c.type === 'Income')
  const expenseCategories = categories.filter(c => c.type === 'Expense')
  const selectedCategories = categories.filter(c => selectedIds.has(c.id))

  const toggleCategory = (id: string) =>
    setSelectedIds(prev => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })

  const gridColor    = dark ? '#374151' : '#e5e7eb'
  const tickColor    = dark ? '#9ca3af' : '#6b7280'
  const tooltipStyle = {
    backgroundColor: dark ? '#1f2937' : '#fff',
    borderColor:     dark ? '#374151' : '#e5e7eb',
    color:           dark ? '#f3f4f6' : '#111827',
    borderRadius: 8,
    fontSize: 12,
  }

  return (
    <div className="p-6 space-y-5 overflow-y-auto h-full">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Transactions Dashboard</h1>

      {/* Controls row */}
      <div className="flex items-center gap-3 flex-wrap">

        {/* Category picker */}
        <div className="relative" ref={pickerRef}>
          <button
            onClick={() => setPickerOpen(o => !o)}
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-sm text-gray-600 dark:text-gray-300 hover:border-indigo-400 dark:hover:border-indigo-500 transition-colors"
          >
            {catsLoading
              ? <Spinner size="sm" />
              : <span>{selectedIds.size === 0 ? 'Select categories…' : `${selectedIds.size} selected`}</span>
            }
            <span className="text-gray-400 text-xs">▾</span>
          </button>

          {pickerOpen && (
            <div className="absolute top-full mt-1 left-0 z-30 w-60 rounded-xl border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 shadow-xl overflow-hidden">
              <div className="max-h-72 overflow-y-auto">
                {[
                  { label: 'Income', items: incomeCategories },
                  { label: 'Expense', items: expenseCategories },
                ].map(group => group.items.length === 0 ? null : (
                  <div key={group.label}>
                    <div className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 bg-gray-50 dark:bg-gray-700/60 sticky top-0">
                      {group.label}
                    </div>
                    {group.items.map(cat => (
                      <button
                        key={cat.id}
                        onClick={() => toggleCategory(cat.id)}
                        className="flex items-center gap-2.5 w-full px-3 py-2 text-sm text-left hover:bg-gray-50 dark:hover:bg-gray-700/60 transition-colors"
                      >
                        <span
                          className="w-2.5 h-2.5 rounded-sm flex-shrink-0"
                          style={{ backgroundColor: getCatColor(cat, categories) }}
                        />
                        <span className="flex-1 text-gray-700 dark:text-gray-300 truncate">{cat.name}</span>
                        {selectedIds.has(cat.id) && (
                          <span className="text-indigo-500 dark:text-indigo-400 font-bold text-xs">✓</span>
                        )}
                      </button>
                    ))}
                  </div>
                ))}
              </div>
              {selectedIds.size > 0 && (
                <div className="px-3 py-2 border-t border-gray-100 dark:border-gray-700 bg-white dark:bg-gray-800">
                  <button
                    onClick={() => setSelectedIds(new Set())}
                    className="text-xs text-gray-400 hover:text-red-500 transition-colors"
                  >
                    Clear all
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Selected category chips */}
        {selectedCategories.map(cat => (
          <span
            key={cat.id}
            className="flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium text-white"
            style={{ backgroundColor: getCatColor(cat, categories) }}
          >
            {cat.name}
            <button
              onClick={() => toggleCategory(cat.id)}
              className="opacity-70 hover:opacity-100 leading-none"
            >
              ✕
            </button>
          </span>
        ))}

        {/* Period tabs */}
        <div className="flex gap-0.5 ml-auto">
          {MONTH_OPTIONS.map(opt => (
            <button
              key={opt}
              onClick={() => setMonths(opt)}
              className={`px-2.5 py-1 text-xs rounded transition-colors ${
                months === opt
                  ? 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/60 dark:text-indigo-300 font-medium'
                  : 'text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300'
              }`}
            >
              {opt === 12 ? '1Y' : opt === 24 ? '2Y' : `${opt}M`}
            </button>
          ))}
        </div>
      </div>

      {/* Chart card */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
        {loading ? (
          <div className="flex items-center justify-center h-80">
            <Spinner />
          </div>
        ) : selectedIds.size === 0 ? (
          <div className="flex flex-col items-center justify-center h-80 gap-3 text-gray-400 dark:text-gray-500">
            <span className="text-4xl">📊</span>
            <span className="text-sm">Select one or more categories above to see the monthly trend</span>
          </div>
        ) : (
          <ResponsiveContainer width="100%" height={380}>
            <BarChart data={barData} barCategoryGap="28%" barGap={3}>
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
              {selectedCategories.map(cat => (
                <Bar
                  key={cat.id}
                  dataKey={cat.name}
                  fill={getCatColor(cat, categories)}
                  radius={[4, 4, 0, 0]}
                />
              ))}
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>
    </div>
  )
}
