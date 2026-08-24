import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usePersistedState } from '../hooks/usePersistedState'
import { useNotify } from '../NotificationContext'
import { currenciesApi, type Currency } from '../api/currencies'
import { exchangeRatesApi, type ExchangeRateLatest } from '../api/exchangeRates'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'
import { Spinner } from '../components/Spinner'

type ModalState = null | 'create' | Currency

function RateCell({ rate }: { rate: ExchangeRateLatest }) {
  const trend = rate.previousRate !== null
    ? rate.rate > rate.previousRate ? 'up' : rate.rate < rate.previousRate ? 'down' : 'flat'
    : 'flat'
  return (
    <span className="inline-flex items-center gap-1">
      <span className="text-gray-900 dark:text-gray-100">{rate.rate.toFixed(4)}</span>
      {trend === 'up' && <span className="text-green-500 text-xs">↑</span>}
      {trend === 'down' && <span className="text-red-500 text-xs">↓</span>}
    </span>
  )
}

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

export function CurrenciesPage() {
  const { t } = useTranslation()
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

  // filters & pagination (persisted)
  const [search, setSearch] = usePersistedState('cur_filter_search', '')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = usePersistedState('cur_filter_isActive', '')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = usePersistedState<PageSize>('cur_filter_pageSize', 100)
  const [sortBy, setSortBy] = usePersistedState<string | null>('cur_filter_sortBy', null)
  const [sortDirection, setSortDirection] = usePersistedState<SortDirection>('cur_filter_sortDirection', 'Asc')
  const [refreshKey, setRefreshKey] = useState(0)
  const [rates, setRates] = useState<Record<string, ExchangeRateLatest>>({})
  const [syncing, setSyncing] = useState(false)

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
      .catch(() => { if (!cancelled) setError(t('currencies.error.loadFailed')) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debouncedSearch, isActiveFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  useEffect(() => {
    exchangeRatesApi.getLatest()
      .then(list => {
        const map: Record<string, ExchangeRateLatest> = {}
        list.forEach(r => { map[r.currencyCode] = r })
        setRates(map)
      })
      .catch(() => {})
  }, [refreshKey])

  const handleSync = async () => {
    setSyncing(true)
    try {
      const { synced } = await exchangeRatesApi.sync()
      notify(t('currencies.notify.synced', { n: synced }), 'success')
      refetch()
    } catch {
      notify(t('currencies.notify.syncFailed'), 'error')
    } finally {
      setSyncing(false)
    }
  }

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
        notify(t('currencies.notify.created'), 'success')
      } else if (modal !== null) {
        await currenciesApi.update(modal.id, { name: formName, symbol: formSymbol })
        notify(t('currencies.notify.updated'), 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError(t('currencies.error.saveFailed'))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (currency: Currency) => {
    if (currency.isActive) { await currenciesApi.deactivate(currency.id); notify(t('currencies.notify.deactivated'), 'error') }
    else { await currenciesApi.activate(currency.id); notify(t('currencies.notify.activated'), 'success') }
    refetch()
  }

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 flex justify-center text-gray-400 dark:text-gray-500"><Spinner /></div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">{t('currencies.title')}</h1>
          <div className="flex items-center gap-2">
            <button
              onClick={handleSync}
              disabled={syncing}
              className="px-4 py-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
            >
              {syncing ? t('currencies.button.syncing') : t('currencies.button.syncRates')}
            </button>
            <button
              onClick={openCreate}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
            >
              {t('currencies.button.new')}
            </button>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <input
            type="search"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('currencies.filter.searchByName')}
            className={`${filterCls} w-64`}
          />
          <select value={isActiveFilter} onChange={e => handleIsActiveChange(e.target.value)} className={filterCls}>
            <option value="">{t('currencies.filter.allStatuses')}</option>
            <option value="true">{t('currencies.filter.activeOnly')}</option>
            <option value="false">{t('currencies.filter.inactiveOnly')}</option>
          </select>
          {loading && <Spinner size="sm" />}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? t('currencies.modal.editTitle') : t('currencies.modal.newTitle')} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!isEditing && (
              <div>
                <label className={labelCls}>{t('currencies.form.code')}</label>
                <input required maxLength={10} value={formCode} onChange={e => setFormCode(e.target.value.toUpperCase())} className={inputCls} placeholder={t('currencies.form.codePlaceholder')} />
              </div>
            )}
            <div>
              <label className={labelCls}>{t('currencies.form.name')}</label>
              <input required maxLength={100} value={formName} onChange={e => setFormName(e.target.value)} className={inputCls} placeholder={t('currencies.form.namePlaceholder')} />
            </div>
            <div>
              <label className={labelCls}>{t('currencies.form.symbol')}</label>
              <input required maxLength={10} value={formSymbol} onChange={e => setFormSymbol(e.target.value)} className={inputCls} placeholder={t('currencies.form.symbolPlaceholder')} />
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100">{t('currencies.button.cancel')}</button>
              <button type="submit" disabled={saving} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
                {saving ? t('currencies.button.saving') : isEditing ? t('currencies.button.save') : t('currencies.button.create')}
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
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('currencies.table.symbol')}</th>
                <SortableHeader label={t('currencies.table.code')} field="code" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <SortableHeader label={t('currencies.table.name')} field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('currencies.table.rateEur')}</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('currencies.table.status')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(currency => (
                <tr key={currency.id} className={currency.isActive ? 'hover:bg-gray-50 dark:hover:bg-gray-700' : 'bg-gray-100 dark:bg-gray-900/50'}>
                  <td className="px-4 py-3 font-bold text-gray-700 dark:text-gray-300 w-12">{currency.symbol}</td>
                  <td className="px-4 py-3 font-mono text-gray-900 dark:text-gray-100">{currency.code}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{currency.name}</td>
                  <td className="px-4 py-3 font-mono text-sm">
                    {currency.code === 'EUR'
                      ? <span className="text-gray-400 dark:text-gray-500">{t('currencies.table.rateBase')}</span>
                      : rates[currency.code]
                        ? <RateCell rate={rates[currency.code]} />
                        : <span className="text-gray-300 dark:text-gray-600">—</span>
                    }
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${currency.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                      {currency.isActive ? t('currencies.status.active') : t('currencies.status.inactive')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton icon={<PencilIcon />} label={t('currencies.action.edit')} onClick={() => openEdit(currency)} className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton icon={currency.isActive ? <BanIcon /> : <CheckCircleIcon />} label={currency.isActive ? t('currencies.action.deactivate') : t('currencies.action.activate')} onClick={() => toggle(currency)} className={currency.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'} />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">{t('currencies.table.empty')}</td>
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
