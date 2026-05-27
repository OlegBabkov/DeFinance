import { useEffect, useState } from 'react'
import { useNotify } from '../NotificationContext'
import { useMainCurrency } from '../MainCurrencyContext'
import { transactionsApi, type Transaction, type CreateTransactionRequest } from '../api/transactions'
import { accountsApi, type Account } from '../api/accounts'
import { categoriesApi, type Category } from '../api/categories'
import { counterpartiesApi, type Counterparty } from '../api/counterparties'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { currenciesApi, type Currency } from '../api/currencies'
import { type PagedResult, type PageSize } from '../api/common'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'
import { Modal } from '../components/Modal'
import { IconButton, InfoIcon, PencilIcon, TrashIcon } from '../components/IconButton'
import { TransactionPanel } from '../components/TransactionPanel'
import { useFavorites, sortByFavorites } from '../hooks/useFavorites'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

function fmt(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

function num(n: number, decimals = 2) {
  return n.toLocaleString('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

function toDateOnly(iso: string) {
  return iso.slice(0, 10)
}

function todayDate() {
  return new Date().toISOString().slice(0, 10)
}

function activeOrCurrent<T extends { id: string; isActive: boolean }>(items: T[], currentId: string): T[] {
  const active = items.filter(i => i.isActive)
  if (currentId && !active.some(i => i.id === currentId)) {
    const cur = items.find(i => i.id === currentId)
    if (cur) return [...active, cur]
  }
  return active
}

type ModalState = null | 'create' | Transaction

interface FormState {
  dateTime: string
  accountId: string
  categoryId: string
  counterpartyId: string
  paymentStatusId: string
  inCurrencyId: string
  sum: string
  exchangeRate: string
  notes: string
}

function emptyForm(defaults: { accountId?: string; currencyId?: string; paymentStatusId?: string }): FormState {
  return {
    dateTime: todayDate(),
    accountId: defaults.accountId ?? '',
    categoryId: '',
    counterpartyId: '',
    paymentStatusId: defaults.paymentStatusId ?? '',
    inCurrencyId: defaults.currencyId ?? '',
    sum: '',
    exchangeRate: '1',
    notes: '',
  }
}

function txToForm(tx: Transaction): FormState {
  return {
    dateTime: toDateOnly(tx.dateTime),
    accountId: tx.accountId,
    categoryId: tx.categoryId,
    counterpartyId: tx.counterpartyId ?? '',
    paymentStatusId: tx.paymentStatusId,
    inCurrencyId: tx.inCurrencyId,
    sum: String(tx.sum),
    exchangeRate: String(tx.exchangeRate),
    notes: tx.notes ?? '',
  }
}

export function TransactionsPage() {
  const notify = useNotify()
  const { mainCurrency } = useMainCurrency()
  const { favorites: favCats } = useFavorites('categories')
  const { favorites: favCps }  = useFavorites('counterparties')
  const [result, setResult] = useState<PagedResult<Transaction> | null>(null)
  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [counterparties, setCounterparties] = useState<Counterparty[]>([])
  const [paymentStatuses, setPaymentStatuses] = useState<PaymentStatus[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [refreshKey, setRefreshKey] = useState(0)

  // filters
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [accountId, setAccountId] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [counterpartyId, setCounterpartyId] = useState('')
  const [paymentStatusId, setPaymentStatusId] = useState('')
  const [inCurrencyId, setInCurrencyId] = useState('')
  const [notes, setNotes] = useState('')
  const [debouncedNotes, setDebouncedNotes] = useState('')

  // pagination & sort
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState<PageSize>(25)
  const [sortBy, setSortBy] = useState<string | null>(null)
  const [sortDirection, setSortDirection] = useState<'Asc' | 'Desc'>('Desc')

  // side panel
  const [selectedTx, setSelectedTx] = useState<Transaction | null>(null)

  // modal
  const [modal, setModal] = useState<ModalState>(null)
  const [form, setForm] = useState<FormState>(emptyForm({}))
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const refetch = () => setRefreshKey(k => k + 1)

  // load filter dropdowns once
  useEffect(() => {
    accountsApi.getAll({ pageSize: 100 }).then(r => setAccounts(r.items)).catch(() => {})
    categoriesApi.getAll({ pageSize: 100 }).then(r => setCategories(r.items)).catch(() => {})
    counterpartiesApi.getAll({ pageSize: 100 }).then(r => setCounterparties(r.items)).catch(() => {})
    paymentStatusesApi.getAll({ pageSize: 100 }).then(r => setPaymentStatuses(r.items)).catch(() => {})
    currenciesApi.getAll({ pageSize: 100 }).then(r => setCurrencies(r.items)).catch(() => {})
  }, [])

  // debounce notes search
  useEffect(() => {
    const t = setTimeout(() => { setDebouncedNotes(notes); setPage(1) }, 400)
    return () => clearTimeout(t)
  }, [notes])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    transactionsApi.getAll({
      dateFrom: dateFrom || undefined,
      dateTo: dateTo || undefined,
      accountId: accountId || undefined,
      categoryId: categoryId || undefined,
      counterpartyId: counterpartyId || undefined,
      paymentStatusId: paymentStatusId || undefined,
      inCurrencyId: inCurrencyId || undefined,
      notes: debouncedNotes || undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    })
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError('Failed to load transactions') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [dateFrom, dateTo, accountId, categoryId, counterpartyId, paymentStatusId,
    inCurrencyId, debouncedNotes, page, pageSize, sortBy, sortDirection, refreshKey])

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const resetFilters = () => {
    setDateFrom(''); setDateTo(''); setAccountId(''); setCategoryId('')
    setCounterpartyId(''); setPaymentStatusId(''); setInCurrencyId(''); setNotes('')
    setPage(1)
  }

  const hasFilters = dateFrom || dateTo || accountId || categoryId ||
    counterpartyId || paymentStatusId || inCurrencyId || notes

  // modal helpers
  const defaultForm = () => {
    const firstActiveAccount = accounts.find(a => a.isActive)
    const firstActiveStatus = paymentStatuses.find(s => s.isActive)
    return emptyForm({
      accountId: firstActiveAccount?.id,
      currencyId: firstActiveAccount?.currencyId,
      paymentStatusId: firstActiveStatus?.id,
    })
  }

  const openCreate = () => {
    setForm(defaultForm())
    setFormError(null)
    setModal('create')
  }

  const openEdit = (tx: Transaction) => {
    setForm(txToForm(tx))
    setFormError(null)
    setModal(tx)
  }

  const closeModal = () => setModal(null)

  const setField = (key: keyof FormState) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }))

  const onAccountChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const acct = accounts.find(a => a.id === e.target.value)
    setForm(f => ({ ...f, accountId: e.target.value, inCurrencyId: acct?.currencyId ?? f.inCurrencyId }))
  }

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      const req: CreateTransactionRequest = {
        dateTime: form.dateTime + 'T00:00:00Z',
        sum: parseFloat(form.sum),
        exchangeRate: parseFloat(form.exchangeRate),
        inCurrencyId: form.inCurrencyId,
        accountId: form.accountId,
        categoryId: form.categoryId,
        counterpartyId: form.counterpartyId || null,
        paymentStatusId: form.paymentStatusId,
        notes: form.notes || null,
      }
      if (modal === 'create') {
        await transactionsApi.create(req)
        notify('Transaction created', 'success')
      } else if (modal !== null) {
        await transactionsApi.update(modal.id, { id: modal.id, ...req })
        notify('Transaction updated', 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (tx: Transaction) => {
    if (!confirm(`Delete this transaction (${tx.category?.name ?? ''} ${tx.sum})?`)) return
    try {
      await transactionsApi.remove(tx.id)
      notify('Transaction deleted', 'error')
      refetch()
    } catch {
      alert('Failed to delete transaction.')
    }
  }

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Transactions</h1>
          <div className="flex items-center gap-3">
            {loading && <span className="text-xs text-gray-400 dark:text-gray-500">Loading…</span>}
            <button
              onClick={openCreate}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
            >
              + New Transaction
            </button>
          </div>
        </div>

        {/* Filters — row 1 */}
        <div className="flex flex-wrap items-center gap-3 mb-2">
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-500 dark:text-gray-400">From</span>
            <input type="date" value={dateFrom} onChange={e => { setDateFrom(e.target.value); setPage(1) }} className={filterCls} />
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-500 dark:text-gray-400">To</span>
            <input type="date" value={dateTo} onChange={e => { setDateTo(e.target.value); setPage(1) }} className={filterCls} />
          </div>
          <input
            type="search" value={notes} onChange={e => setNotes(e.target.value)}
            placeholder="Search notes…" className={`${filterCls} w-44`}
          />
          {hasFilters && (
            <button onClick={resetFilters} className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline">
              Clear filters
            </button>
          )}
        </div>

        {/* Filters — row 2 */}
        <div className="flex flex-wrap items-center gap-3">
          <select value={accountId} onChange={e => { setAccountId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All accounts</option>
            {accounts.filter(a => a.isActive).map(a => <option key={a.id} value={a.id}>{a.name}{a.currency ? ` (${a.currency.code})` : ''}</option>)}
          </select>
          <select value={categoryId} onChange={e => { setCategoryId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All categories</option>
            {sortByFavorites(categories.filter(c => c.isActive), favCats).map(c => <option key={c.id} value={c.id}>{favCats.has(c.id) ? `★ ${c.name}` : c.name}</option>)}
          </select>
          <select value={counterpartyId} onChange={e => { setCounterpartyId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All counterparties</option>
            {sortByFavorites(counterparties.filter(c => c.isActive), favCps).map(c => <option key={c.id} value={c.id}>{favCps.has(c.id) ? `★ ${c.name}` : c.name}</option>)}
          </select>
          <select value={paymentStatusId} onChange={e => { setPaymentStatusId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All statuses</option>
            {paymentStatuses.filter(s => s.isActive).map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select value={inCurrencyId} onChange={e => { setInCurrencyId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All base currencies</option>
            {currencies.filter(c => c.isActive).map(c => <option key={c.id} value={c.id}>{c.symbol} {c.code}</option>)}
          </select>
        </div>
      </div>

      {/* Modal */}
      {modal !== null && (
        <Modal title={modal === 'create' ? 'New Transaction' : 'Edit Transaction'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelCls}>Date</label>
                <input required type="date" value={form.dateTime} onChange={setField('dateTime')} className={inputCls} />
              </div>
              <div>
                <label className={labelCls}>Account</label>
                <select required value={form.accountId} onChange={onAccountChange} className={inputCls}>
                  <option value="">Select account</option>
                  {activeOrCurrent(accounts, form.accountId).map(a => <option key={a.id} value={a.id}>{a.name}{a.currency ? ` (${a.currency.code})` : ''}</option>)}
                </select>
              </div>
              <div>
                <label className={labelCls}>Category</label>
                <select required value={form.categoryId} onChange={setField('categoryId')} className={inputCls}>
                  <option value="">Select category</option>
                  {sortByFavorites(activeOrCurrent(categories, form.categoryId), favCats).map(c => <option key={c.id} value={c.id}>{favCats.has(c.id) ? `★ ${c.name}` : c.name}</option>)}
                </select>
              </div>
              <div>
                <label className={labelCls}>Payment Status</label>
                <select required value={form.paymentStatusId} onChange={setField('paymentStatusId')} className={inputCls}>
                  <option value="">Select status</option>
                  {activeOrCurrent(paymentStatuses, form.paymentStatusId).map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              <div>
                <label className={labelCls}>Sum</label>
                <input required type="number" min="0.01" step="0.01" value={form.sum} onChange={setField('sum')} className={inputCls} placeholder="0.00" />
              </div>
              <div>
                <label className={labelCls}>Exchange Rate</label>
                <input required type="number" min="0.000001" step="0.000001" value={form.exchangeRate} onChange={setField('exchangeRate')} className={inputCls} />
              </div>
              <div>
                <label className={labelCls}>Currency</label>
                <input
                  disabled
                  value={(() => { const c = accounts.find(a => a.id === form.accountId)?.currency; return c ? `${c.symbol} ${c.code}` : '—' })()}
                  className={`${inputCls} opacity-50 cursor-not-allowed`}
                />
              </div>
              <div>
                <label className={labelCls}>In Main Currency</label>
                <input
                  disabled
                  value={(() => { const r = parseFloat(form.exchangeRate); const v = parseFloat(form.sum) / r; return isNaN(v) || !isFinite(v) ? '—' : num(v) })()}
                  className={`${inputCls} opacity-50 cursor-not-allowed`}
                />
              </div>
              <div>
                <label className={labelCls}>Counterparty <span className="text-gray-400 font-normal">(optional)</span></label>
                <select value={form.counterpartyId} onChange={setField('counterpartyId')} className={inputCls}>
                  <option value="">None</option>
                  {sortByFavorites(activeOrCurrent(counterparties, form.counterpartyId), favCps).map(c => <option key={c.id} value={c.id}>{favCps.has(c.id) ? `★ ${c.name}` : c.name}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className={labelCls}>Notes <span className="text-gray-400 font-normal">(optional)</span></label>
              <textarea
                value={form.notes} onChange={setField('notes')} maxLength={500} rows={2}
                className={`${inputCls} resize-none`} placeholder="…"
              />
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100">
                Cancel
              </button>
              <button type="submit" disabled={saving} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
                {saving ? 'Saving…' : modal === 'create' ? 'Create' : 'Save'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Table */}
      <div className="flex flex-col flex-1 min-h-0 mx-8 mb-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <div className="flex-1 min-h-0 overflow-y-auto overflow-x-hidden">
          <table className="w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <SortableHeader label="Date & Time" field="datetime" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Account</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Category</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Counterparty</th>
                <SortableHeader label="Sum" field="sum" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Exch. Rate</th>
                <SortableHeader label="In Main Currency" field="amountincurrency" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Status</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Notes</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(tx => (
                <tr key={tx.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400 font-mono text-xs">
                    {fmt(tx.dateTime)}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100">
                    <span className="font-medium">{tx.account?.name ?? '—'}</span>
                    {tx.account?.currency && (
                      <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">({tx.account.currency.code})</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                    {tx.category ? (
                      <span className="flex items-center gap-1.5">
                        {tx.category.color && <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: tx.category.color }} />}
                        {tx.category.icon && <span className="text-xs">{tx.category.icon}</span>}
                        {tx.category.name}
                      </span>
                    ) : '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                    {tx.counterparty?.name ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono text-right">
                    <span className="text-gray-400 dark:text-gray-500 mr-0.5 text-xs">{tx.account?.currency?.symbol ?? ''}</span>
                    {num(tx.sum)}
                  </td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400 font-mono text-xs text-right">
                    {num(tx.exchangeRate, 4)}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono text-right">
                    <span className="text-gray-400 dark:text-gray-500 mr-0.5 text-xs">{mainCurrency?.symbol ?? ''}</span>
                    {num(tx.amountInCurrency)}
                    {mainCurrency && <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">{mainCurrency.code}</span>}
                  </td>
                  <td className="px-4 py-3">
                    {tx.paymentStatus ? (
                      <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-indigo-100 text-indigo-700 dark:bg-indigo-900 dark:text-indigo-300 whitespace-nowrap">
                        {tx.paymentStatus.name}
                      </span>
                    ) : '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-[160px] truncate" title={tx.notes ?? ''}>
                    {tx.notes ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                  </td>
                  <td className="px-4 py-3 text-right whitespace-nowrap">
                    <div className="inline-flex items-center gap-1">
                      <IconButton icon={<InfoIcon />} label="Details" onClick={() => setSelectedTx(tx)}
                        className="text-gray-400 hover:text-blue-500 dark:hover:text-blue-400" />
                      <IconButton icon={<PencilIcon />} label="Edit" onClick={() => openEdit(tx)}
                        className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton icon={<TrashIcon />} label="Delete" onClick={() => handleDelete(tx)}
                        className="text-gray-400 hover:text-red-500 dark:hover:text-red-400" />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={10} className="px-4 py-12 text-center text-gray-400 dark:text-gray-500">
                    No transactions found.
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
      <TransactionPanel transaction={selectedTx} onClose={() => setSelectedTx(null)} />
    </div>
  )
}
