import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { calendarEventsApi, type CalendarEvent } from '../api/calendarEvents'
import { Spinner } from './Spinner'
import { AddCalendarEventModal } from './AddCalendarEventModal'

interface Props {
  day: Date | null
  intlLocale: string
  onClose: () => void
  onEventsChanged?: () => void
}

function fmtAmount(n: number) {
  return n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function CalendarDayPanel({ day, intlLocale, onClose, onEventsChanged }: Props) {
  const { t } = useTranslation()
  const open = day !== null
  const [events, setEvents] = useState<CalendarEvent[]>([])
  const [loading, setLoading] = useState(false)
  const [addEventOpen, setAddEventOpen] = useState(false)

  const dayStr = day
    ? `${day.getFullYear()}-${String(day.getMonth() + 1).padStart(2, '0')}-${String(day.getDate()).padStart(2, '0')}`
    : ''

  const reload = () => {
    if (!day) return
    setLoading(true)
    calendarEventsApi
      .getByDate(dayStr)
      .then(r => setEvents(r.items))
      .catch(() => setEvents([]))
      .finally(() => setLoading(false))
    onEventsChanged?.()
  }

  useEffect(() => {
    if (!day) { setEvents([]); return }
    reload()
  }, [day?.getTime()])

  const dateLabel = day
    ? (() => {
        const s = new Intl.DateTimeFormat(intlLocale, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).format(day)
        return s.charAt(0).toUpperCase() + s.slice(1)
      })()
    : ''

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        className={`fixed inset-0 z-30 bg-black/20 dark:bg-black/40 transition-opacity duration-300 ${open ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'}`}
      />

      {/* Panel */}
      <div
        className={`fixed top-12 right-0 bottom-0 w-96 z-40 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 shadow-xl flex flex-col transform transition-transform duration-300 ease-in-out ${open ? 'translate-x-0' : 'translate-x-full'}`}
      >
        {/* Header */}
        <div className="flex items-start justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-700 shrink-0">
          <div className="flex-1 min-w-0">
            <span className="text-sm font-semibold text-gray-800 dark:text-gray-100 leading-snug">{dateLabel}</span>
            <div className="mt-2">
              <button
                onClick={() => setAddEventOpen(true)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-medium rounded-lg transition-colors"
              >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" className="w-3.5 h-3.5">
                  <path d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z" />
                </svg>
                {t('calendar.dayPanel.addEvent')}
              </button>
            </div>
          </div>
          <button
            onClick={onClose}
            className="ml-3 shrink-0 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-3">
          {loading && <Spinner size="sm" />}

          {!loading && events.length === 0 && (
            <p className="text-sm text-gray-400 dark:text-gray-500 text-center py-10">
              {t('calendar.dayPanel.noEvents')}
            </p>
          )}

          {!loading && events.map(ev => (
            <div
              key={ev.id}
              className="rounded-lg border border-gray-100 dark:border-gray-700 bg-gray-50 dark:bg-gray-700/40 p-3"
            >
              {ev.eventType === 'Event' ? (
                /* ── Event card ── */
                <div className="flex items-start gap-2">
                  <span
                    className="mt-0.5 shrink-0"
                    style={{ color: ev.color ?? '#6366F1' }}
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" className="w-4 h-4">
                      <path fillRule="evenodd" d="M4 1.75a.75.75 0 0 1 1.5 0V3h5V1.75a.75.75 0 0 1 1.5 0V3A2 2 0 0 1 14 5v7a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2V1.75ZM3.5 7a.5.5 0 0 0 0 1h9a.5.5 0 0 0 0-1h-9Z" clipRule="evenodd" />
                    </svg>
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-gray-800 dark:text-gray-200 truncate">
                      {ev.title || t('calendar.form.typeEvent')}
                    </p>
                    {(ev.timeFrom || ev.timeTo) && (
                      <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
                        {ev.timeFrom ?? '?'}{ev.timeTo ? ` → ${ev.timeTo}` : ''}
                      </p>
                    )}
                    {ev.notes && (
                      <p className="text-xs text-gray-400 dark:text-gray-500 italic mt-0.5 truncate">{ev.notes}</p>
                    )}
                  </div>
                </div>
              ) : (
                /* ── Payment card ── */
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-start gap-2 min-w-0">
                    {(ev.color ?? ev.categoryColor) ? (
                      <span className="w-2 h-2 rounded-full shrink-0 mt-1.5" style={{ backgroundColor: ev.color ?? ev.categoryColor ?? undefined }} />
                    ) : (
                      <span className="w-2 h-2 shrink-0" />
                    )}
                    <div className="min-w-0">
                      <p className="text-sm text-gray-800 dark:text-gray-200 truncate">
                        {ev.categoryIcon && <span className="mr-1">{ev.categoryIcon}</span>}
                        {ev.categoryName ?? '—'}
                      </p>
                      <p className="text-xs text-gray-400 dark:text-gray-500 truncate">{ev.accountName ?? ''}</p>
                      {ev.counterpartyName && (
                        <p className="text-xs text-gray-400 dark:text-gray-500 truncate">{ev.counterpartyName}</p>
                      )}
                      {ev.paymentStatusName && (
                        <p
                          className="text-xs mt-0.5 font-medium"
                          style={{ color: ev.paymentStatusColor ?? undefined }}
                        >
                          {ev.paymentStatusName}
                        </p>
                      )}
                      {ev.notes && (
                        <p className="text-xs text-gray-400 dark:text-gray-500 italic truncate">{ev.notes}</p>
                      )}
                    </div>
                  </div>
                  {ev.sum != null && (
                    <div className="text-right shrink-0">
                      <p className="text-sm font-mono font-medium text-gray-700 dark:text-gray-300">
                        {ev.accountCurrencySymbol} {fmtAmount(ev.sum)}
                      </p>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {addEventOpen && day && (
        <AddCalendarEventModal
          initialDate={dayStr}
          onClose={() => setAddEventOpen(false)}
          onCreated={reload}
        />
      )}
    </>
  )
}
