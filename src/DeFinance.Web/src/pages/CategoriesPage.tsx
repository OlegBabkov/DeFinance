import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNotify } from '../NotificationContext'
import {
  categoriesApi,
  type Category,
  type CategoryType,
  type CategoryPaymentObligation,
  PAYMENT_OBLIGATION_LABELS,
} from '../api/categories'
import { type PagedResult, type PageSize, type SortDirection } from '../api/common'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon, StarIcon, StarFilledIcon, InfoIcon } from '../components/IconButton'
import { EmojiPicker } from '../components/EmojiPicker'
import { Pagination } from '../components/Pagination'
import { SortableHeader } from '../components/SortableHeader'
import { Spinner } from '../components/Spinner'
import { useFavorites } from '../hooks/useFavorites'
import { usePersistedState } from '../hooks/usePersistedState'
import { CategoryPanel } from '../components/CategoryPanel'

const PAYMENT_OBLIGATIONS: { value: CategoryPaymentObligation; label: string }[] = [
  { value: 'SepaTransfer', label: PAYMENT_OBLIGATION_LABELS.SepaTransfer },
  { value: 'Mandatory', label: PAYMENT_OBLIGATION_LABELS.Mandatory },
  { value: 'NonMandatory', label: PAYMENT_OBLIGATION_LABELS.NonMandatory },
]

type Tab = 'Income' | 'Expense' | 'Transfer'
type ModalState = null | 'create' | Category

const TABS: { id: Tab; label: string }[] = [
  { id: 'Income', label: 'categories.tab.income' },
  { id: 'Expense', label: 'categories.tab.losses' },
  { id: 'Transfer', label: 'categories.tab.transfers' },
]

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

const filterCls =
  'px-3 py-1.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

export function CategoriesPage() {
  const { t } = useTranslation()
  const notify = useNotify()
  const { isFavorite, toggle: toggleFav } = useFavorites('categories')
  const [result, setResult] = useState<PagedResult<Category> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [tab, setTab] = usePersistedState<Tab>('cat_filter_tab', 'Income')
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [formName, setFormName] = useState('')
  const [formColor, setFormColor] = useState('')
  const [formIcon, setFormIcon] = useState('')
  const [formParentId, setFormParentId] = useState('')
  const [formPaymentObligation, setFormPaymentObligation] = useState<CategoryPaymentObligation | ''>('')
  const [formTransferType, setFormTransferType] = useState<'TransferIn' | 'TransferOut'>('TransferIn')
  const [parentOptions, setParentOptions] = useState<Category[]>([])

  // filters & pagination (persisted)
  const [search, setSearch] = usePersistedState('cat_filter_search', '')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = usePersistedState('cat_filter_isActive', '')
  const [obligationFilter, setObligationFilter] = usePersistedState('cat_filter_obligation', '')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = usePersistedState<PageSize>('cat_filter_pageSize', 100)
  const [sortBy, setSortBy] = usePersistedState<string | null>('cat_filter_sortBy', null)
  const [sortDirection, setSortDirection] = usePersistedState<SortDirection>('cat_filter_sortDirection', 'Asc')
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1) }, 400)
    return () => clearTimeout(t)
  }, [search])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    const baseParams = {
      search: debouncedSearch || undefined,
      isActive: isActiveFilter !== '' ? isActiveFilter === 'true' : undefined,
      paymentObligation: obligationFilter !== '' ? (obligationFilter as CategoryPaymentObligation) : undefined,
      page,
      pageSize,
      sortBy: sortBy ?? undefined,
      sortDirection,
    }
    const fetch = tab === 'Transfer'
      ? Promise.all([
          categoriesApi.getAll({ ...baseParams, type: 'TransferIn' }),
          categoriesApi.getAll({ ...baseParams, type: 'TransferOut' }),
        ]).then(([a, b]) => {
          const items = [...a.items, ...b.items].sort((x, y) => x.name.localeCompare(y.name))
          return { items, totalCount: items.length, page: 1, pageSize: items.length || 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false }
        })
      : categoriesApi.getAll({ ...baseParams, type: tab })
    fetch
      .then(r => { if (!cancelled) { setResult(r); setError(null) } })
      .catch(() => { if (!cancelled) setError(t('categories.error.loadFailed')) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [tab, debouncedSearch, isActiveFilter, obligationFilter, page, pageSize, sortBy, sortDirection, refreshKey])

  const refetch = () => setRefreshKey(k => k + 1)

  const handleSort = (field: string) => {
    if (sortBy === field) setSortDirection(d => d === 'Asc' ? 'Desc' : 'Asc')
    else { setSortBy(field); setSortDirection('Asc') }
    setPage(1)
  }

  const handleTabChange = (t: Tab) => { setTab(t); setPage(1) }
  const handleIsActiveChange = (v: string) => { setIsActiveFilter(v); setPage(1) }
  const handleObligationChange = (v: string) => { setObligationFilter(v); setPage(1) }

  const openCreate = async () => {
    setFormName(''); setFormColor('#6366f1'); setFormIcon(''); setFormParentId('')
    setFormPaymentObligation(''); setFormError(null)
    if (tab !== 'Transfer') {
      const r = await categoriesApi.getAll({ type: tab, pageSize: 500 })
      setParentOptions(r.items)
    } else {
      setParentOptions([])
    }
    setModal('create')
  }

  const openEdit = (c: Category) => {
    setFormName(c.name); setFormColor(c.color ?? '#6366f1'); setFormIcon(c.icon ?? '')
    setFormPaymentObligation(c.paymentObligation ?? ''); setFormError(null); setModal(c)
  }

  const closeModal = () => setModal(null)
  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      const color = formColor || null
      const icon = formIcon.trim() || null
      const paymentObligation = formPaymentObligation || null
      const categoryType: CategoryType = tab === 'Transfer' ? formTransferType : (tab as CategoryType)
      if (modal === 'create') {
        await categoriesApi.create({ name: formName, type: categoryType, color, icon, parentId: formParentId || null, paymentObligation })
        notify(t('categories.notify.created'), 'success')
      } else if (modal !== null) {
        await categoriesApi.update(modal.id, { name: formName, color, icon, paymentObligation })
        notify(t('categories.notify.updated'), 'info')
      }
      closeModal()
      refetch()
    } catch {
      setFormError(t('categories.error.saveFailed'))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (category: Category) => {
    if (category.isActive) { await categoriesApi.deactivate(category.id); notify(t('categories.notify.deactivated'), 'error') }
    else { await categoriesApi.activate(category.id); notify(t('categories.notify.activated'), 'success') }
    refetch()
  }

  const parentName = (parentId: string | null) =>
    parentId ? (result?.items.find(c => c.id === parentId)?.name ?? '—') : '—'

  const items = result?.items ?? []

  if (!result && loading) return <div className="p-8 flex justify-center text-gray-400 dark:text-gray-500"><Spinner /></div>
  if (error && !result) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-0 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">{t('categories.title')}</h1>
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            {t('categories.button.newCategory')}
          </button>
        </div>
        <div className="flex items-center gap-3 mb-3">
          <input
            type="search"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('categories.filter.searchByName')}
            className={`${filterCls} w-52`}
          />
          <select value={isActiveFilter} onChange={e => handleIsActiveChange(e.target.value)} className={filterCls}>
            <option value="">{t('categories.filter.allStatuses')}</option>
            <option value="true">{t('categories.filter.activeOnly')}</option>
            <option value="false">{t('categories.filter.inactiveOnly')}</option>
          </select>
          {tab !== 'Transfer' && (
            <select value={obligationFilter} onChange={e => handleObligationChange(e.target.value)} className={filterCls}>
              <option value="">{t('categories.filter.allObligations')}</option>
              {PAYMENT_OBLIGATIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          )}
          {loading && <Spinner size="sm" />}
        </div>
        <div className="flex border-b border-gray-200 dark:border-gray-700">
          {TABS.map(({ id, label }) => (
            <button
              key={id}
              onClick={() => handleTabChange(id)}
              className={`px-5 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors ${
                tab === id
                  ? 'border-indigo-600 text-indigo-600 dark:text-indigo-400 dark:border-indigo-400'
                  : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
            >
              {t(label)}
              {tab === id && result != null && (
                <span className="ml-2 rounded-full bg-gray-100 dark:bg-gray-700 px-2 py-0.5 text-xs text-gray-600 dark:text-gray-300">
                  {result.totalCount}
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? t('categories.modal.editTitle') : `New ${tab === 'Transfer' ? 'Transfer' : tab} Category`} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!isEditing && tab === 'Transfer' && (
              <div>
                <label className={labelCls}>{t('categories.form.transferType')}</label>
                <select value={formTransferType} onChange={e => setFormTransferType(e.target.value as 'TransferIn' | 'TransferOut')} className={inputCls}>
                  <option value="TransferIn">{t('categories.form.transferIn')}</option>
                  <option value="TransferOut">{t('categories.form.transferOut')}</option>
                </select>
              </div>
            )}
            <div>
              <label className={labelCls}>{t('categories.form.name')}</label>
              <input required maxLength={100} value={formName} onChange={e => setFormName(e.target.value)} className={inputCls} placeholder={t('categories.form.namePlaceholder')} />
            </div>
            <div className="flex gap-4">
              <div className="flex-1">
                <label className={labelCls}>{t('categories.form.icon')}</label>
                <EmojiPicker value={formIcon} onChange={setFormIcon} />
              </div>
              <div>
                <label className={labelCls}>{t('categories.form.color')}</label>
                <div className="flex items-center gap-2 mt-1">
                  <input type="color" value={formColor} onChange={e => setFormColor(e.target.value)} className="h-9 w-14 rounded border border-gray-300 dark:border-gray-600 cursor-pointer bg-white dark:bg-gray-700 p-0.5" />
                  <span className="text-xs font-mono text-gray-500 dark:text-gray-400">{formColor}</span>
                </div>
              </div>
            </div>
            {!isEditing && parentOptions.length > 0 && (
              <div>
                <label className={labelCls}>{t('categories.form.parentCategory')}</label>
                <select value={formParentId} onChange={e => setFormParentId(e.target.value)} className={inputCls}>
                  <option value="">{t('categories.form.noParent')}</option>
                  {parentOptions.map(c => (
                    <option key={c.id} value={c.id}>{c.icon ? `${c.icon} ` : ''}{c.name}</option>
                  ))}
                </select>
              </div>
            )}
            <div>
              <label className={labelCls}>{t('categories.form.paymentObligation')}</label>
              <select value={formPaymentObligation} onChange={e => setFormPaymentObligation(e.target.value as CategoryPaymentObligation | '')} className={inputCls}>
                <option value="">{t('categories.form.noObligation')}</option>
                {PAYMENT_OBLIGATIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            {formError && <p className="text-sm text-red-500">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100">{t('categories.button.cancel')}</button>
              <button type="submit" disabled={saving} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
                {saving ? t('categories.button.saving') : isEditing ? t('categories.button.save') : t('categories.button.create')}
              </button>
            </div>
          </form>
        </Modal>
      )}

      <div className="flex flex-col flex-1 min-h-0 mx-8 mb-4 mt-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <div className="flex-1 min-h-0 overflow-y-auto overflow-x-hidden">
          <table className="w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
              <tr>
                <SortableHeader label={t('categories.table.name')} field="name" sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} />
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('categories.table.color')}</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('categories.table.parent')}</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('categories.table.obligation')}</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">{t('categories.table.status')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {items.map(cat => (
                <tr key={cat.id} className={cat.isActive ? 'hover:bg-gray-50 dark:hover:bg-gray-700' : 'bg-gray-100 dark:bg-gray-900/50'}>
                  <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">
                    {cat.icon && <span className="mr-1">{cat.icon}</span>}
                    {cat.name}
                  </td>
                  <td className="px-4 py-3">
                    {cat.color ? (
                      <div className="flex items-center gap-2">
                        <span className="inline-block w-4 h-4 rounded-full border border-gray-200 dark:border-gray-600" style={{ backgroundColor: cat.color }} />
                        <span className="text-gray-500 dark:text-gray-400 font-mono text-xs">{cat.color}</span>
                      </div>
                    ) : (
                      <span className="text-gray-300 dark:text-gray-600">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-500 dark:text-gray-400">{parentName(cat.parentId)}</td>
                  <td className="px-4 py-3">
                    {cat.paymentObligation ? (
                      <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-indigo-100 text-indigo-700 dark:bg-indigo-900 dark:text-indigo-300">
                        {PAYMENT_OBLIGATION_LABELS[cat.paymentObligation]}
                      </span>
                    ) : (
                      <span className="text-gray-300 dark:text-gray-600">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${cat.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                      {cat.isActive ? t('categories.status.active') : t('categories.status.inactive')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="inline-flex items-center gap-1">
                      <IconButton
                        icon={isFavorite(cat.id) ? <StarFilledIcon /> : <StarIcon />}
                        label={isFavorite(cat.id) ? t('categories.action.removeFromFavourites') : t('categories.action.addToFavourites')}
                        onClick={() => {
                          const added = toggleFav(cat.id)
                          notify(added ? t('categories.notify.addedToFavourites', { name: cat.name }) : t('categories.notify.removedFromFavourites', { name: cat.name }), added ? 'success' : 'info')
                        }}
                        className={isFavorite(cat.id) ? 'text-amber-400 hover:text-amber-500' : 'text-gray-300 hover:text-amber-400 dark:text-gray-600 dark:hover:text-amber-400'}
                      />
                      <IconButton icon={<InfoIcon />} label={t('categories.action.details')} onClick={() => setSelectedCategory(cat)} className="text-gray-400 hover:text-blue-500 dark:hover:text-blue-400" />
                      <IconButton icon={<PencilIcon />} label={t('categories.action.edit')} onClick={() => openEdit(cat)} className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400" />
                      <IconButton icon={cat.isActive ? <BanIcon /> : <CheckCircleIcon />} label={cat.isActive ? t('categories.action.deactivate') : t('categories.action.activate')} onClick={() => toggle(cat)} className={cat.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'} />
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 && !loading && (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">{t('categories.table.empty')}</td>
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
      <CategoryPanel category={selectedCategory} onClose={() => setSelectedCategory(null)} />
    </div>
  )
}
