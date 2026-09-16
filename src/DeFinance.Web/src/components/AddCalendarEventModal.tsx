import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNotify } from '../NotificationContext'
import { useMainCurrency } from '../MainCurrencyContext'
import { calendarEventsApi, type CalendarEventType } from '../api/calendarEvents'
import { accountsApi, type Account } from '../api/accounts'
import { categoriesApi, type Category } from '../api/categories'
import { counterpartiesApi, type Counterparty } from '../api/counterparties'
import { paymentStatusesApi, type PaymentStatus } from '../api/paymentStatuses'
import { Modal } from './Modal'
import { CalcIcon } from './IconButton'
import { CalculatorModal } from './CalculatorModal'
import { useFavorites, sortByFavorites } from '../hooks/useFavorites'

interface Props {
  initialDate: string   // YYYY-MM-DD
  onClose: () => void
  onCreated: () => void
}

const inputCls =
  'w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500'
const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'

function activeOrCurrent<T extends { id: string; isActive: boolean }>(items: T[], currentId: string): T[] {
  const active = items.filter(i => i.isActive)
  if (currentId && !active.some(i => i.id === currentId)) {
    const cur = items.find(i => i.id === currentId)
    if (cur) return [...active, cur]
  }
  return active
}

interface FormState {
  eventType: CalendarEventType
  date: string
  // Event
  title: string
  timeFrom: string
  timeTo: string
  // Payment
  accountId: string
  categoryId: string
  counterpartyId: string
  paymentStatusId: string
  inCurrencyId: string
  sum: string
  exchangeRate: string
  // Common
  notes: string
}

export function AddCalendarEventModal({ initialDate, onClose, onCreated }: Props) {
  const { t } = useTranslation()
  const notify = useNotify()
  const { mainCurrency } = useMainCurrency()
  const { favorites: favCats } = useFavorites('categories')
  const { favorites: favCps  } = useFavorites('counterparties')

  const [accounts, setAccounts]           = useState<Account[]>([])
  const [categories, setCategories]       = useState<Category[]>([])
  const [counterparties, setCounterparties] = useState<Counterparty[]>([])
  const [paymentStatuses, setPaymentStatuses] = useState<PaymentStatus[]>([])
  const [saving, setSaving]       = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [showCalculator, setShowCalculator] = useState(false)

  const [form, setForm] = useState<FormState>({
    eventType:       'Event',
    date:            initialDate,
    title:           '',
    timeFrom:        '',
    timeTo:          '',
    accountId:       '',
    categoryId:      '',
    counterpartyId:  '',
    paymentStatusId: '',
    inCurrencyId:    '',
    sum:             '',
    exchangeRate:    '1',
    notes:           '',
  })

  useEffect(() => {
    Promise.all([
      accountsApi.getAll({ pageSize: 500 }),
      categoriesApi.getAll({ pageSize: 500 }),
      counterpartiesApi.getAll({ pageSize: 500 }),
      paymentStatusesApi.getAll({ pageSize: 500 }),
    ]).then(([accts, cats, cps, statuses]) => {
      setAccounts(accts.items)
      setCategories(cats.items)
      setCounterparties(cps.items)
      setPaymentStatuses(statuses.items)
      const firstAccount = accts.items.find(a => a.isActive)
      const firstStatus  = statuses.items.find(s => s.isActive)
      setForm(f => ({
        ...f,
        accountId:       firstAccount?.id ?? '',
        inCurrencyId:    firstAccount?.currencyId ?? '',
        paymentStatusId: firstStatus?.id ?? '',
      }))
    }).catch(() => {})
  }, [])

  const setField = (key: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm(f => ({ ...f, [key]: e.target.value }))

  const onAccountChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const acct = accounts.find(a => a.id === e.target.value)
    const sameAsMain = acct?.currencyId === mainCurrency?.id
    setForm(f => ({
      ...f,
      accountId:    e.target.value,
      inCurrencyId: acct?.currencyId ?? f.inCurrencyId,
      exchangeRate: sameAsMain ? '1' : f.exchangeRate,
    }))
  }

  const sameAsMain = !!form.accountId &&
    accounts.find(a => a.id === form.accountId)?.currencyId === mainCurrency?.id

  const inMainVal = (() => {
    const r = parseFloat(form.exchangeRate)
    const v = parseFloat(form.sum) / r
    return isNaN(v) || !isFinite(v)
      ? '—'
      : v.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
  })()

  const selectedAccountCurrency = accounts.find(a => a.id === form.accountId)?.currency

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      if (form.eventType === 'Event') {
        await calendarEventsApi.create({
          date:      form.date,
          eventType: 'Event',
          title:     form.title || null,
          timeFrom:  form.timeFrom || null,
          timeTo:    form.timeTo || null,
          notes:     form.notes || null,
        })
      } else {
        await calendarEventsApi.create({
          date:            form.date,
          eventType:       'Payment',
          accountId:       form.accountId,
          categoryId:      form.categoryId,
          counterpartyId:  form.counterpartyId || null,
          paymentStatusId: form.paymentStatusId,
          inCurrencyId:    form.inCurrencyId,
          sum:             parseFloat(form.sum),
          exchangeRate:    parseFloat(form.exchangeRate),
          notes:           form.notes || null,
        })
      }
      notify(t('calendar.dayPanel.eventCreated'), 'success')
      onCreated()
      onClose()
    } catch {
      setFormError(t('transactions.error.saveFailed'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <>
      <Modal title={t('calendar.dayPanel.addEvent')} onClose={onClose} wide>
        <form onSubmit={handleSubmit} className="space-y-4">

          {/* Date + Type row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelCls}>{t('transactions.form.date')}</label>
              <input
                required type="date" value={form.date}
                onChange={setField('date')} className={inputCls}
              />
            </div>
            <div>
              <label className={labelCls}>{t('calendar.form.type')}</label>
              <select
                value={form.eventType}
                onChange={e => setForm(f => ({ ...f, eventType: e.target.value as CalendarEventType }))}
                className={inputCls}
              >
                <option value="Event">{t('calendar.form.typeEvent')}</option>
                <option value="Payment">{t('calendar.form.typePayment')}</option>
              </select>
            </div>
          </div>

          {/* ── Event fields ─────────────────────────────────────────────── */}
          {form.eventType === 'Event' && (
            <>
              <div>
                <label className={labelCls}>{t('calendar.form.title')}</label>
                <input
                  type="text" value={form.title}
                  onChange={setField('title')} className={inputCls}
                  placeholder={t('calendar.form.titlePlaceholder')}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={labelCls}>
                    {t('calendar.form.timeFrom')}{' '}
                    <span className="text-gray-400 font-normal">{t('transactions.form.counterpartyOptional')}</span>
                  </label>
                  <input
                    type="time" value={form.timeFrom}
                    onChange={setField('timeFrom')} className={inputCls}
                  />
                </div>
                <div>
                  <label className={labelCls}>
                    {t('calendar.form.timeTo')}{' '}
                    <span className="text-gray-400 font-normal">{t('transactions.form.counterpartyOptional')}</span>
                  </label>
                  <input
                    type="time" value={form.timeTo}
                    onChange={setField('timeTo')} className={inputCls}
                  />
                </div>
              </div>
            </>
          )}

          {/* ── Payment fields ────────────────────────────────────────────── */}
          {form.eventType === 'Payment' && (
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelCls}>{t('transactions.form.account')}</label>
                <select required value={form.accountId} onChange={onAccountChange} className={inputCls}>
                  <option value="">{t('transactions.form.selectAccount')}</option>
                  {activeOrCurrent(accounts, form.accountId).map(a => (
                    <option key={a.id} value={a.id}>{a.name}{a.currency ? ` (${a.currency.code})` : ''}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.category')}</label>
                <select required value={form.categoryId} onChange={setField('categoryId')} className={inputCls}>
                  <option value="">{t('transactions.form.selectCategory')}</option>
                  {sortByFavorites(activeOrCurrent(categories, form.categoryId), favCats).map(c => (
                    <option key={c.id} value={c.id}>{favCats.has(c.id) ? `★ ${c.name}` : c.name}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.sum')}</label>
                <div className="relative">
                  <input
                    required type="number" min="0.01" step="0.01"
                    value={form.sum} onChange={setField('sum')}
                    className={`${inputCls} pr-8 [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none`}
                    placeholder="0.00"
                  />
                  <button
                    type="button" onClick={() => setShowCalculator(true)} tabIndex={-1}
                    title={t('transactions.form.openCalculator')}
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                  >
                    <CalcIcon />
                  </button>
                </div>
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.exchangeRate')}</label>
                <input
                  required type="number" min="0.000001" step="0.000001"
                  value={form.exchangeRate} onChange={setField('exchangeRate')}
                  disabled={sameAsMain}
                  className={`${inputCls} [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none${sameAsMain ? ' opacity-50 cursor-not-allowed' : ''}`}
                />
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.currency')}</label>
                <input
                  disabled
                  value={selectedAccountCurrency ? `${selectedAccountCurrency.symbol} ${selectedAccountCurrency.code}` : '—'}
                  className={`${inputCls} opacity-50 cursor-not-allowed`}
                />
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.inMainCurrency')}</label>
                <input disabled value={inMainVal} className={`${inputCls} opacity-50 cursor-not-allowed`} />
              </div>

              <div>
                <label className={labelCls}>{t('transactions.form.paymentStatus')}</label>
                <select required value={form.paymentStatusId} onChange={setField('paymentStatusId')} className={inputCls}>
                  <option value="">{t('transactions.form.selectStatus')}</option>
                  {activeOrCurrent(paymentStatuses, form.paymentStatusId).map(s => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className={labelCls}>
                  {t('transactions.form.counterparty')}{' '}
                  <span className="text-gray-400 font-normal">{t('transactions.form.counterpartyOptional')}</span>
                </label>
                <select value={form.counterpartyId} onChange={setField('counterpartyId')} className={inputCls}>
                  <option value="">{t('transactions.form.noCounterparty')}</option>
                  {sortByFavorites(activeOrCurrent(counterparties, form.counterpartyId), favCps).map(c => (
                    <option key={c.id} value={c.id}>{favCps.has(c.id) ? `★ ${c.name}` : c.name}</option>
                  ))}
                </select>
              </div>
            </div>
          )}

          {/* ── Notes (common) ────────────────────────────────────────────── */}
          <div>
            <label className={labelCls}>
              {t('transactions.form.notes')}{' '}
              <span className="text-gray-400 font-normal">{t('transactions.form.notesOptional')}</span>
            </label>
            <textarea
              value={form.notes} onChange={setField('notes')} maxLength={500} rows={2}
              className={`${inputCls} resize-none`} placeholder="…"
            />
          </div>

          {formError && <p className="text-sm text-red-500">{formError}</p>}

          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button" onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
            >
              {t('transactions.button.cancel')}
            </button>
            <button
              type="submit" disabled={saving}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
            >
              {saving ? t('transactions.button.saving') : t('transactions.button.create')}
            </button>
          </div>
        </form>
      </Modal>

      {showCalculator && (
        <CalculatorModal
          onApply={value => setForm(f => ({ ...f, sum: value }))}
          onClose={() => setShowCalculator(false)}
        />
      )}
    </>
  )
}
