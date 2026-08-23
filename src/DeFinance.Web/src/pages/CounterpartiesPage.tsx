import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNotify } from '../NotificationContext'
import { counterpartiesApi, type Counterparty, type CounterpartyType } from '../api/counterparties'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon, StarIcon, StarFilledIcon, InfoIcon } from '../components/IconButton'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'
import { useFavorites } from '../hooks/useFavorites'
import { usePersistedState } from '../hooks/usePersistedState'
import { CounterpartyPanel } from '../components/CounterpartyPanel'
import { Spinner } from '../components/Spinner'

type ModalState = null | 'create' | Counterparty

const COUNTERPARTY_TYPES: CounterpartyType[] = ['Person', 'Company', 'Other']

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

export function CounterpartiesPage() {
  const { t } = useTranslation()
  const notify = useNotify()
  const { isFavorite, toggle: toggleFav } = useFavorites('counterparties')
  const [result, setResult] = useState<PagedResult<Counterparty> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [selectedCounterparty, setSelectedCounterparty] = useState<Counterparty | null>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [formName, setFormName] = useState('')
  const [formType, setFormType] = useState<CounterpartyType>('Person')
  const [formContactInfo, setFormContactInfo] = useState('')

  // filters & pagination (persisted)
  const [search, setSearch] = usePersistedState('cp_filter_search', '')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = usePersistedState('cp_filter_isActive', '')
  const [typeFilter, setTypeFilter] = usePersistedState('cp_filter_type', '')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = usePersistedState<PageSize>('cp_filter_pageSize', 100)
  const [sortBy, setSortBy] = usePersistedState<string | null>('cp_filter_sortBy', null)
  const [sortDirection, setSortDirection] = usePersistedState<SortDirection>('cp_filter_sortDirection', 'Asc')
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1) }, 400)
    return () => clearTimeout(t)
  }, [search])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    counterpartiesApi.getAll({
      search: debouncedSearch || undefined,
      isActive: isActiveFilter !== '' ? isActiveFilter === 'true' : undefined,
      type: typeFilter !== '' ? (typeFilter as CounterpartyType) : undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    })
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError(t('counterparties.error.loadFailed')) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debouncedSearch, isActiveFilter, typeFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  const refetch = () => setRefreshKey(k => k + 1)

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const handleIsActiveChange = (v: string) => { setIsActiveFilter(v); setPage(1) }
  const handleTypeChange = (v: string) => { setTypeFilter(v); setPage(1) }

  const openCreate = () => {
    setFormName(''); setFormType('Person'); setFormContactInfo(''); setFormError(null); setModal('create')
  }

  const openEdit = (c: Counterparty) => {
    setFormName(c.name); setFormType(c.type); setFormContactInfo(c.contactInfo ?? ''); setFormError(null); setModal(c)
  }

  const closeModal = () => setModal(null)
  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      const contactInfo = formContactInfo.trim() || null
      if (modal === 'create') {
        await counterpartiesApi.create({ name: formName, type: formType, contactInfo })
        notify(t('counterparties.notify.created'), 'success')
      } else if (modal !== null) {
        await counterpartiesApi.update(modal.id, { name: formName, type: formType, contactInfo })
        notify(t('counterparties.notify.updated'), 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError(t('counterparties.error.saveFailed'))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (counterparty: Counterparty) => {
    if (counterparty.isActive) { await counterpartiesApi.deactivate(counterparty.id); notify(t('counterparties.notify.deactivated'), 'error') }
    else { await counterpartiesApi.activate(counterparty.id); notify(t('counterparties.notify.activated'), 'success') }
    refetch()
  }

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 flex justify-center text-gray-400 dark:text-gray-500"><Spinner /></div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-4 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">{t('counterparties.title')}</h1>
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            {t('counterparties.button.new')}
          </button>
        </div>
        <div className="flex items-center gap-3">
          <input
            type="search"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('counterparties.filter.searchByName')}
            className={`${filterCls} w-60`}
          />
          <select value={isActiveFilter} onChange={e => handleIsActiveChange(e.target.value)} className={filterCls}>
            <option value="">{t('counterparties.filter.allStatuses')}</option>
            <option value="true">{t('counterparties.filter.activeOnly')}</option>
            <option value="false">{t('counterparties.filter.inactiveOnly')}</option>
          </select>
          <select value={typeFilter} onChange={e => handleTypeChange(e.target.value)} className={filterCls}>
            <option value="">{t('counterparties.filter.allTypes')}</option>
            {COUNTERPARTY_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
          </select>
          {loading && <Spinner size="sm" />}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? t('counterparties.modal.editTitle') : t('counterparties.modal.newTitle')} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>{t('counterparties.form.name')}</label>
              <input required maxLength={100} value={formName} onChange={e => setFormName(e.target.value)} className={inputCls} placeholder={t('counterparties.form.namePlaceholder')} />
            </div>
            <div>
              <label className={labelCls}>{t('counterparties.form.type')}</label>
              <select value={formType} onChange={e => setFormType(e.target.value as CounterpartyType)} className={inputCls}>
                {COUNTERPARTY_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>{t('counterparties.form.contactInfo')}</label>
              <textarea maxLength={500} value={formContactInfo} onChange={e => setFormContactInfo(e.target.value)} className={`${inputCls} resize-none`} rows={3} placeholder={t('counterparties.form.contactInfoPlaceholder')} />
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100">{t('counterparties.button.cancel')}</button>
              <button type="submit" disabled={saving} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
                {saving ? t('counterparties.button.saving') : isEditing ? t('counterparties.button.save') : t('counterparties.button.create')}
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
                <SortableHeader label={t('counterparties.table.name')} field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <SortableHeader label={t('counterparties.table.type')} field="type" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('counterparties.table.contactInfo')}</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('counterparties.table.status')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(cp => (
                <tr key={cp.id} className={cp.isActive ? 'hover:bg-gray-50 dark:hover:bg-gray-700' : 'bg-gray-100 dark:bg-gray-900/50'}>
                  <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{cp.name}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{cp.type}</td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-xs truncate">
                    {cp.contactInfo ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${cp.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                      {cp.isActive ? t('counterparties.status.active') : t('counterparties.status.inactive')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton
                        icon={isFavorite(cp.id) ? <StarFilledIcon /> : <StarIcon />}
                        label={isFavorite(cp.id) ? t('counterparties.action.removeFromFavourites') : t('counterparties.action.addToFavourites')}
                        onClick={() => {
                          const added = toggleFav(cp.id)
                          notify(added ? t('counterparties.notify.addedToFavourites', { name: cp.name }) : t('counterparties.notify.removedFromFavourites', { name: cp.name }), added ? 'success' : 'info')
                        }}
                        className={isFavorite(cp.id) ? 'text-amber-400 hover:text-amber-500' : 'text-gray-300 hover:text-amber-400 dark:text-gray-600 dark:hover:text-amber-400'}
                      />
                      <IconButton icon={<InfoIcon />} label={t('counterparties.action.details')} onClick={() => setSelectedCounterparty(cp)} className="text-gray-400 hover:text-blue-500 dark:hover:text-blue-400" />
                      <IconButton icon={<PencilIcon />} label={t('counterparties.action.edit')} onClick={() => openEdit(cp)} className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton icon={cp.isActive ? <BanIcon /> : <CheckCircleIcon />} label={cp.isActive ? t('counterparties.action.deactivate') : t('counterparties.action.activate')} onClick={() => toggle(cp)} className={cp.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'} />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">{t('counterparties.table.empty')}</td>
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
      <CounterpartyPanel counterparty={selectedCounterparty} onClose={() => setSelectedCounterparty(null)} />
    </div>
  )
}
