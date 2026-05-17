import { useEffect, useState } from 'react'
import { counterpartiesApi, type Counterparty, type CounterpartyType } from '../api/counterparties'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'

type ModalState = null | 'create' | Counterparty

const COUNTERPARTY_TYPES: CounterpartyType[] = ['Person', 'Company', 'Other']

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

export function CounterpartiesPage() {
  const [counterparties, setCounterparties] = useState<Counterparty[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const [formName, setFormName] = useState('')
  const [formType, setFormType] = useState<CounterpartyType>('Person')
  const [formContactInfo, setFormContactInfo] = useState('')

  useEffect(() => {
    counterpartiesApi
      .getAll()
      .then(setCounterparties)
      .catch(() => setError('Failed to load counterparties'))
      .finally(() => setLoading(false))
  }, [])

  const openCreate = () => {
    setFormName('')
    setFormType('Person')
    setFormContactInfo('')
    setFormError(null)
    setModal('create')
  }

  const openEdit = (c: Counterparty) => {
    setFormName(c.name)
    setFormType(c.type)
    setFormContactInfo(c.contactInfo ?? '')
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
      const contactInfo = formContactInfo.trim() || null
      if (modal === 'create') {
        const created = await counterpartiesApi.create({ name: formName, type: formType, contactInfo })
        setCounterparties(prev => [...prev, created])
      } else if (modal !== null) {
        const updated = await counterpartiesApi.update(modal.id, { name: formName, type: formType, contactInfo })
        setCounterparties(prev => prev.map(c => (c.id === updated.id ? updated : c)))
      }
      closeModal()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (counterparty: Counterparty) => {
    const updated = counterparty.isActive
      ? await counterpartiesApi.deactivate(counterparty.id)
      : await counterpartiesApi.activate(counterparty.id)
    setCounterparties(prev => prev.map(c => (c.id === updated.id ? updated : c)))
  }

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Counterparties</h1>
        <button
          onClick={openCreate}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + New Counterparty
        </button>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Counterparty' : 'New Counterparty'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>Name</label>
              <input
                required
                maxLength={100}
                value={formName}
                onChange={e => setFormName(e.target.value)}
                className={inputCls}
                placeholder="Counterparty name"
              />
            </div>
            <div>
              <label className={labelCls}>Type</label>
              <select
                value={formType}
                onChange={e => setFormType(e.target.value as CounterpartyType)}
                className={inputCls}
              >
                {COUNTERPARTY_TYPES.map(t => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <div>
              <label className={labelCls}>Contact Info (optional)</label>
              <textarea
                maxLength={500}
                value={formContactInfo}
                onChange={e => setFormContactInfo(e.target.value)}
                className={`${inputCls} resize-none`}
                rows={3}
                placeholder="Email, phone, address…"
              />
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

      <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              {['Name', 'Type', 'Contact Info', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
            {counterparties.map(cp => (
              <tr key={cp.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{cp.name}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{cp.type}</td>
                <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-xs truncate">
                  {cp.contactInfo ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      cp.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                        : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                    }`}
                  >
                    {cp.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="inline-flex items-center gap-1">
                    <IconButton
                      icon={<PencilIcon />}
                      label="Edit"
                      onClick={() => openEdit(cp)}
                      className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400"
                    />
                    <IconButton
                      icon={cp.isActive ? <BanIcon /> : <CheckCircleIcon />}
                      label={cp.isActive ? 'Deactivate' : 'Activate'}
                      onClick={() => toggle(cp)}
                      className={cp.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'}
                    />
                  </div>
                </td>
              </tr>
            ))}
            {counterparties.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">
                  No counterparties yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
