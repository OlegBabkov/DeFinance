import { useEffect, useState } from 'react'
import { usePersistedState } from '../hooks/usePersistedState'
import { useNotify } from '../NotificationContext'
import { planFactApi, type PlanFactCategoryRow, type PlanFactMonthData, type PlanFactSummaryResponse } from '../api/planFact'
import { Spinner } from '../components/Spinner'
import { Modal } from '../components/Modal'
import { CalculatorModal } from '../components/CalculatorModal'
import { CalcIcon } from '../components/IconButton'

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

const now = new Date()
const CURRENT_YEAR = now.getFullYear()
const CURRENT_MONTH = now.getMonth() + 1

const fmt = (n: number) =>
  n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

const pct = (plan: number, fact: number) =>
  plan === 0 ? null : Math.round((fact / plan) * 100)

interface ModalState {
  categoryId: string
  categoryName: string
  year: number
  month: number
  value: string
  isExpense: boolean
}

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

function PctCell({ plan, fact, isExpense, showDiff }: { plan: number; fact: number; isExpense?: boolean; showDiff?: boolean }) {
  if (showDiff) {
    const diff = fact - plan
    if (plan === 0 && fact === 0) return <td className="px-2 py-2 text-center text-gray-400 text-xs">—</td>
    const good = isExpense ? diff <= 0 : diff >= 0
    const cls = diff === 0
      ? 'text-gray-500 dark:text-gray-400'
      : good
      ? 'text-emerald-600 dark:text-emerald-400'
      : 'text-red-500 dark:text-red-400'
    return (
      <td className={`px-2 py-2 text-right text-xs font-medium font-mono ${cls}`}>
        {diff > 0 ? '+' : ''}{fmt(diff)}
      </td>
    )
  }

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
  onClick,
}: {
  value: number
  onClick: () => void
}) {
  return (
    <td
      className="px-2 py-2 text-right text-xs text-gray-500 dark:text-gray-400 cursor-pointer hover:bg-indigo-50 dark:hover:bg-indigo-900/20 hover:text-indigo-700 dark:hover:text-indigo-400 select-none"
      title="Click to set plan"
      onClick={onClick}
    >
      {value === 0 ? <span className="text-gray-300 dark:text-gray-600">+</span> : fmt(value)}
    </td>
  )
}

interface PlanRow { id: number; name: string; amount: string }

let rowIdSeq = 0
const newRow = (): PlanRow => ({ id: ++rowIdSeq, name: '', amount: '' })

function PlanModal({
  state,
  onClose,
  onSave,
}: {
  state: ModalState
  onClose: () => void
  onSave: (value: string) => void
}) {
  const initialRows: PlanRow[] = state.value
    ? [{ id: ++rowIdSeq, name: '', amount: state.value }]
    : [newRow()]

  const [rows, setRows] = useState<PlanRow[]>(initialRows)
  const [calcRowId, setCalcRowId] = useState<number | null>(null)

  const total = rows.reduce((s, r) => s + (parseFloat(r.amount) || 0), 0)

  const updateRow = (id: number, field: 'name' | 'amount', value: string) =>
    setRows(prev => prev.map(r => r.id === id ? { ...r, [field]: value } : r))

  const removeRow = (id: number) =>
    setRows(prev => prev.length > 1 ? prev.filter(r => r.id !== id) : prev)

  const addRow = () => setRows(prev => [...prev, newRow()])

  const handleSubmit = (e: React.SyntheticEvent) => {
    e.preventDefault()
    onSave(total.toString())
  }

  const inputCls = 'px-2 py-1.5 rounded-md border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

  return (
    <Modal title={state.categoryName} onClose={onClose} wide>
      <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
        {MONTH_NAMES[state.month - 1]} {state.year}
      </p>
      <form onSubmit={handleSubmit}>
        {/* Column headers */}
        <div className="grid grid-cols-[1fr_10rem_2rem] gap-2 mb-1 px-1">
          <span className="text-xs font-medium text-gray-500 dark:text-gray-400">Name</span>
          <span className="text-xs font-medium text-gray-500 dark:text-gray-400 text-right">Amount</span>
          <span />
        </div>

        {/* Rows */}
        <div className="space-y-2 mb-3">
          {rows.map((row, i) => (
            <div key={row.id} className="grid grid-cols-[1fr_10rem_2rem] gap-2 items-center">
              <input
                type="text"
                value={row.name}
                onChange={e => updateRow(row.id, 'name', e.target.value)}
                placeholder={`Item ${i + 1}`}
                className={`${inputCls} w-full`}
                autoFocus={i === 0}
              />
              <div className="relative">
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={row.amount}
                  onChange={e => updateRow(row.id, 'amount', e.target.value)}
                  placeholder="0.00"
                  className={`${inputCls} w-full text-right pr-7 [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none`}
                />
                <button
                  type="button"
                  tabIndex={-1}
                  onClick={() => setCalcRowId(row.id)}
                  title="Open calculator"
                  className="absolute right-1.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                >
                  <CalcIcon />
                </button>
              </div>
              <button
                type="button"
                onClick={() => removeRow(row.id)}
                disabled={rows.length === 1}
                className="flex items-center justify-center w-8 h-8 rounded-md text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-25 disabled:cursor-not-allowed transition-colors"
              >
                ✕
              </button>
            </div>
          ))}
        </div>

        {/* Add row */}
        <button
          type="button"
          onClick={addRow}
          className="flex items-center gap-1.5 text-sm text-indigo-600 dark:text-indigo-400 hover:text-indigo-800 dark:hover:text-indigo-300 mb-5 transition-colors"
        >
          <span className="text-base leading-none">+</span> Add row
        </button>

        {/* Total */}
        <div className="flex justify-end items-center gap-3 border-t border-gray-200 dark:border-gray-700 pt-3 mb-5">
          <span className="text-sm text-gray-500 dark:text-gray-400">Total</span>
          <span className="text-base font-semibold text-gray-900 dark:text-gray-100 w-36 text-right font-mono">
            {fmt(total)}
          </span>
        </div>

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm rounded-lg border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            className="px-4 py-2 text-sm rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-medium transition-colors"
          >
            Save
          </button>
        </div>
      </form>

      {calcRowId !== null && (
        <CalculatorModal
          onApply={value => { updateRow(calcRowId, 'amount', value); setCalcRowId(null) }}
          onClose={() => setCalcRowId(null)}
        />
      )}
    </Modal>
  )
}

export function PlanFactPage() {
  const notify = useNotify()
  const [year, setYear] = usePersistedState('planfact:year', CURRENT_YEAR)
  const [selectedMonths, setSelectedMonths] = usePersistedState<number[]>('planfact:months', [CURRENT_MONTH])
  const [data, setData] = useState<PlanFactSummaryResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [modal, setModal] = useState<ModalState | null>(null)
  const [hideEmpty, setHideEmpty] = useState(false)
  const [showDiff, setShowDiff] = useState(false)

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

  const openModal = (categoryId: string, categoryName: string, year: number, month: number, currentValue: number, isExpense: boolean) => {
    setModal({ categoryId, categoryName, year, month, value: currentValue === 0 ? '' : currentValue.toFixed(2), isExpense })
  }

  const handleSave = async (rawValue: string) => {
    if (!modal) return
    const amount = parseFloat(rawValue) || 0
    const { categoryId, year: y, month: m } = modal
    setModal(null)

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

          {loading && <Spinner size="sm" />}
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
                    <th key={`${gi}-pct`}  className="px-2 py-1 text-right font-medium w-16 cursor-pointer select-none hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors" onClick={() => setShowDiff(d => !d)} title="Toggle % / difference">{showDiff ? 'Diff' : '%'}</th>
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
                        <PlanCell key={`${m}-${id}-plan`} value={plan} onClick={() => openModal(id, getIncomeName(id), year, m, plan, false)} />
                        <td key={`${m}-${id}-fact`} className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{fact > 0 ? fmt(fact) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell key={`${m}-${id}-pct`} plan={plan} fact={fact} showDiff={showDiff} />
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
                        <PctCell plan={p} fact={f} showDiff={showDiff} />
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
                      <PctCell key={`${m}-ti-pct`} plan={t?.incomePlan ?? 0} fact={t?.incomeFact ?? 0} showDiff={showDiff} />
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
                      <PctCell plan={p} fact={f} showDiff={showDiff} />
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
                        <PlanCell key={`${m}-${id}-plan`} value={plan} onClick={() => openModal(id, getExpenseName(id), year, m, plan, true)} />
                        <td key={`${m}-${id}-fact`} className="px-2 py-2 text-right text-xs font-mono text-gray-800 dark:text-gray-200">{fact > 0 ? fmt(fact) : <span className="text-gray-300 dark:text-gray-600">—</span>}</td>
                        <PctCell key={`${m}-${id}-pct`} plan={plan} fact={fact} isExpense showDiff={showDiff} />
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
                        <PctCell plan={p} fact={f} isExpense showDiff={showDiff} />
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
                      <PctCell key={`${m}-tl-pct`} plan={t?.expensePlan ?? 0} fact={t?.expenseFact ?? 0} isExpense showDiff={showDiff} />
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
                      <PctCell plan={p} fact={f} isExpense showDiff={showDiff} />
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
                      <PctCell key={`${m}-cb-pct`} plan={planClose} fact={factClose} showDiff={showDiff} />
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
                      <PctCell plan={planClose} fact={factClose} showDiff={showDiff} />
                    </>
                  )
                })()}
              </tr>

            </tbody>
          </table>
        </div>
      )}

      {modal && (
        <PlanModal
          state={modal}
          onClose={() => setModal(null)}
          onSave={handleSave}
        />
      )}
    </div>
  )
}
