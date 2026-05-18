import { useEffect, useState } from 'react'
import { transactionsApi, type Transaction } from '../api/transactions'
import { accountsApi, type Account } from '../api/accounts'
import { categoriesApi, type Category } from '../api/categories'
import { counterpartiesApi, type Counterparty } from '../api/counterparties'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { currenciesApi, type Currency } from '../api/currencies'
import { type PagedResult, type PageSize } from '../api/common'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

function fmt(dateStr: string) {
  const d = new Date(dateStr)
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) +
    ' ' + d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
}

function num(n: number, decimals = 2) {
  return n.toLocaleString('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

export function TransactionsPage() {
  const [result, setResult] = useState<PagedResult<Transaction> | null>(null)
  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [counterparties, setCounterparties] = useState<Counterparty[]>([])
  const [paymentStatuses, setPaymentStatuses] = useState<PaymentStatus[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
    inCurrencyId, debouncedNotes, page, pageSize, sortBy, sortDirection])

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

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Transactions</h1>
          {loading && <span className="text-xs text-gray-400 dark:text-gray-500">Loading…</span>}
        </div>

        {/* Filters — row 1: dates + notes */}
        <div className="flex flex-wrap items-center gap-3 mb-2">
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-500 dark:text-gray-400">From</span>
            <input
              type="date"
              value={dateFrom}
              onChange={e => { setDateFrom(e.target.value); setPage(1) }}
              className={filterCls}
            />
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-500 dark:text-gray-400">To</span>
            <input
              type="date"
              value={dateTo}
              onChange={e => { setDateTo(e.target.value); setPage(1) }}
              className={filterCls}
            />
          </div>
          <input
            type="search"
            value={notes}
            onChange={e => setNotes(e.target.value)}
            placeholder="Search notes…"
            className={`${filterCls} w-44`}
          />
          {hasFilters && (
            <button onClick={resetFilters} className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline">
              Clear filters
            </button>
          )}
        </div>

        {/* Filters — row 2: dropdowns */}
        <div className="flex flex-wrap items-center gap-3">
          <select value={accountId} onChange={e => { setAccountId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All accounts</option>
            {accounts.map(a => (
              <option key={a.id} value={a.id}>{a.name}{a.currency ? ` (${a.currency.code})` : ''}</option>
            ))}
          </select>
          <select value={categoryId} onChange={e => { setCategoryId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All categories</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <select value={counterpartyId} onChange={e => { setCounterpartyId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All counterparties</option>
            {counterparties.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <select value={paymentStatusId} onChange={e => { setPaymentStatusId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All statuses</option>
            {paymentStatuses.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select value={inCurrencyId} onChange={e => { setInCurrencyId(e.target.value); setPage(1) }} className={filterCls}>
            <option value="">All base currencies</option>
            {currencies.map(c => <option key={c.id} value={c.id}>{c.symbol} {c.code}</option>)}
          </select>
        </div>
      </div>

      {/* Table */}
      <div className="flex flex-col flex-1 min-h-0 mx-8 mb-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <div className="flex-1 min-h-0 overflow-auto">
          <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <SortableHeader label="Date & Time" field="datetime" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400 whitespace-nowrap">Account</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Category</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Counterparty</th>
                <SortableHeader label="Sum" field="sum" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400 whitespace-nowrap">Exch. Rate</th>
                <SortableHeader label="In Currency" field="amountincurrency" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400 whitespace-nowrap">Status</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Notes</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(tx => (
                <tr key={tx.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400 font-mono text-xs whitespace-nowrap">
                    {fmt(tx.dateTime)}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100 whitespace-nowrap">
                    <span className="font-medium">{tx.account?.name ?? '—'}</span>
                    {tx.account?.currency && (
                      <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">
                        ({tx.account.currency.code})
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                    {tx.category ? (
                      <span className="flex items-center gap-1.5">
                        {tx.category.color && (
                          <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: tx.category.color }} />
                        )}
                        {tx.category.icon && <span className="text-xs">{tx.category.icon}</span>}
                        {tx.category.name}
                      </span>
                    ) : '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                    {tx.counterparty?.name ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono whitespace-nowrap text-right">
                    <span className="text-gray-400 dark:text-gray-500 mr-0.5 text-xs">
                      {tx.account?.currency?.symbol ?? ''}
                    </span>
                    {num(tx.sum)}
                  </td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400 font-mono text-xs text-right">
                    {num(tx.exchangeRate, 4)}
                  </td>
                  <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono whitespace-nowrap text-right">
                    <span className="text-gray-400 dark:text-gray-500 mr-0.5 text-xs">
                      {tx.inCurrency?.symbol ?? ''}
                    </span>
                    {num(tx.amountInCurrency)}
                    {tx.inCurrency && (
                      <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">{tx.inCurrency.code}</span>
                    )}
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
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={9} className="px-4 py-12 text-center text-gray-400 dark:text-gray-500">
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
    </div>
  )
}
