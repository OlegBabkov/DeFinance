import { useEffect, useState, type ReactNode } from 'react'
import { useNotify } from '../NotificationContext'
import { useMainCurrency } from '../MainCurrencyContext'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
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
                <tr key={s.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
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

export function AdministrationPage() {
  return (
    <div className="p-6 h-full flex flex-col">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Administration</h1>

      <div className="flex-1 grid grid-cols-2 grid-rows-2 gap-4 min-h-0">
        <Panel title="Payment Statuses">
          <PaymentStatusesPanel />
        </Panel>

        <Panel title="Main Currency">
          <MainCurrencyPanel />
        </Panel>

        <Panel title="" empty />
        <Panel title="" empty />
      </div>
    </div>
  )
}

function Panel({ title, children, empty }: { title: string; children?: ReactNode; empty?: boolean }) {
  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex flex-col overflow-hidden">
      {!empty && (
        <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">{title}</h2>
        </div>
      )}
      <div className="flex-1 p-4 overflow-auto flex flex-col min-h-0">
        {empty ? (
          <div className="h-full flex items-center justify-center text-gray-300 dark:text-gray-600 text-sm select-none">
            —
          </div>
        ) : (
          children
        )}
      </div>
    </div>
  )
}
