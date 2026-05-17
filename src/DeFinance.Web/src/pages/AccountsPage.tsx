import { useEffect, useState } from 'react'
import { accountsApi, type Account, type AccountType } from '../api/accounts'
import { currenciesApi, type Currency } from '../api/currencies'
import { Modal } from '../components/Modal'
import { IconButton, PencilIcon, CheckCircleIcon, BanIcon } from '../components/IconButton'

type ModalState = null | 'create' | Account

const ACCOUNT_TYPES: AccountType[] = ['Checking', 'Savings', 'Credit', 'Cash', 'Investment']

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'

const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

export function AccountsPage() {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ModalState>(null)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const [formName, setFormName] = useState('')
  const [formType, setFormType] = useState<AccountType>('Checking')
  const [formBalance, setFormBalance] = useState('0')
  const [formCurrencyId, setFormCurrencyId] = useState('')

  useEffect(() => {
    Promise.all([accountsApi.getAll(), currenciesApi.getAll()])
      .then(([accs, curs]) => {
        setAccounts(accs)
        setCurrencies(curs)
        if (curs.length > 0) setFormCurrencyId(curs[0].id)
      })
      .catch(() => setError('Failed to load data'))
      .finally(() => setLoading(false))
  }, [])

  const openCreate = () => {
    setFormName('')
    setFormType('Checking')
    setFormBalance('0')
    setFormCurrencyId(currencies[0]?.id ?? '')
    setFormError(null)
    setModal('create')
  }

  const openEdit = (a: Account) => {
    setFormName(a.name)
    setFormError(null)
    setModal(a)
  }

  const closeModal = () => setModal(null)

  const isEditing = modal !== null && modal !== 'create'

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      if (modal === 'create') {
        const created = await accountsApi.create({
          name: formName,
          type: formType,
          initialBalance: parseFloat(formBalance),
          currencyId: formCurrencyId,
        })
        setAccounts(prev => [...prev, created])
      } else if (modal !== null) {
        const updated = await accountsApi.update(modal.id, { name: formName })
        setAccounts(prev => prev.map(a => (a.id === updated.id ? updated : a)))
      }
      closeModal()
    } catch {
      setFormError('Failed to save. Please check your input and try again.')
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (account: Account) => {
    const updated = account.isActive
      ? await accountsApi.deactivate(account.id)
      : await accountsApi.activate(account.id)
    setAccounts(prev => prev.map(a => (a.id === updated.id ? updated : a)))
  }

  const currencySymbol = (id: string) => currencies.find(c => c.id === id)?.symbol ?? ''

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Accounts</h1>
        <button
          onClick={openCreate}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + New Account
        </button>
      </div>

      {modal !== null && (
        <Modal title={isEditing ? 'Edit Account' : 'New Account'} onClose={closeModal}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className={labelCls}>Name</label>
              <input
                required
                maxLength={100}
                value={formName}
                onChange={e => setFormName(e.target.value)}
                className={inputCls}
                placeholder="My Account"
              />
            </div>
            {!isEditing && (
              <>
                <div>
                  <label className={labelCls}>Type</label>
                  <select
                    value={formType}
                    onChange={e => setFormType(e.target.value as AccountType)}
                    className={inputCls}
                  >
                    {ACCOUNT_TYPES.map(t => (
                      <option key={t} value={t}>{t}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={labelCls}>Currency</label>
                  <select
                    value={formCurrencyId}
                    onChange={e => setFormCurrencyId(e.target.value)}
                    className={inputCls}
                  >
                    {currencies.map(c => (
                      <option key={c.id} value={c.id}>{c.symbol} {c.code} — {c.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={labelCls}>Initial Balance</label>
                  <input
                    required
                    type="number"
                    min="0"
                    step="0.01"
                    value={formBalance}
                    onChange={e => setFormBalance(e.target.value)}
                    className={inputCls}
                  />
                </div>
              </>
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

      <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              {['Name', 'Type', 'Balance', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
            {accounts.map(account => (
              <tr key={account.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{account.name}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{account.type}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-gray-100 font-mono">
                  {currencySymbol(account.currencyId)} {account.balance.toFixed(2)}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      account.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                        : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                    }`}
                  >
                    {account.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="inline-flex items-center gap-1">
                    <IconButton
                      icon={<PencilIcon />}
                      label="Edit"
                      onClick={() => openEdit(account)}
                      className="text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400"
                    />
                    <IconButton
                      icon={account.isActive ? <BanIcon /> : <CheckCircleIcon />}
                      label={account.isActive ? 'Deactivate' : 'Activate'}
                      onClick={() => toggle(account)}
                      className={account.isActive ? 'text-gray-400 hover:text-red-500 dark:hover:text-red-400' : 'text-gray-400 hover:text-green-600 dark:hover:text-green-400'}
                    />
                  </div>
                </td>
              </tr>
            ))}
            {accounts.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">
                  No accounts yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
