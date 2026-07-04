import { useEffect, useRef, useState, type ReactNode } from 'react'
import * as signalR from '@microsoft/signalr'
import { useNotify } from '../NotificationContext'
import { useMainCurrency } from '../MainCurrencyContext'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { accountsApi, type Account } from '../api/accounts'
import { categoriesApi, type Category } from '../api/categories'
import { counterpartiesApi, type Counterparty } from '../api/counterparties'
import {
  reportsApi, type Report, type ReportType, type ReportPeriod,
  REPORT_TYPE_LABELS, REPORT_PERIOD_LABELS
} from '../api/reports'
import { TOKEN_KEY } from '../api/auth'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'
import { Spinner } from '../components/Spinner'

type ModalState = null | 'create' | PaymentStatus

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-2 py-1 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500'

// ─────────────────────────────────────────────────────────────
// Payment Statuses Panel (unchanged)
// ─────────────────────────────────────────────────────────────
function PaymentStatusesPanel() {
  const notify = useNotify()
  const [result, setResult] = useState<PagedResult<PaymentStatus> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [formName, setFormName] = useState('')
  const [formDescription, setFormDescription] = useState('')
  const [formColor, setFormColor] = useState<string | null>(null)

  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState<PageSize>(100)
  const [sortBy, setSortBy] = useState<string | null>(null)
  const [sortDirection, setSortDirection] = useState<SortDirection>('Asc')
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1) }, 400)
    return () => clearTimeout(t)
  }, [search])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    paymentStatusesApi.getAll({
      search: debouncedSearch || undefined,
      isActive: isActiveFilter !== '' ? isActiveFilter === 'true' : undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    })
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError('Failed to load payment statuses') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debouncedSearch, isActiveFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  const refetch = () => setRefreshKey(k => k + 1)

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const openCreate = () => {
    setFormName(''); setFormDescription(''); setFormColor(null); setFormError(null); setModal('create')
  }

  const openEdit = (s: PaymentStatus) => {
    setFormName(s.name); setFormDescription(s.description ?? ''); setFormColor(s.color ?? null); setFormError(null); setModal(s)
  }

  const closeModal = () => setModal(null)
  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      const description = formDescription.trim() || null
      if (modal === 'create') {
        await paymentStatusesApi.create({ name: formName, description, color: formColor })
        notify('Payment status created', 'success')
      } else if (modal !== null) {
        await paymentStatusesApi.update(modal.id, { name: formName, description, color: formColor })
        notify('Payment status updated', 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (status: PaymentStatus) => {
    if (status.isActive) { await paymentStatusesApi.deactivate(status.id); notify('Payment status deactivated', 'error') }
    else { await paymentStatusesApi.activate(status.id); notify('Payment status activated', 'success') }
    refetch()
  }

  const items = result?.items ?? []

  return (
    <>
      {modal !== null && (
        <Modal title={isEditing ? 'Edit Payment Status' : 'New Payment Status'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>Name</label>
              <input
                required
                maxLength={100}
                value={formName}
                onChange={e => setFormName(e.target.value)}
                className={inputCls}
                placeholder="Status name"
              />
            </div>
            <div>
              <label className={labelCls}>Description (optional)</label>
              <textarea
                maxLength={500}
                value={formDescription}
                onChange={e => setFormDescription(e.target.value)}
                className={`${inputCls} resize-none`}
                rows={2}
                placeholder="Short description…"
              />
            </div>
            <div>
              <label className={labelCls}>Color (optional)</label>
              <div className="flex items-center gap-3">
                <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
                  <input
                    type="checkbox"
                    checked={formColor !== null}
                    onChange={e => setFormColor(e.target.checked ? '#6366f1' : null)}
                    className="rounded"
                  />
                  Custom color
                </label>
                {formColor !== null && (
                  <>
                    <input
                      type="color"
                      value={formColor}
                      onChange={e => setFormColor(e.target.value)}
                      className="w-10 h-8 rounded cursor-pointer border border-gray-300 dark:border-gray-600 bg-transparent"
                    />
                    <span
                      className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium"
                      style={{ backgroundColor: formColor + '25', color: formColor }}
                    >
                      {formName || 'Preview'}
                    </span>
                  </>
                )}
              </div>
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button
                type="button"
                onClick={closeModal}
                className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={saving}
                className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
              >
                {saving ? 'Saving…' : isEditing ? 'Save' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      <div className="flex items-center gap-2 mb-3 flex-wrap">
        <input
          type="search"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search…"
          className={`${filterCls} w-36`}
        />
        <select value={isActiveFilter} onChange={e => { setIsActiveFilter(e.target.value); setPage(1) }} className={filterCls}>
          <option value="">All statuses</option>
          <option value="true">Active only</option>
          <option value="false">Inactive only</option>
        </select>
        {loading && <Spinner size="sm" />}
        <button
          onClick={openCreate}
          className="ml-auto px-3 py-1 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-medium rounded-lg transition-colors"
        >
          + New
        </button>
      </div>

      {error && <p className="text-sm text-red-500 mb-2">{error}</p>}

      <div className="flex flex-col flex-1 min-h-0 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div className="flex-1 min-h-0 overflow-y-auto overflow-x-hidden">
          <table className="w-full divide-y divide-gray-200 dark:divide-gray-700 text-xs">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <SortableHeader label="Name" field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Description</th>
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Status</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700 bg-white dark:bg-gray-800">
              {items.map(s => (
                <tr key={s.id} className={s.isActive ? 'hover:bg-gray-50 dark:hover:bg-gray-700' : 'bg-gray-100 dark:bg-gray-900/50'}>
                  <td className="px-3 py-2 font-medium text-gray-900 dark:text-gray-100">
                    <div className="flex items-center gap-2">
                      {s.color
                        ? <span className="w-3 h-3 rounded-full flex-shrink-0" style={{ backgroundColor: s.color }} />
                        : <span className="w-3 h-3 rounded-full flex-shrink-0 border border-gray-300 dark:border-gray-600" />
                      }
                      {s.name}
                    </div>
                  </td>
                  <td className="px-3 py-2 text-gray-500 dark:text-gray-400 max-w-xs truncate">
                    {s.description ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                  </td>
                  <td className="px-3 py-2">
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                        s.isActive
                          ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                          : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                      }`}
                    >
                      {s.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton
                        icon={<PencilIcon />}
                        label="Edit"
                        onClick={() => openEdit(s)}
                        className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400"
                      />
                      <IconButton
                        icon={s.isActive ? <BanIcon /> : <CheckCircleIcon />}
                        label={s.isActive ? 'Deactivate' : 'Activate'}
                        onClick={() => toggle(s)}
                        className={s.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'}
                      />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={4} className="px-3 py-6 text-center text-gray-400 dark:text-gray-500">
                    No payment statuses found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
        {result && (
          <Pagination
            page={result.page}
            pageSize={pageSize}
            totalCount={result.totalCount}
            totalPages={result.totalPages}
            onPageChange={setPage}
            onPageSizeChange={size => { setPageSize(size); setPage(1) }}
          />
        )}
      </div>
    </>
  )
}

// ─────────────────────────────────────────────────────────────
// Main Currency Panel (unchanged)
// ─────────────────────────────────────────────────────────────
function MainCurrencyPanel() {
  const { currencies, mainCurrency, setMainCurrencyId } = useMainCurrency()

  return (
    <div className="flex flex-col gap-4">
      <p className="text-xs text-gray-500 dark:text-gray-400">
        Choose the currency used to display the <strong>In Main Currency</strong> column in the Transactions table. Stored locally in your browser.
      </p>
      <div>
        <label className={labelCls}>Main Currency</label>
        <select
          value={mainCurrency?.id ?? ''}
          onChange={e => setMainCurrencyId(e.target.value)}
          className={inputCls}
        >
          {currencies.map(c => (
            <option key={c.id} value={c.id}>{c.symbol} {c.code} — {c.name}</option>
          ))}
        </select>
      </div>
      {mainCurrency && (
        <div className="flex items-center gap-3 px-3 py-2 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600">
          <span className="text-2xl font-bold text-gray-700 dark:text-gray-200">{mainCurrency.symbol}</span>
          <div>
            <p className="text-sm font-medium text-gray-800 dark:text-gray-100">{mainCurrency.name}</p>
            <p className="text-xs text-gray-400 dark:text-gray-500">{mainCurrency.code}</p>
          </div>
        </div>
      )}
    </div>
  )
}

// ─────────────────────────────────────────────────────────────
// Reports Panel
// ─────────────────────────────────────────────────────────────

const REPORT_TYPE_DESCRIPTIONS: Record<ReportType, string> = {
  CashFlowStatement:        'Daily income vs. expenses with net cash flow over the period.',
  ExpenseCategoryBreakdown: 'Spending grouped by category with proportion bars.',
  AccountBalanceSummary:    'Opening and closing balances per account with change summary.',
  CounterpartySpending:     'Total transactions per counterparty with amount and proportion bars.',
}

const STATUS_STYLES: Record<string, string> = {
  Pending:    'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/40 dark:text-yellow-300',
  Processing: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
  Completed:  'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
  Failed:     'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300',
}

async function downloadReport(id: string, fileName: string) {
  const token = localStorage.getItem(TOKEN_KEY) ?? ''
  const res = await fetch(`/api/reports/${id}/download`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (!res.ok) return
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

function ReportsPanel() {
  const notify = useNotify()

  const [reportType, setReportType] = useState<ReportType>('CashFlowStatement')
  const [period, setPeriod] = useState<ReportPeriod>('LastMonth')
  const [accountId, setAccountId] = useState<string>('')
  const [categoryIds, setCategoryIds] = useState<string[]>([])
  const [counterpartyIds, setCounterpartyIds] = useState<string[]>([])
  const [generating, setGenerating] = useState(false)

  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [counterparties, setCounterparties] = useState<Counterparty[]>([])
  const [reports, setReports] = useState<Report[]>([])
  const [loadingReports, setLoadingReports] = useState(true)
  const [readyReportId, setReadyReportId] = useState<string | null>(null)

  const refreshReports = () =>
    reportsApi.getAll().then(setReports).catch(() => {})

  useEffect(() => {
    accountsApi.getAll({ pageSize: 200 }).then(r => setAccounts(r.items.filter(a => a.isActive))).catch(() => {})
    categoriesApi.getAll({ pageSize: 500 }).then(r => setCategories(r.items.filter(c => c.isActive))).catch(() => {})
    counterpartiesApi.getAll({ pageSize: 500 }).then(r => setCounterparties(r.items.filter(c => c.isActive))).catch(() => {})
    refreshReports().finally(() => setLoadingReports(false))
  }, [])

  // Stable refs so SignalR handler never captures a stale closure
  const refreshRef = useRef(refreshReports)
  refreshRef.current = refreshReports
  const notifyRef = useRef(notify)
  notifyRef.current = notify

  // SignalR — empty dep array so the connection is created exactly once
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => localStorage.getItem(TOKEN_KEY) ?? '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('ReportGenerated', (data: { reportId: string; success: boolean }) => {
      refreshRef.current()
      if (data.success) {
        setReadyReportId(data.reportId)
        notifyRef.current('Report is ready for download', 'success')
      } else {
        notifyRef.current('Report generation failed', 'error')
      }
    })

    connection.start().catch(() => {})
    return () => { connection.stop() }
  }, [])

  // Polling fallback: re-fetch every 3 s while any report is still in-flight.
  // Also fires the popup notification when a report transitions to Completed/Failed,
  // so the user gets feedback even if the SignalR event was missed.
  useEffect(() => {
    const hasActive = reports.some(r => r.status === 'Pending' || r.status === 'Processing')
    if (!hasActive) return
    const id = setInterval(() => {
      reportsApi.getAll().then(latest => {
        latest.forEach(r => {
          const prev = reports.find(p => p.id === r.id)
          if (!prev) return
          const wasActive = prev.status === 'Pending' || prev.status === 'Processing'
          if (!wasActive) return
          if (r.status === 'Completed') {
            setReadyReportId(r.id)
            notifyRef.current('Report is ready for download', 'success')
          } else if (r.status === 'Failed') {
            notifyRef.current('Report generation failed', 'error')
          }
        })
        setReports(latest)
      }).catch(() => {})
    }, 3000)
    return () => clearInterval(id)
  }, [reports])

  const handleGenerate = async () => {
    setGenerating(true)
    try {
      await reportsApi.generate({
        type: reportType,
        period,
        accountId: accountId || null,
        categoryIds: categoryIds.length > 0 ? categoryIds : undefined,
        counterpartyIds: counterpartyIds.length > 0 ? counterpartyIds : undefined,
      })
      await refreshReports()
      notify('Report queued — you\'ll be notified when it\'s ready', 'info')
    } catch {
      notify('Failed to generate report', 'error')
    } finally {
      setGenerating(false)
    }
  }

  const expenseCategories = categories.filter(c => c.type === 'Expense' || c.type === 'TransferOut')

  return (
    <div className="flex gap-5 h-full min-h-0">
      {/* ── Form ── */}
      <div className="w-72 flex-shrink-0 flex flex-col gap-4">
        {/* Report type cards */}
        <div>
          <label className={labelCls}>Report Type</label>
          <div className="flex flex-col gap-2">
            {(Object.keys(REPORT_TYPE_LABELS) as ReportType[]).map(rt => (
              <button
                key={rt}
                onClick={() => setReportType(rt)}
                className={`text-left px-3 py-2.5 rounded-lg border transition-all ${
                  reportType === rt
                    ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 ring-1 ring-indigo-500'
                    : 'border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500'
                }`}
              >
                <p className={`text-xs font-semibold ${reportType === rt ? 'text-indigo-700 dark:text-indigo-300' : 'text-gray-700 dark:text-gray-300'}`}>
                  {REPORT_TYPE_LABELS[rt]}
                </p>
                <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5 leading-relaxed">
                  {REPORT_TYPE_DESCRIPTIONS[rt]}
                </p>
              </button>
            ))}
          </div>
        </div>

        {/* Period */}
        <div>
          <label className={labelCls}>Period</label>
          <select value={period} onChange={e => setPeriod(e.target.value as ReportPeriod)} className={inputCls}>
            {(Object.keys(REPORT_PERIOD_LABELS) as ReportPeriod[]).map(p => (
              <option key={p} value={p}>{REPORT_PERIOD_LABELS[p]}</option>
            ))}
          </select>
        </div>

        {/* Dynamic filter fields */}
        {reportType === 'CashFlowStatement' && (
          <div>
            <label className={labelCls}>Account (optional)</label>
            <select value={accountId} onChange={e => setAccountId(e.target.value)} className={inputCls}>
              <option value="">All accounts</option>
              {accounts.map(a => (
                <option key={a.id} value={a.id}>{a.name} ({a.currency?.code ?? '?'})</option>
              ))}
            </select>
          </div>
        )}

        {reportType === 'ExpenseCategoryBreakdown' && (
          <div className="flex flex-col gap-1.5">
            <div className="flex items-center justify-between">
              <label className={labelCls}>Categories (optional)</label>
              {categoryIds.length > 0 && (
                <button
                  onClick={() => setCategoryIds([])}
                  className="text-xs text-indigo-500 hover:text-indigo-700 dark:text-indigo-400"
                >
                  Clear ({categoryIds.length})
                </button>
              )}
            </div>
            <div className="max-h-44 overflow-y-auto rounded-lg border border-gray-200 dark:border-gray-600 divide-y divide-gray-100 dark:divide-gray-700">
              {expenseCategories.length === 0 && (
                <p className="px-3 py-2 text-xs text-gray-400">No expense categories found.</p>
              )}
              {expenseCategories.map(c => {
                const checked = categoryIds.includes(c.id)
                return (
                  <label
                    key={c.id}
                    className={`flex items-center gap-2.5 px-3 py-1.5 cursor-pointer transition-colors text-xs
                      ${checked
                        ? 'bg-indigo-50 dark:bg-indigo-900/20 text-indigo-800 dark:text-indigo-200'
                        : 'hover:bg-gray-50 dark:hover:bg-gray-700/50 text-gray-700 dark:text-gray-300'}`}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={e =>
                        setCategoryIds(prev =>
                          e.target.checked ? [...prev, c.id] : prev.filter(id => id !== c.id)
                        )
                      }
                      className="rounded border-gray-300 dark:border-gray-600 text-indigo-600 focus:ring-indigo-500 shrink-0"
                    />
                    <span className="truncate">{c.name}</span>
                  </label>
                )
              })}
            </div>
            {categoryIds.length === 0 && (
              <p className="text-xs text-gray-400 dark:text-gray-500">Leave empty to include all categories.</p>
            )}
          </div>
        )}

        {reportType === 'CounterpartySpending' && (
          <div className="flex flex-col gap-1.5">
            <div className="flex items-center justify-between">
              <label className={labelCls}>Counterparties (optional)</label>
              {counterpartyIds.length > 0 && (
                <button
                  onClick={() => setCounterpartyIds([])}
                  className="text-xs text-indigo-500 hover:text-indigo-700 dark:text-indigo-400"
                >
                  Clear ({counterpartyIds.length})
                </button>
              )}
            </div>
            <div className="max-h-44 overflow-y-auto rounded-lg border border-gray-200 dark:border-gray-600 divide-y divide-gray-100 dark:divide-gray-700">
              {counterparties.length === 0 && (
                <p className="px-3 py-2 text-xs text-gray-400">No counterparties found.</p>
              )}
              {counterparties.map(cp => {
                const checked = counterpartyIds.includes(cp.id)
                return (
                  <label
                    key={cp.id}
                    className={`flex items-center gap-2.5 px-3 py-1.5 cursor-pointer transition-colors text-xs
                      ${checked
                        ? 'bg-indigo-50 dark:bg-indigo-900/20 text-indigo-800 dark:text-indigo-200'
                        : 'hover:bg-gray-50 dark:hover:bg-gray-700/50 text-gray-700 dark:text-gray-300'}`}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={e =>
                        setCounterpartyIds(prev =>
                          e.target.checked ? [...prev, cp.id] : prev.filter(id => id !== cp.id)
                        )
                      }
                      className="rounded border-gray-300 dark:border-gray-600 text-indigo-600 focus:ring-indigo-500 shrink-0"
                    />
                    <span className="flex-1 truncate">{cp.name}</span>
                    <span className="text-gray-400 dark:text-gray-500 shrink-0">{cp.type}</span>
                  </label>
                )
              })}
            </div>
            {counterpartyIds.length === 0 && (
              <p className="text-xs text-gray-400 dark:text-gray-500">Leave empty to include all counterparties.</p>
            )}
          </div>
        )}

        <button
          onClick={handleGenerate}
          disabled={generating}
          className="mt-auto w-full px-4 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors flex items-center justify-center gap-2"
        >
          {generating ? (
            <><Spinner size="sm" /><span>Queuing…</span></>
          ) : (
            <>
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.75}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
              </svg>
              Generate Report
            </>
          )}
        </button>
      </div>

      {/* ── History ── */}
      <div className="flex-1 flex flex-col min-h-0">
        <div className="flex items-center justify-between mb-2">
          <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide">Report History</p>
          {loadingReports && <Spinner size="sm" />}
        </div>

        <div className="flex-1 min-h-0 overflow-y-auto rounded-lg border border-gray-200 dark:border-gray-700">
          <table className="w-full divide-y divide-gray-200 dark:divide-gray-700 text-xs">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Type</th>
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Period</th>
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Status</th>
                <th className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400">Created</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700 bg-white dark:bg-gray-800">
              {reports.map(r => (
                <tr key={r.id} className={`hover:bg-gray-50 dark:hover:bg-gray-700 ${readyReportId === r.id ? 'bg-green-50 dark:bg-green-900/10' : ''}`}>
                  <td className="px-3 py-2 font-medium text-gray-800 dark:text-gray-200">
                    {REPORT_TYPE_LABELS[r.type]}
                  </td>
                  <td className="px-3 py-2 text-gray-500 dark:text-gray-400">
                    {REPORT_PERIOD_LABELS[r.period]}
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex items-center gap-1.5">
                      {(r.status === 'Pending' || r.status === 'Processing') && (
                        <span className="inline-block w-2 h-2 rounded-full bg-blue-400 animate-pulse" />
                      )}
                      <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_STYLES[r.status] ?? ''}`}>
                        {r.status}
                      </span>
                    </div>
                    {r.status === 'Failed' && r.errorMessage && (
                      <p className="text-xs text-red-400 mt-0.5 truncate max-w-xs" title={r.errorMessage}>{r.errorMessage}</p>
                    )}
                  </td>
                  <td className="px-3 py-2 text-gray-400 dark:text-gray-500 whitespace-nowrap">
                    {new Date(r.createdAt).toLocaleString()}
                  </td>
                  <td className="px-3 py-2 text-right">
                    {r.status === 'Completed' && r.fileName && (
                      <button
                        onClick={() => downloadReport(r.id, r.fileName!)}
                        className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium text-indigo-600 dark:text-indigo-400 hover:text-indigo-800 dark:hover:text-indigo-300 bg-indigo-50 dark:bg-indigo-900/20 hover:bg-indigo-100 dark:hover:bg-indigo-900/40 rounded-lg transition-colors"
                      >
                        <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                          <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5M16.5 12 12 16.5m0 0L7.5 12m4.5 4.5V3" />
                        </svg>
                        Download
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {reports.length === 0 && !loadingReports && (
                <tr>
                  <td colSpan={5} className="px-3 py-8 text-center text-gray-400 dark:text-gray-500">
                    No reports generated yet. Choose a type and click <strong>Generate Report</strong>.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────
// Page layout
// ─────────────────────────────────────────────────────────────
export function AdministrationPage() {
  return (
    <div className="p-6 h-full flex flex-col">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Administration</h1>

      <div className="flex-1 grid grid-cols-2 gap-4 min-h-0" style={{ gridTemplateRows: 'minmax(0,1fr) minmax(0,1fr)' }}>
        <Panel title="Payment Statuses">
          <PaymentStatusesPanel />
        </Panel>

        <Panel title="Main Currency">
          <MainCurrencyPanel />
        </Panel>

        <div className="col-span-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex flex-col overflow-hidden min-h-0">
          <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex items-center gap-2">
            <svg className="w-4 h-4 text-indigo-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.75}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
            </svg>
            <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">Reports</h2>
          </div>
          <div className="flex-1 p-4 overflow-auto min-h-0">
            <ReportsPanel />
          </div>
        </div>
      </div>
    </div>
  )
}

function Panel({ title, children }: { title: string; children?: ReactNode }) {
  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex flex-col overflow-hidden">
      <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-700">
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">{title}</h2>
      </div>
      <div className="flex-1 p-4 overflow-auto flex flex-col min-h-0">
        {children}
      </div>
    </div>
  )
}
