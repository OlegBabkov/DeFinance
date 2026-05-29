import { useEffect, useRef, useState } from 'react'
import { useNotify } from '../NotificationContext'
import { planFactApi, type PlanFactCategoryRow, type PlanFactMonthData, type PlanFactSummaryResponse } from '../api/planFact'

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

const now = new Date()
const CURRENT_YEAR = now.getFullYear()
const CURRENT_MONTH = now.getMonth() + 1

const fmt = (n: number) =>
  n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

const pct = (plan: number, fact: number) =>
  plan === 0 ? null : Math.round((fact / plan) * 100)

interface EditState { categoryId: string; year: number; month: number; value: string }

interface MonthTotals {
  openingBalance: number
  incomePlan: number
  incomeFact: number
  expensePlan: number
  expenseFact: number
}

function calcMonthTotals(m: PlanFactMonthData): MonthTotals {
  const incomePlan = m.incomeCategories.reduce((s, c) => s + c.plan, 0)
  const incomeFact = m.incomeCategories.reduce((s, c) => s + c.fact, 0)
  const expensePlan = m.expenseCategories.reduce((s, c) => s + c.plan, 0)
  const expenseFact = m.expenseCategories.reduce((s, c) => s + c.fact, 0)
  return { openingBalance: m.openingBalance, incomePlan, incomeFact, expensePlan, expenseFact }
}

function PctCell({ plan, fact, isExpense }: { plan: number; fact: number; isExpense?: boolean }) {
  const p = pct(plan, fact)
  if (p === null) return <td className="px-2 py-2 text-center text-gray-400 text-xs">—</td>
  const good = isExpense ? p <= 100 : p >= 100
  const warn = isExpense ? p <= 120 : p >= 80
  const cls = good
    ? 'text-emerald-600 dark:text-emerald-400'
    : warn
    ? 'text-amber-500 dark:text-amber-400'
    : 'text-red-500 dark:text-red-400'
  return (
    <td className={`px-2 py-2 text-right text-xs font-medium ${cls}`}>
      {p}%
    </td>
  )
}

function PlanCell({
  value,
  categoryId,
  year,
  month,
  editing,
  onStartEdit,
  onChangeEdit,
  onCommit,
}: {
  value: number
  categoryId: string
  year: number
  month: number
  editing: EditState | null
  onStartEdit: (e: EditState) => void
  onChangeEdit: (v: string) => void
  onCommit: () => void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const isEditing =
    editing?.categoryId === categoryId && editing?.year === year && editing?.month === month

  useEffect(() => {
    if (isEditing) inputRef.current?.select()
  }, [isEditing])

  if (isEditing) {
    return (
      <td className="px-1 py-1">
        <input
          ref={inputRef}
          type="number"
          min="0"
          step="0.01"
          value={editing!.value}
          onChange={e => onChangeEdit(e.target.value)}
          onBlur={onCommit}
          onKeyDown={e => { if (e.key === 'Enter') onCommit(); if (e.key === 'Escape') onCommit() }}
          className="w-20 text-right text-xs px-1 py-0.5 rounded border border-indigo-400 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500"
        />
      </td>
    )
  }

  return (
    <td
      className="px-2 py-2 text-right text-xs text-gray-500 dark:text-gray-400 cursor-pointer hover:bg-indigo-50 dark:hover:bg-indigo-900/20 hover:text-indigo-700 dark:hover:text-indigo-400 select-none"
      title="Click to set plan"
      onClick={() => onStartEdit({ categoryId, year, month, value: value === 0 ? '' : value.toFixed(2) })}
    >
      {value === 0 ? <span className="text-gray-300 dark:text-gray-600">+</span> : fmt(value)}
    </td>
  )
}

export function PlanFactPage() {
  const notify = useNotify()
  const [year, setYear] = useState(CURRENT_YEAR)
  const [selectedMonths, setSelectedMonths] = useState<number[]>([CURRENT_MONTH])
  const [data, setData] = useState<PlanFactSummaryResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [editing, setEditing] = useState<EditState | null>(null)
  const [hideEmpty, setHideEmpty] = useState(false)

  const orderedMonths = [...selectedMonths].sort((a, b) => a - b)
  const showTotal = orderedMonths.length > 1

  const fetchData = () => {
    if (orderedMonths.length === 0) return
    setLoading(true)
    planFactApi.getSummary(year, orderedMonths)
      .then(setData)
      .catch(() => notify('Failed to load plan/fact data', 'error'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchData() }, [year, JSON.stringify(orderedMonths)])

  const toggleMonth = (m: number) =>
    setSelectedMonths(prev =>
      prev.includes(m) ? (prev.length > 1 ? prev.filter(x => x !== m) : prev) : [...prev, m]
    )

  const handleStartEdit = (e: EditState) => setEditing(e)
  const handleChangeEdit = (v: string) => setEditing(prev => prev ? { ...prev, value: v } : prev)

  const handleCommit = async () => {
    if (!editing) return
    const amount = parseFloat(editing.value) || 0
    const { categoryId, year: y, month: m } = editing
    setEditing(null)

    // Optimistic update
    setData(prev => {
      if (!prev) return prev
      return {
        months: prev.months.map(md =>
          md.year === y && md.month === m
            ? {
                ...md,
                incomeCategories: md.incomeCategories.map(c =>
                  c.categoryId === categoryId ? { ...c, plan: amount } : c
                ),
                expenseCategories: md.expenseCategories.map(c =>
                  c.categoryId === categoryId ? { ...c, plan: amount } : c
                ),
              }
            : md
        ),
      }
    })

    try {
      await planFactApi.upsertEntry(categoryId, y, m, amount)
    } catch {
      notify('Failed to save plan value', 'error')
      fetchData()
    }
  }

  const getMonthData = (month: number) => data?.months.find(m => m.month === month)

  // Compute combined totals across all selected months
  const combined = (() => {
    if (!data || orderedMonths.length === 0) return null
    const allMonths = orderedMonths.map(m => getMonthData(m)).filter(Boolean) as PlanFactMonthData[]
    if (allMonths.length === 0) return null

    const allCategories = new Map<string, { name: string; isExpense: boolean; planByMonth: Map<number, number>; factByMonth: Map<number, number> }>()

    for (const md of allMonths) {
      for (const c of md.incomeCategories) {
        if (!allCategories.has(c.categoryId)) allCategories.set(c.categoryId, { name: c.categoryName, isExpense: false, planByMonth: new Map(), factByMonth: new Map() })
        const entry = allCategories.get(c.categoryId)!
        entry.planByMonth.set(md.month, c.plan)
        entry.factByMonth.set(md.month, c.fact)
      }
      for (const c of md.expenseCategories) {
        if (!allCategories.has(c.categoryId)) allCategories.set(c.categoryId, { name: c.categoryName, isExpense: true, planByMonth: new Map(), factByMonth: new Map() })
        const entry = allCategories.get(c.categoryId)!
        entry.planByMonth.set(md.month, c.plan)
        entry.factByMonth.set(md.month, c.fact)
      }
    }

    const firstOpening = allMonths[0].openingBalance
    const totals = Array.from(allCategories.entries()).map(([id, v]) => ({
      categoryId: id,
      categoryName: v.name,
      isExpense: v.isExpense,
      plan: Array.from(v.planByMonth.values()).reduce((s, x) => s + x, 0),
      fact: Array.from(v.factByMonth.values()).reduce((s, x) => s + x, 0),
    }))

    return { firstOpening, categories: totals }
  })()

  const colGroups = orderedMonths.length + (showTotal ? 1 : 0)
  const totalCols = 1 + colGroups * 3

  const getCatForMonth = (month: number, id: string, isExpense: boolean): PlanFactCategoryRow | undefined => {
    const md = getMonthData(month)
    return isExpense ? md?.expenseCategories.find(c => c.categoryId === id) : md?.incomeCategories.find(c => c.categoryId === id)
  }

  // Collect all income/expense category IDs in display order from data
  const allIncomeIds: string[] = data?.months[0]?.incomeCategories.map(c => c.categoryId) ?? []
  const allExpenseIds: string[] = data?.months[0]?.expenseCategories.map(c => c.categoryId) ?? []

  const hasAnyValue = (id: string, isExpense: boolean) =>
    orderedMonths.some(m => {
      const c = getCatForMonth(m, id, isExpense)
      return (c?.plan ?? 0) > 0 || (c?.fact ?? 0) > 0
    })

  const incomeIds = hideEmpty ? allIncomeIds.filter(id => hasAnyValue(id, false)) : allIncomeIds
  const expenseIds = hideEmpty ? allExpenseIds.filter(id => hasAnyValue(id, true)) : allExpenseIds

  const getIncomeName = (id: string) => data?.months[0]?.incomeCategories.find(c => c.categoryId === id)?.categoryName ?? ''
  const getExpenseName = (id: string) => data?.months[0]?.expenseCategories.find(c => c.categoryId === id)?.categoryName ?? ''

  const monthTotals = (month: number): MonthTotals | null => {
    const md = getMonthData(month)
    return md ? calcMonthTotals(md) : null
  }

  const sectionHdrCls = 'px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-750'
  const totalRowCls = 'bg-gray-50 dark:bg-gray-800/80 font-medium'
  const balRowCls = 'bg-indigo-50/60 dark:bg-indigo-900/20 font-semibold'

  const signedColor = (v: number) =>
    v > 0 ? 'text-emerald-600 dark:text-emerald-400' : v < 0 ? 'text-red-500 dark:text-red-400' : ''

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="px-8 pt-8 pb-4 shrink-0">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-4">Plan / Fact</h1>

        <div className="flex items-center gap-4 flex-wrap">
          {/* Year picker */}
          <div className="flex items-center gap-1">
            <button
              onClick={() => setYear(y => y - 1)}
              className="p-1 rounded text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              ‹
            </button>
            <span className="text-sm font-medium text-gray-800 dark:text-gray-200 w-12 text-center">{year}</span>
            <button
              onClick={() => setYear(y => y + 1)}
              className="p-1 rounded text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              ›
            </button>
          </div>

          {/* Month chips */}
          <div className="flex gap-1 flex-wrap">
            {MONTH_NAMES.map((name, i) => {
              const m = i + 1
              const active = selectedMonths.includes(m)
              return (
                <button
                  key={m}
                  onClick={() => toggleMonth(m)}
                  className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                    active
                      ? 'bg-indigo-600 text-white'
                      : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-600'
                  }`}
                >
                  {name}
                </button>
              )
            })}
          </div>

          <button
            onClick={() => setHideEmpty(h => !h)}
            className={`px-3 py-1.5 rounded-lg border text-xs font-medium transition-colors ${
              hideEmpty
                ? 'border-indigo-400 bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300'
                : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-600 dark:text-gray-400 hover:border-gray-400'
            }`}
          >
            {hideEmpty ? 'Show all categories' : 'Hide empty categories'}
          </button>

          {loading && <span className="text-xs text-gray-400">Loading…</span>}
        </div>
      </div>

      {/* Table */}
      {orderedMonths.length > 0 && (
        <div className="flex-1 min-h-0 overflow-auto mx-8 mb-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
          <table className="text-sm border-collapse">
            <thead className="sticky top-0 z-10">
              {/* Row 1: month group headers */}
              <tr className="bg-gray-50 dark:bg-gray-700 border-b border-gray-200 dark:border-gray-600">
                <th className="px-4 py-2 text-left font-medium text-gray-500 dark:text-gray-400 w-48 min-w-[12rem] sticky left-0 bg-gray-50 dark:bg-gray-700 z-20 border-r border-gray-200 dark:border-gray-600">
                  Category
                </th>
                {orderedMonths.map(m => (
                  <th key={m} colSpan={3} className="px-3 py-2 text-center font-semibold text-gray-700 dark:text-gray-200 border-l border-gray-200 dark:border-gray-600 min-w-[14rem]">
                    {MONTH_NAMES[m - 1]} {year}
                  </th>
                ))}
                {showTotal && (
                  <th colSpan={3} className="px-3 py-2 text-center font-semibold text-indigo-700 dark:text-indigo-300 border-l border-gray-200 dark:border-gray-600 min-w-[14rem]">
                    Total
                  </th>
                )}
              </tr>
              {/* Row 2: Plan / Fact / % sub-headers */}
              <tr className="bg-gray-50 dark:bg-gray-700 border-b border-gray-200 dark:border-gray-600 text-xs text-gray-400 dark:text-gray-500">
                <th className="sticky left-0 bg-gray-50 dark:bg-gray-700 z-20 border-r border-gray-200 dark:border-gray-600" />
                {Array.from({ length: colGroups }).map((_, gi) => (
                  <>
                    <th key={`${gi}-plan`} className="px-2 py-1 text-right font-medium border-l border-gray-100 dark:border-gray-700 w-24">Plan</th>
                    <th key={`${gi}-fact`} className="px-2 py-1 text-right font-medium w-24">Fact</th>
                    <th key={`${gi}-pct`}  className="px-2 py-1 text-right font-medium w-16">%</th>
                  </>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">

              {/* Opening Balance */}
              <tr className={balRowCls}>
                <td className="px-4 py-2.5 text-gray-700 dark:text-gray-200 sticky left-0 bg-inherit z-10 border-r border-gray-200 dark:border-gray-600 whitespace-nowrap">
                  Opening Balance
                </td>
                {orderedMonths.map(m => {
                  const ob = getMonthData(m)?.openingBalance ?? 0
                  return (
                    <>
                      <td key={`${m}-ob-plan`} className="px-2 py-2.5 text-right text-gray-400 text-xs border-l border-gray-100 dark:border-gray-700">—</td>
                      <td key={`${m}-ob-fact`} className={`px-2 py-2.5 text-right text-xs font-mono ${signedColor(ob)}`}>{fmt(ob)}</td>
                      <td key={`${m}-ob-pct`}  className="px-2 py-2.5 text-center text-gray-400 text-xs">—</td>
                    </>
                  )
                })}
                {showTotal && combined && (
                  <>
                    <td className="px-2 py-2.5 text-right text-gray-400 text-xs border-l border-gray-100 dark:border-gray-700">—</td>
                    <td className={`px-2 py-2.5 text-right text-xs font-mono ${signedColor(combined.firstOpening)}`}>{fmt(combined.firstOpening)}</td>
                    <td className="px-2 py-2.5 text-center text-gray-400 text-xs">—</td>
                  </>
                )}
              </tr>

              {/* Income section header */}
              <tr>
                <td colSpan={totalCols} className={sectionHdrCls}>Income</td>
              </tr>

              {/* Income category rows */}
              {incomeIds.map(id => (
                <tr key={id} className="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <td className="px-4 py-2 text-gray-700 dark:text-gray-300 sticky left-0 bg-white dark:bg-gray-800 z-10 border-r border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700/50">
                    {getIncomeName(id)}
                  </td>
                  {orderedMonths.map(m => {
                    const c = getCatForMonth(m, id, false)
                    const plan = c?.plan ?? 0
                    const fact = c?.fact ?? 0
                    return (
                      <>
                        <PlanCell key={`${m}-${id}-plan`} value={plan} categoryId={id} year={year} month={m} editing={editing} onStartEdit={handleStartEdit} onChangeEdit={handleChangeEdit} onCommit={handleCommit} />
                        <td key={`${m}-${id}-fact`} className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{fact > 0 ? fmt(fact) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell key={`${m}-${id}-pct`} plan={plan} fact={fact} />
                      </>
                    )
                  })}
                  {showTotal && combined && (() => {
                    const ct = combined.categories.find(c => c.categoryId === id)
                    const p = ct?.plan ?? 0
                    const f = ct?.fact ?? 0
                    return (
                      <>
                        <td className="px-2 py-2 text-right text-xs text-gray-500 dark:text-gray-400 border-l border-gray-100 dark:border-gray-700 font-mono">{p > 0 ? fmt(p) : '—'}</td>
                        <td className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{f > 0 ? fmt(f) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell plan={p} fact={f} />
                      </>
                    )
                  })()}
                </tr>
              ))}

              {/* Total Income */}
              <tr className={totalRowCls}>
                <td className="px-4 py-2 text-gray-800 dark:text-gray-100 sticky left-0 bg-gray-50 dark:bg-gray-800/80 z-10 border-r border-gray-200 dark:border-gray-600">
                  Total Income
                </td>
                {orderedMonths.map(m => {
                  const t = monthTotals(m)
                  return (
                    <>
                      <td key={`${m}-ti-plan`} className="px-2 py-2 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 text-gray-700 dark:text-gray-300">{t ? fmt(t.incomePlan) : '—'}</td>
                      <td key={`${m}-ti-fact`} className="px-2 py-2 text-right text-xs font-mono text-emerald-600 dark:text-emerald-400">{t ? fmt(t.incomeFact) : '—'}</td>
                      <PctCell key={`${m}-ti-pct`} plan={t?.incomePlan ?? 0} fact={t?.incomeFact ?? 0} />
                    </>
                  )
                })}
                {showTotal && combined && (() => {
                  const p = combined.categories.filter(c => !c.isExpense).reduce((s, c) => s + c.plan, 0)
                  const f = combined.categories.filter(c => !c.isExpense).reduce((s, c) => s + c.fact, 0)
                  return (
                    <>
                      <td className="px-2 py-2 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 text-gray-700 dark:text-gray-300">{fmt(p)}</td>
                      <td className="px-2 py-2 text-right text-xs font-mono text-emerald-600 dark:text-emerald-400">{fmt(f)}</td>
                      <PctCell plan={p} fact={f} />
                    </>
                  )
                })()}
              </tr>

              {/* Expense section header */}
              <tr>
                <td colSpan={totalCols} className={sectionHdrCls}>Losses</td>
              </tr>

              {/* Expense category rows */}
              {expenseIds.map(id => (
                <tr key={id} className="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <td className="px-4 py-2 text-gray-700 dark:text-gray-300 sticky left-0 bg-white dark:bg-gray-800 z-10 border-r border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700/50">
                    {getExpenseName(id)}
                  </td>
                  {orderedMonths.map(m => {
                    const c = getCatForMonth(m, id, true)
                    const plan = c?.plan ?? 0
                    const fact = c?.fact ?? 0
                    return (
                      <>
                        <PlanCell key={`${m}-${id}-plan`} value={plan} categoryId={id} year={year} month={m} editing={editing} onStartEdit={handleStartEdit} onChangeEdit={handleChangeEdit} onCommit={handleCommit} />
                        <td key={`${m}-${id}-fact`} className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{fact > 0 ? fmt(fact) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell key={`${m}-${id}-pct`} plan={plan} fact={fact} isExpense />
                      </>
                    )
                  })}
                  {showTotal && combined && (() => {
                    const ct = combined.categories.find(c => c.categoryId === id)
                    const p = ct?.plan ?? 0
                    const f = ct?.fact ?? 0
                    return (
                      <>
                        <td className="px-2 py-2 text-right text-xs text-gray-500 dark:text-gray-400 border-l border-gray-100 dark:border-gray-700 font-mono">{p > 0 ? fmt(p) : '—'}</td>
                        <td className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{f > 0 ? fmt(f) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell plan={p} fact={f} isExpense />
                      </>
                    )
                  })()}
                </tr>
              ))}

              {/* Total Losses */}
              <tr className={totalRowCls}>
                <td className="px-4 py-2 text-gray-800 dark:text-gray-100 sticky left-0 bg-gray-50 dark:bg-gray-800/80 z-10 border-r border-gray-200 dark:border-gray-600">
                  Total Losses
                </td>
                {orderedMonths.map(m => {
                  const t = monthTotals(m)
                  return (
                    <>
                      <td key={`${m}-tl-plan`} className="px-2 py-2 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 text-gray-700 dark:text-gray-300">{t ? fmt(t.expensePlan) : '—'}</td>
                      <td key={`${m}-tl-fact`} className="px-2 py-2 text-right text-xs font-mono text-red-500 dark:text-red-400">{t ? fmt(t.expenseFact) : '—'}</td>
                      <PctCell key={`${m}-tl-pct`} plan={t?.expensePlan ?? 0} fact={t?.expenseFact ?? 0} isExpense />
                    </>
                  )
                })}
                {showTotal && combined && (() => {
                  const p = combined.categories.filter(c => c.isExpense).reduce((s, c) => s + c.plan, 0)
                  const f = combined.categories.filter(c => c.isExpense).reduce((s, c) => s + c.fact, 0)
                  return (
                    <>
                      <td className="px-2 py-2 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 text-gray-700 dark:text-gray-300">{fmt(p)}</td>
                      <td className="px-2 py-2 text-right text-xs font-mono text-red-500 dark:text-red-400">{fmt(f)}</td>
                      <PctCell plan={p} fact={f} isExpense />
                    </>
                  )
                })()}
              </tr>

              {/* Closing Balance */}
              <tr className={balRowCls}>
                <td className="px-4 py-2.5 text-gray-700 dark:text-gray-200 sticky left-0 bg-inherit z-10 border-r border-gray-200 dark:border-gray-600 whitespace-nowrap">
                  Closing Balance
                </td>
                {orderedMonths.map(m => {
                  const t = monthTotals(m)
                  if (!t) return (
                    <>
                      <td key={`${m}-cb-plan`} className="px-2 py-2.5 text-right text-xs border-l border-gray-100 dark:border-gray-700">—</td>
                      <td key={`${m}-cb-fact`} className="px-2 py-2.5 text-right text-xs">—</td>
                      <td key={`${m}-cb-pct`}  className="px-2 py-2.5 text-right text-xs">—</td>
                    </>
                  )
                  const planClose = t.openingBalance + t.incomePlan - t.expensePlan
                  const factClose = t.openingBalance + t.incomeFact - t.expenseFact
                  return (
                    <>
                      <td key={`${m}-cb-plan`} className={`px-2 py-2.5 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 ${signedColor(planClose)}`}>{fmt(planClose)}</td>
                      <td key={`${m}-cb-fact`} className={`px-2 py-2.5 text-right text-xs font-mono ${signedColor(factClose)}`}>{fmt(factClose)}</td>
                      <PctCell key={`${m}-cb-pct`} plan={planClose} fact={factClose} />
                    </>
                  )
                })}
                {showTotal && combined && (() => {
                  const incomePlan = combined.categories.filter(c => !c.isExpense).reduce((s, c) => s + c.plan, 0)
                  const incomeFact = combined.categories.filter(c => !c.isExpense).reduce((s, c) => s + c.fact, 0)
                  const expensePlan = combined.categories.filter(c => c.isExpense).reduce((s, c) => s + c.plan, 0)
                  const expenseFact = combined.categories.filter(c => c.isExpense).reduce((s, c) => s + c.fact, 0)
                  const planClose = combined.firstOpening + incomePlan - expensePlan
                  const factClose = combined.firstOpening + incomeFact - expenseFact
                  return (
                    <>
                      <td className={`px-2 py-2.5 text-right text-xs font-mono border-l border-gray-100 dark:border-gray-700 ${signedColor(planClose)}`}>{fmt(planClose)}</td>
                      <td className={`px-2 py-2.5 text-right text-xs font-mono ${signedColor(factClose)}`}>{fmt(factClose)}</td>
                      <PctCell plan={planClose} fact={factClose} />
                    </>
                  )
                })()}
              </tr>

            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
