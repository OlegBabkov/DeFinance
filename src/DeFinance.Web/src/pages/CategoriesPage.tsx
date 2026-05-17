import { useEffect, useState } from 'react'
import { categoriesApi, type Category, type CategoryType } from '../api/categories'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'

type Tab = 'Income' | 'Expense'
type ModalState = null | 'create' | Category

const TABS: { id: Tab; label: string }[] = [
  { id: 'Income', label: 'Income' },
  { id: 'Expense', label: 'Losses' },
]

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [tab, setTab] = useState<Tab>('Income')
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const [formName, setFormName] = useState('')
  const [formColor, setFormColor] = useState('')
  const [formIcon, setFormIcon] = useState('')
  const [formParentId, setFormParentId] = useState('')

  useEffect(() => {
    categoriesApi
      .getAll()
      .then(setCategories)
      .catch(() => setError('Failed to load categories'))
      .finally(() => setLoading(false))
  }, [])

  const openCreate = () => {
    setFormName('')
    setFormColor('#6366f1')
    setFormIcon('')
    setFormParentId('')
    setFormError(null)
    setModal('create')
  }

  const openEdit = (c: Category) => {
    setFormName(c.name)
    setFormColor(c.color ?? '#6366f1')
    setFormIcon(c.icon ?? '')
    setFormError(null)
    setModal(c)
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
      if (modal === 'create') {
        const created = await categoriesApi.create({
          name: formName,
          type: tab as CategoryType,
          color,
          icon,
          parentId: formParentId || null,
        })
        setCategories(prev => [...prev, created])
      } else if (modal !== null) {
        const updated = await categoriesApi.update(modal.id, { name: formName, color, icon })
        setCategories(prev => prev.map(c => (c.id === updated.id ? updated : c)))
      }
      closeModal()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (category: Category) => {
    const updated = category.isActive
      ? await categoriesApi.deactivate(category.id)
      : await categoriesApi.activate(category.id)
    setCategories(prev => prev.map(c => (c.id === updated.id ? updated : c)))
  }

  const parentName = (parentId: string | null) =>
    parentId ? (categories.find(c => c.id === parentId)?.name ?? '—') : '—'

  const sameTypeCategories = categories.filter(c => c.type === tab && c.id !== (isEditing ? (modal as Category).id : ''))

  const visible = categories.filter(c => c.type === tab)

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="h-full flex flex-col">
      <div className="px-8 pt-8 pb-0 shrink-0">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Categories</h1>
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            + New Category
          </button>
        </div>

        <div className="flex border-b border-gray-200 dark:border-gray-700">
          {TABS.map(({ id, label }) => (
            <button
              key={id}
              onClick={() => setTab(id)}
              className={`px-5 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors ${
                tab === id
                  ? 'border-indigo-600 text-indigo-600 dark:text-indigo-400 dark:border-indigo-400'
                  : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
            >
              {label}
              <span className="ml-2 rounded-full bg-gray-100 dark:bg-gray-700 px-2 py-0.5 text-xs text-gray-600 dark:text-gray-300">
                {categories.filter(c => c.type === id).length}
              </span>
            </button>
          ))}
        </div>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Category' : `New ${tab} Category`} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>Name</label>
              <input
                required
                maxLength={100}
                value={formName}
                onChange={e => setFormName(e.target.value)}
                className={inputCls}
                placeholder="Category name"
              />
            </div>
            <div className="flex gap-4">
              <div className="flex-1">
                <label className={labelCls}>Icon (emoji)</label>
                <input
                  maxLength={50}
                  value={formIcon}
                  onChange={e => setFormIcon(e.target.value)}
                  className={inputCls}
                  placeholder="🍔"
                />
              </div>
              <div>
                <label className={labelCls}>Color</label>
                <div className="flex items-center gap-2 mt-1">
                  <input
                    type="color"
                    value={formColor}
                    onChange={e => setFormColor(e.target.value)}
                    className="h-9 w-14 rounded border border-gray-300 dark:border-gray-600 cursor-pointer bg-white dark:bg-gray-700 p-0.5"
                  />
                  <span className="text-xs font-mono text-gray-500 dark:text-gray-400">{formColor}</span>
                </div>
              </div>
            </div>
            {!isEditing && sameTypeCategories.length > 0 && (
              <div>
                <label className={labelCls}>Parent Category (optional)</label>
                <select
                  value={formParentId}
                  onChange={e => setFormParentId(e.target.value)}
                  className={inputCls}
                >
                  <option value="">— None —</option>
                  {sameTypeCategories.map(c => (
                    <option key={c.id} value={c.id}>
                      {c.icon ? `${c.icon} ` : ''}{c.name}
                    </option>
                  ))}
                </select>
              </div>
            )}
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

      <div className="flex-1 min-h-0 overflow-auto mx-8 mb-8 mt-6 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
            <tr>
              {['Name', 'Color', 'Parent', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
            {visible.map(cat => (
              <tr key={cat.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">
                  {cat.icon && <span className="mr-1">{cat.icon}</span>}
                  {cat.name}
                </td>
                <td className="px-4 py-3">
                  {cat.color ? (
                    <div className="flex items-center gap-2">
                      <span
                        className="inline-block w-4 h-4 rounded-full border border-gray-200 dark:border-gray-600"
                        style={{ backgroundColor: cat.color }}
                      />
                      <span className="text-gray-500 dark:text-gray-400 font-mono text-xs">{cat.color}</span>
                    </div>
                  ) : (
                    <span className="text-gray-300 dark:text-gray-600">—</span>
                  )}
                </td>
                <td className="px-4 py-3 text-gray-500 dark:text-gray-400">{parentName(cat.parentId)}</td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      cat.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                        : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                    }`}
                  >
                    {cat.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="inline-flex items-center gap-1">
                    <IconButton
                      icon={<PencilIcon />}
                      label="Edit"
                      onClick={() => openEdit(cat)}
                      className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400"
                    />
                    <IconButton
                      icon={cat.isActive ? <BanIcon /> : <CheckCircleIcon />}
                      label={cat.isActive ? 'Deactivate' : 'Activate'}
                      onClick={() => toggle(cat)}
                      className={cat.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'}
                    />
                  </div>
                </td>
              </tr>
            ))}
            {visible.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">
                  No categories yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
