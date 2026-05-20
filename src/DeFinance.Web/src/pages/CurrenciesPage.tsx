import { useEffect, useState } from 'react'
import { useNotify } from '../NotificationContext'
import { currenciesApi, type Currency } from '../api/currencies'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'

type ModalState = null | 'create' | Currency

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

export function CurrenciesPage() {
  const notify = useNotify()
  const [result, setResult] = useState<PagedResult<Currency> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [formCode, setFormCode] = useState('')
  const [formName, setFormName] = useState('')
  const [formSymbol, setFormSymbol] = useState('')

  // filters & pagination
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
    currenciesApi.getAll({
      search: debouncedSearch || undefined,
      isActive: isActiveFilter !== '' ? isActiveFilter === 'true' : undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    })
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError('Failed to load currencies') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debouncedSearch, isActiveFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  const refetch = () => setRefreshKey(k => k + 1)

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const handleIsActiveChange = (v: string) => { setIsActiveFilter(v); setPage(1) }

  const openCreate = () => {
    setFormCode(''); setFormName(''); setFormSymbol(''); setFormError(null); setModal('create')
  }

  const openEdit = (c: Currency) => {
    setFormName(c.name); setFormSymbol(c.symbol); setFormError(null); setModal(c)
  }

  const closeModal = () => setModal(null)
  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      if (modal === 'create') {
        await currenciesApi.create({ code: formCode, name: formName, symbol: formSymbol })
        notify('Currency created', 'success')
      } else if (modal !== null) {
        await currenciesApi.update(modal.id, { name: formName, symbol: formSymbol })
        notify('Currency updated', 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (currency: Currency) => {
    if (currency.isActive) { await currenciesApi.deactivate(currency.id); notify('Currency deactivated', 'error') }
    else { await currenciesApi.activate(currency.id); notify('Currency activated', 'success') }
    refetch()
  }

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Currencies</h1>
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            + New Currency
          </button>
        </div>
        <div className="flex items-center gap-3">
          <input
            type="search"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search by name, code or symbol…"
            className={`${filterCls} w-64`}
          />
          <select value={isActiveFilter} onChange={e => handleIsActiveChange(e.target.value)} className={filterCls}>
            <option value="">All statuses</option>
            <option value="true">Active only</option>
            <option value="false">Inactive only</option>
          </select>
          {loading && <span className="text-xs text-gray-400 dark:text-gray-500">Loading…</span>}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Currency' : 'New Currency'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!isEditing && (
              <div>
                <label className={labelCls}>Code</label>
                <input required maxLength={10} value={formCode} onChange={e => setFormCode(e.target.value.toUpperCase())} className={inputCls} placeholder="USD" />
              </div>
            )}
            <div>
              <label className={labelCls}>Name</label>
              <input required maxLength={100} value={formName} onChange={e => setFormName(e.target.value)} className={inputCls} placeholder="US Dollar" />
            </div>
            <div>
              <label className={labelCls}>Symbol</label>
              <input required maxLength={10} value={formSymbol} onChange={e => setFormSymbol(e.target.value)} className={inputCls} placeholder="$" />
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
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Symbol</th>
                <SortableHeader label="Code" field="code" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <SortableHeader label="Name" field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(currency => (
                <tr key={currency.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 font-bold text-gray-700 dark:text-gray-300 w-12">{currency.symbol}</td>
                  <td className="px-4 py-3 font-mono text-gray-900 dark:text-gray-100">{currency.code}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{currency.name}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${currency.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                      {currency.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton icon={<PencilIcon />} label="Edit" onClick={() => openEdit(currency)} className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton icon={currency.isActive ? <BanIcon /> : <CheckCircleIcon />} label={currency.isActive ? 'Deactivate' : 'Activate'} onClick={() => toggle(currency)} className={currency.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'} />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">No currencies found.</td>
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
