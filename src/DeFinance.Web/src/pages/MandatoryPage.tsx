import { useEffect, useState } from 'react'
import { useNotify } from '../NotificationContext'
import {
  mandatoryPaymentsApi,
  formatDayOfPeriod,
  FREQUENCY_LABELS,
  type MandatoryPayment,
  type MandatoryPaymentQuery,
  type PaymentFrequency,
} from '../api/mandatoryPayments'
import { accountsApi, type Account } from '../api/accounts'
import { categoriesApi, type Category, PAYMENT_OBLIGATION_LABELS } from '../api/categories'
import { currenciesApi, type Currency } from '../api/currencies'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'

type ModalState = null | 'create' | MandatoryPayment

const FREQUENCIES: PaymentFrequency[] = ['Weekly', 'Monthly', 'Quarterly', 'Yearly']

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const OBLIGATION_COLORS: Record<string, string> = {
  SepaTransfer: 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
  Mandatory: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
  NonMandatory: 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400',
}

function dayLabel(frequency: PaymentFrequency): string {
  return frequency === 'Weekly' ? 'Day of Week (1=Mon…7=Sun)' : 'Day of Month (1–31)'
}

function dayMax(frequency: PaymentFrequency): number {
  return frequency === 'Weekly' ? 7 : 31
}

export function MandatoryPage() {
  const notify = useNotify()
  const [result, setResult] = useState<PagedResult<MandatoryPayment> | null>(null)
  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [paymentStatuses, setPaymentStatuses] = useState<PaymentStatus[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  // form fields
  const [formName, setFormName] = useState('')
  const [formAmount, setFormAmount] = useState('')
  const [formCurrencyId, setFormCurrencyId] = useState('')
  const [formAccountId, setFormAccountId] = useState('')
  const [formCategoryId, setFormCategoryId] = useState('')
  const [formPaymentStatusId, setFormPaymentStatusId] = useState('')
  const [formFrequency, setFormFrequency] = useState<PaymentFrequency>('Monthly')
  const [formDay, setFormDay] = useState('1')
  const [formNotes, setFormNotes] = useState('')

  // filters
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState('')
  const [accountFilter, setAccountFilter] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('')
  const [paymentStatusFilter, setPaymentStatusFilter] = useState('')
  const [frequencyFilter, setFrequencyFilter] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState<PageSize>(25)
  const [sortBy, setSortBy] = useState<string | null>(null)
  const [sortDirection, setSortDirection] = useState<SortDirection>('Asc')
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    Promise.all([
      accountsApi.getAll({ isActive: true, pageSize: 100 }),
      categoriesApi.getAll({ isActive: true, pageSize: 100 }),
      currenciesApi.getAll({ isActive: true, pageSize: 100 }),
      paymentStatusesApi.getAll({ isActive: true, pageSize: 100 }),
    ]).then(([acc, cat, cur, ps]) => {
      setAccounts(acc.items)
      setCategories(cat.items)
      setCurrencies(cur.items)
      setPaymentStatuses(ps.items)
      if (acc.items.length > 0) setFormAccountId(acc.items[0].id)
      if (cur.items.length > 0) setFormCurrencyId(cur.items[0].id)
    }).catch(() => {})
  }, [])

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1) }, 400)
    return () => clearTimeout(t)
  }, [search])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    const query: MandatoryPaymentQuery = {
      search: debouncedSearch || undefined,
      isActive: isActiveFilter !== '' ? isActiveFilter === 'true' : undefined,
      accountId: accountFilter || undefined,
      categoryId: categoryFilter || undefined,
      paymentStatusId: paymentStatusFilter || undefined,
      frequency: frequencyFilter !== '' ? (frequencyFilter as PaymentFrequency) : undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    }
    mandatoryPaymentsApi.getAll(query)
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError('Failed to load mandatory payments') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debouncedSearch, isActiveFilter, accountFilter, categoryFilter, paymentStatusFilter, frequencyFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  const refetch = () => setRefreshKey(k => k + 1)

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const openCreate = () => {
    setFormName(''); setFormAmount(''); setFormNotes('')
    setFormFrequency('Monthly'); setFormDay('1')
    setFormCategoryId(''); setFormPaymentStatusId('')
    setFormAccountId(accounts[0]?.id ?? '')
    setFormCurrencyId(currencies[0]?.id ?? '')
    setFormError(null); setModal('create')
  }

  const openEdit = (p: MandatoryPayment) => {
    setFormName(p.name)
    setFormAmount(String(p.amount))
    setFormCurrencyId(p.currencyId)
    setFormAccountId(p.accountId)
    setFormCategoryId(p.categoryId ?? '')
    setFormPaymentStatusId(p.paymentStatusId ?? '')
    setFormFrequency(p.frequency)
    setFormDay(String(p.dayOfPeriod))
    setFormNotes(p.notes ?? '')
    setFormError(null); setModal(p)
  }

  const closeModal = () => setModal(null)
  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true); setFormError(null)
    try {
      const req = {
        name: formName,
        amount: parseFloat(formAmount),
        currencyId: formCurrencyId,
        accountId: formAccountId,
        categoryId: formCategoryId || null,
        paymentStatusId: formPaymentStatusId || null,
        frequency: formFrequency,
        dayOfPeriod: parseInt(formDay, 10),
        notes: formNotes || null,
      }
      if (modal === 'create') {
        await mandatoryPaymentsApi.create(req)
        notify('Mandatory payment created', 'success')
      } else if (modal !== null) {
        await mandatoryPaymentsApi.update(modal.id, { ...req, id: modal.id })
        notify('Mandatory payment updated', 'info')
      }
      closeModal(); refetch()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (p: MandatoryPayment) => {
    if (p.isActive) { await mandatoryPaymentsApi.deactivate(p.id); notify('Payment deactivated', 'error') }
    else { await mandatoryPaymentsApi.activate(p.id); notify('Payment activated', 'success') }
    refetch()
  }

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Mandatory Payments</h1>
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            + New Payment
          </button>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <input
            type="search"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search by name…"
            className={`${filterCls} w-48`}
          />
          <select value={isActiveFilter} onChange={e => { setIsActiveFilter(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All statuses</option>
            <option value="true">Active only</option>
            <option value="false">Inactive only</option>
          </select>
          <select value={accountFilter} onChange={e => { setAccountFilter(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All accounts</option>
            {accounts.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
          </select>
          <select value={categoryFilter} onChange={e => { setCategoryFilter(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All categories</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <select value={paymentStatusFilter} onChange={e => { setPaymentStatusFilter(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All statuses</option>
            {paymentStatuses.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select value={frequencyFilter} onChange={e => { setFrequencyFilter(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All frequencies</option>
            {FREQUENCIES.map(f => <option key={f} value={f}>{FREQUENCY_LABELS[f]}</option>)}
          </select>
          {loading && <span className="text-xs text-gray-400 dark:text-gray-500">Loading…</span>}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Payment' : 'New Mandatory Payment'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>Name</label>
              <input required maxLength={200} value={formName} onChange={e => setFormName(e.target.value)} className={inputCls} placeholder="e.g. Rent" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelCls}>Amount</label>
                <input required type="number" min="0.01" step="0.01" value={formAmount} onChange={e => setFormAmount(e.target.value)} className={inputCls} />
              </div>
              <div>
                <label className={labelCls}>Currency</label>
                <select value={formCurrencyId} onChange={e => setFormCurrencyId(e.target.value)} className={inputCls}>
                  {currencies.map(c => <option key={c.id} value={c.id}>{c.symbol} {c.code}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className={labelCls}>Account (Bank)</label>
              <select required value={formAccountId} onChange={e => setFormAccountId(e.target.value)} className={inputCls}>
                <option value="">Select account…</option>
                {accounts.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>Category (optional)</label>
              <select value={formCategoryId} onChange={e => setFormCategoryId(e.target.value)} className={inputCls}>
                <option value="">No category</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>Payment Status (optional)</label>
              <select value={formPaymentStatusId} onChange={e => setFormPaymentStatusId(e.target.value)} className={inputCls}>
                <option value="">No status</option>
                {paymentStatuses.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelCls}>Frequency</label>
                <select value={formFrequency} onChange={e => { setFormFrequency(e.target.value as PaymentFrequency); setFormDay('1') }} className={inputCls}>
                  {FREQUENCIES.map(f => <option key={f} value={f}>{FREQUENCY_LABELS[f]}</option>)}
                </select>
              </div>
              <div>
                <label className={labelCls}>{dayLabel(formFrequency)}</label>
                <input required type="number" min="1" max={dayMax(formFrequency)} value={formDay} onChange={e => setFormDay(e.target.value)} className={inputCls} />
              </div>
            </div>
            <div>
              <label className={labelCls}>Notes (optional)</label>
              <textarea maxLength={500} value={formNotes} onChange={e => setFormNotes(e.target.value)} rows={2} className={`${inputCls} resize-none`} placeholder="Any additional details…" />
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100">Cancel</button>
              <button type="submit" disabled={saving} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
                {saving ? 'Saving…' : isEditing ? 'Save' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      <div className="flex flex-col flex-1 min-h-0 mx-8 mb-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <div className="flex-1 min-h-0 overflow-y-auto overflow-x-hidden">
          <table className="w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <SortableHeader label="Name" field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <SortableHeader label="Amount" field="amount" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Account</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Frequency</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Day</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Category</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Payment Status</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Notes</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(p => (
                <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{p.name}</td>
                  <td className="px-4 py-3 font-mono text-gray-900 dark:text-gray-100">
                    {p.currency?.symbol ?? ''} {p.amount.toFixed(2)}
                    {p.currency && <span className="ml-1 text-xs text-gray-400">{p.currency.code}</span>}
                  </td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                    {p.account?.name ?? '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{FREQUENCY_LABELS[p.frequency]}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                    {formatDayOfPeriod(p.frequency, p.dayOfPeriod)}
                  </td>
                  <td className="px-4 py-3">
                    {p.category ? (
                      <div className="flex flex-col gap-1">
                        <div className="flex items-center gap-1.5">
                          {p.category.color && (
                            <span className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: p.category.color }} />
                          )}
                          <span className="text-gray-700 dark:text-gray-300 text-xs">{p.category.name}</span>
                        </div>
                        {p.category.paymentObligation && (
                          <span className={`inline-flex w-fit items-center rounded-full px-1.5 py-0.5 text-xs font-medium ${OBLIGATION_COLORS[p.category.paymentObligation] ?? ''}`}>
                            {PAYMENT_OBLIGATION_LABELS[p.category.paymentObligation]}
                          </span>
                        )}
                      </div>
                    ) : (
                      <span className="text-gray-400 dark:text-gray-500">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                    {p.paymentStatus
                      ? <span className="inline-flex items-center rounded-full bg-indigo-100 text-indigo-700 dark:bg-indigo-900 dark:text-indigo-300 px-2 py-0.5 text-xs font-medium">{p.paymentStatus.name}</span>
                      : <span className="text-gray-400 dark:text-gray-500">—</span>}
                  </td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-[160px] truncate" title={p.notes ?? ''}>
                    {p.notes || '—'}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${p.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                      {p.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton icon={<PencilIcon />} label="Edit" onClick={() => openEdit(p)} className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton
                        icon={p.isActive ? <BanIcon /> : <CheckCircleIcon />}
                        label={p.isActive ? 'Deactivate' : 'Activate'}
                        onClick={() => toggle(p)}
                        className={p.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'}
                      />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={10} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">No mandatory payments found.</td>
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
    </div>
  )
}
