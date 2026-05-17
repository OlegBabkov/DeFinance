import { useEffect, useState } from 'react'
import { currenciesApi, type Currency } from '../api/currencies'
import { Modal } from '../components/Modal'

type ModalState = null | 'create' | Currency

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

export function CurrenciesPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const [formCode, setFormCode] = useState('')
  const [formName, setFormName] = useState('')
  const [formSymbol, setFormSymbol] = useState('')

  useEffect(() => {
    currenciesApi
      .getAll()
      .then(setCurrencies)
      .catch(() => setError('Failed to load currencies'))
      .finally(() => setLoading(false))
  }, [])

  const openCreate = () => {
    setFormCode('')
    setFormName('')
    setFormSymbol('')
    setFormError(null)
    setModal('create')
  }

  const openEdit = (c: Currency) => {
    setFormName(c.name)
    setFormSymbol(c.symbol)
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
      if (modal === 'create') {
        const created = await currenciesApi.create({ code: formCode, name: formName, symbol: formSymbol })
        setCurrencies(prev => [...prev, created])
      } else if (modal !== null) {
        const updated = await currenciesApi.update(modal.id, { name: formName, symbol: formSymbol })
        setCurrencies(prev => prev.map(c => (c.id === updated.id ? updated : c)))
      }
      closeModal()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (currency: Currency) => {
    const updated = currency.isActive
      ? await currenciesApi.deactivate(currency.id)
      : await currenciesApi.activate(currency.id)
    setCurrencies(prev => prev.map(c => (c.id === updated.id ? updated : c)))
  }

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Currencies</h1>
        <button
          onClick={openCreate}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + New Currency
        </button>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Currency' : 'New Currency'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!isEditing && (
              <div>
                <label className={labelCls}>Code</label>
                <input
                  required
                  maxLength={10}
                  value={formCode}
                  onChange={e => setFormCode(e.target.value.toUpperCase())}
                  className={inputCls}
                  placeholder="USD"
                />
              </div>
            )}
            <div>
              <label className={labelCls}>Name</label>
              <input
                required
                maxLength={100}
                value={formName}
                onChange={e => setFormName(e.target.value)}
                className={inputCls}
                placeholder="US Dollar"
              />
            </div>
            <div>
              <label className={labelCls}>Symbol</label>
              <input
                required
                maxLength={10}
                value={formSymbol}
                onChange={e => setFormSymbol(e.target.value)}
                className={inputCls}
                placeholder="$"
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
              {['Symbol', 'Code', 'Name', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
            {currencies.map(currency => (
              <tr key={currency.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-bold text-gray-700 dark:text-gray-300 w-12">{currency.symbol}</td>
                <td className="px-4 py-3 font-mono text-gray-900 dark:text-gray-100">{currency.code}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{currency.name}</td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      currency.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                        : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                    }`}
                  >
                    {currency.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right space-x-3">
                  <button
                    onClick={() => openEdit(currency)}
                    className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => toggle(currency)}
                    className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
                  >
                    {currency.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </td>
              </tr>
            ))}
            {currencies.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">
                  No currencies.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
