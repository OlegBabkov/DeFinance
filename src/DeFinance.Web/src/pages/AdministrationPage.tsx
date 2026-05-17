import { useEffect, useState, type ReactNode } from 'react'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'

type ModalState = null | 'create' | PaymentStatus

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

function PaymentStatusesPanel() {
  const [statuses, setStatuses] = useState<PaymentStatus[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [formName, setFormName] = useState('')
  const [formDescription, setFormDescription] = useState('')

  useEffect(() => {
    paymentStatusesApi
      .getAll()
      .then(setStatuses)
      .catch(() => setError('Failed to load payment statuses'))
      .finally(() => setLoading(false))
  }, [])

  const openCreate = () => {
    setFormName('')
    setFormDescription('')
    setFormError(null)
    setModal('create')
  }

  const openEdit = (s: PaymentStatus) => {
    setFormName(s.name)
    setFormDescription(s.description ?? '')
    setFormError(null)
    setModal(s)
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
        const created = await paymentStatusesApi.create({ name: formName, description })
        setStatuses(prev => [...prev, created])
      } else if (modal !== null) {
        const updated = await paymentStatusesApi.update(modal.id, { name: formName, description })
        setStatuses(prev => prev.map(s => (s.id === updated.id ? updated : s)))
      }
      closeModal()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (status: PaymentStatus) => {
    const updated = status.isActive
      ? await paymentStatusesApi.deactivate(status.id)
      : await paymentStatusesApi.activate(status.id)
    setStatuses(prev => prev.map(s => (s.id === updated.id ? updated : s)))
  }

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

      <div className="flex items-center justify-between mb-3">
        <span />
        <button
          onClick={openCreate}
          className="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-medium rounded-lg transition-colors"
        >
          + New
        </button>
      </div>

      {loading && <p className="text-sm text-gray-400">Loading…</p>}
      {error && <p className="text-sm text-red-500">{error}</p>}

      {!loading && !error && (
        <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700">
          <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                {['Name', 'Description', 'Status', ''].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-gray-500 dark:text-gray-400 text-xs">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700 bg-white dark:bg-gray-800">
              {statuses.map(s => (
                <tr key={s.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-3 py-2 font-medium text-gray-900 dark:text-gray-100">{s.name}</td>
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
              {statuses.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-6 text-center text-gray-400 dark:text-gray-500">
                    No payment statuses yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </>
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

        <Panel title="" empty />
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
      <div className="flex-1 p-4 overflow-auto">
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
