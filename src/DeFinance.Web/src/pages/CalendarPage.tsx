import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { CalendarDayPanel } from '../components/CalendarDayPanel'

type ViewMode = 'week' | 'month' | 'year'

const LOCALE_MAP: Record<string, string> = {
  uk: 'uk-UA', sv: 'sv-SE', no: 'nb-NO', el: 'el-GR',
  bg: 'bg-BG', fi: 'fi-FI', pl: 'pl-PL', it: 'it-IT',
  es: 'es-ES', de: 'de-DE', fr: 'fr-FR', en: 'en-GB',
}

function getIntlLocale(lang: string): string {
  return LOCALE_MAP[lang] ?? lang
}

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
}

function getWeekStart(date: Date): Date {
  const d = new Date(date)
  const day = d.getDay()
  d.setDate(d.getDate() + (day === 0 ? -6 : 1 - day))
  d.setHours(0, 0, 0, 0)
  return d
}

const REF_MONDAY = new Date(2024, 0, 1)

function getWeekDayLabels(intlLocale: string, fmt: 'short' | 'narrow'): string[] {
  const formatter = new Intl.DateTimeFormat(intlLocale, { weekday: fmt })
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(REF_MONDAY)
    d.setDate(REF_MONDAY.getDate() + i)
    return formatter.format(d)
  })
}

// ── Week View ──────────────────────────────────────────────────────────────────

function WeekView({ current, today, intlLocale, onDayClick }: {
  current: Date; today: Date; intlLocale: string
  onDayClick: (date: Date) => void
}) {
  const weekStart = getWeekStart(current)
  const days = Array.from({ length: 7 }, (_, i) => {
    const d = new Date(weekStart)
    d.setDate(weekStart.getDate() + i)
    return d
  })
  const weekDayLabels = getWeekDayLabels(intlLocale, 'short')
  const dateFmt = new Intl.DateTimeFormat(intlLocale, { day: 'numeric' })
  const hours = Array.from({ length: 24 }, (_, i) => i)

  return (
    <div className="h-full overflow-auto">
      {/* Day headers — clickable */}
      <div
        className="sticky top-0 z-10 grid bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700"
        style={{ gridTemplateColumns: '52px repeat(7, 1fr)' }}
      >
        <div className="border-r border-gray-200 dark:border-gray-700" />
        {days.map((day, i) => {
          const isToday = isSameDay(day, today)
          return (
            <button
              key={i}
              onClick={() => onDayClick(day)}
              className="py-3 text-center border-r border-gray-200 dark:border-gray-700 last:border-r-0 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
            >
              <div className={`text-xs font-medium uppercase tracking-wide ${isToday ? 'text-indigo-600 dark:text-indigo-400' : 'text-gray-500 dark:text-gray-400'}`}>
                {weekDayLabels[i]}
              </div>
              <div className="mt-1 flex justify-center">
                <span className={`text-lg font-semibold leading-none flex items-center justify-center w-9 h-9 rounded-full ${
                  isToday ? 'bg-indigo-600 text-white' : 'text-gray-900 dark:text-white'
                }`}>
                  {dateFmt.format(day)}
                </span>
              </div>
            </button>
          )
        })}
      </div>

      {/* Time grid — clicking a cell opens that day */}
      {hours.map(hour => (
        <div
          key={hour}
          className="grid border-b border-gray-100 dark:border-gray-800"
          style={{ gridTemplateColumns: '52px repeat(7, 1fr)', minHeight: '52px' }}
        >
          <div className="pr-2 pt-1 text-right text-xs text-gray-400 dark:text-gray-500 border-r border-gray-200 dark:border-gray-700 select-none">
            {hour === 0 ? '' : `${hour.toString().padStart(2, '0')}:00`}
          </div>
          {days.map((day, i) => (
            <div
              key={i}
              onClick={() => onDayClick(day)}
              className={`border-r border-gray-100 dark:border-gray-800 last:border-r-0 cursor-pointer hover:bg-gray-100/60 dark:hover:bg-gray-700/30 transition-colors ${
                isSameDay(day, today) ? 'bg-indigo-50/50 dark:bg-indigo-950/10' : ''
              }`}
            />
          ))}
        </div>
      ))}
    </div>
  )
}

// ── Month View ─────────────────────────────────────────────────────────────────

function MonthView({ current, today, intlLocale, onDayClick }: {
  current: Date; today: Date; intlLocale: string
  onDayClick: (date: Date) => void
}) {
  const year = current.getFullYear()
  const month = current.getMonth()
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const firstDow = (() => {
    const d = new Date(year, month, 1).getDay()
    return d === 0 ? 6 : d - 1
  })()
  const daysInPrev = new Date(year, month, 0).getDate()

  const cells: { date: Date; current: boolean }[] = []
  for (let i = firstDow - 1; i >= 0; i--) {
    cells.push({ date: new Date(year, month - 1, daysInPrev - i), current: false })
  }
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ date: new Date(year, month, d), current: true })
  }
  let nextDay = 1
  while (cells.length < 42) {
    cells.push({ date: new Date(year, month + 1, nextDay++), current: false })
  }

  const weekDayLabels = getWeekDayLabels(intlLocale, 'short')

  return (
    <div className="h-full flex flex-col p-4">
      <div className="grid grid-cols-7 mb-1 shrink-0">
        {weekDayLabels.map((d, i) => (
          <div key={i} className="py-2 text-center text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">
            {d}
          </div>
        ))}
      </div>
      <div className="flex-1 min-h-0 grid grid-cols-7 auto-rows-fr gap-px bg-gray-200 dark:bg-gray-700 rounded-xl overflow-hidden border border-gray-200 dark:border-gray-700">
        {cells.map(({ date, current: isCurrent }, i) => {
          const isToday = isSameDay(date, today)
          return (
            <button
              key={i}
              onClick={() => onDayClick(date)}
              className={`p-2 flex flex-col items-start bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700/60 transition-colors text-left ${!isCurrent ? 'opacity-40' : ''}`}
            >
              <span className={`text-sm w-7 h-7 flex items-center justify-center rounded-full font-medium ${
                isToday
                  ? 'bg-indigo-600 text-white'
                  : 'text-gray-900 dark:text-white'
              }`}>
                {date.getDate()}
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

// ── Year View ──────────────────────────────────────────────────────────────────

function MiniMonth({ year, month, today, intlLocale, onClick, onDayClick }: {
  year: number; month: number; today: Date; intlLocale: string
  onClick: () => void
  onDayClick: (date: Date) => void
}) {
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const firstDow = (() => {
    const d = new Date(year, month, 1).getDay()
    return d === 0 ? 6 : d - 1
  })()
  const cells: { day: number | null; isToday: boolean; date: Date | null }[] = []
  for (let i = 0; i < firstDow; i++) cells.push({ day: null, isToday: false, date: null })
  for (let d = 1; d <= daysInMonth; d++) {
    const date = new Date(year, month, d)
    cells.push({ day: d, isToday: isSameDay(date, today), date })
  }
  while (cells.length % 7 !== 0) cells.push({ day: null, isToday: false, date: null })

  const monthName = new Intl.DateTimeFormat(intlLocale, { month: 'long' }).format(new Date(year, month, 1))
  const weekDayLabels = getWeekDayLabels(intlLocale, 'narrow')

  return (
    <div className="bg-white dark:bg-gray-800 rounded-xl p-3 hover:ring-2 hover:ring-indigo-500 transition-all">
      {/* Month header — click navigates to month view */}
      <button
        onClick={onClick}
        className="text-sm font-semibold text-gray-900 dark:text-white mb-2 capitalize hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors text-left w-full"
      >
        {monthName}
      </button>
      <div className="grid grid-cols-7">
        {weekDayLabels.map((d, i) => (
          <div key={i} className="text-center text-xs text-gray-400 dark:text-gray-500 pb-1">{d}</div>
        ))}
        {cells.map((cell, i) =>
          cell.day !== null && cell.date !== null ? (
            <button
              key={i}
              onClick={() => onDayClick(cell.date!)}
              className={`text-center text-xs leading-5 rounded-full w-5 h-5 mx-auto flex items-center justify-center hover:bg-indigo-100 dark:hover:bg-indigo-900/40 transition-colors ${
                cell.isToday ? 'bg-indigo-600 text-white font-bold' : 'text-gray-600 dark:text-gray-300'
              }`}
            >
              {cell.day}
            </button>
          ) : (
            <div key={i} className="w-5 h-5 mx-auto" />
          )
        )}
      </div>
    </div>
  )
}

function YearView({ current, today, intlLocale, onMonthClick, onDayClick }: {
  current: Date; today: Date; intlLocale: string
  onMonthClick: (y: number, m: number) => void
  onDayClick: (date: Date) => void
}) {
  const year = current.getFullYear()
  return (
    <div className="h-full overflow-auto p-6">
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
        {Array.from({ length: 12 }, (_, m) => (
          <MiniMonth
            key={m} year={year} month={m} today={today}
            intlLocale={intlLocale}
            onClick={() => onMonthClick(year, m)}
            onDayClick={onDayClick}
          />
        ))}
      </div>
    </div>
  )
}

// ── Page ───────────────────────────────────────────────────────────────────────

export function CalendarPage() {
  const { t, i18n } = useTranslation()
  const [view, setView] = useState<ViewMode>('month')
  const [current, setCurrent] = useState(() => {
    const d = new Date()
    d.setHours(0, 0, 0, 0)
    return d
  })
  const [selectedDay, setSelectedDay] = useState<Date | null>(null)

  const today = new Date()
  today.setHours(0, 0, 0, 0)

  const intlLocale = getIntlLocale(i18n.language || 'en')

  const navigate = (delta: number) => {
    setCurrent(prev => {
      const d = new Date(prev)
      if (view === 'week') d.setDate(d.getDate() + delta * 7)
      else if (view === 'month') d.setMonth(d.getMonth() + delta)
      else d.setFullYear(d.getFullYear() + delta)
      return d
    })
  }

  const goToday = () => {
    const d = new Date()
    d.setHours(0, 0, 0, 0)
    setCurrent(d)
  }

  const viewLabels: Record<ViewMode, string> = {
    week: t('calendar.view.week'),
    month: t('calendar.view.month'),
    year: t('calendar.view.year'),
  }

  let periodLabel = ''
  if (view === 'week') {
    const ws = getWeekStart(current)
    const we = new Date(ws)
    we.setDate(ws.getDate() + 6)
    const fmt = new Intl.DateTimeFormat(intlLocale, { month: 'short', day: 'numeric' })
    if (ws.getMonth() === we.getMonth()) {
      periodLabel = `${fmt.format(ws)} – ${we.getDate()}, ${we.getFullYear()}`
    } else if (ws.getFullYear() === we.getFullYear()) {
      periodLabel = `${fmt.format(ws)} – ${fmt.format(we)}, ${we.getFullYear()}`
    } else {
      const fmtY = new Intl.DateTimeFormat(intlLocale, { month: 'short', day: 'numeric', year: 'numeric' })
      periodLabel = `${fmtY.format(ws)} – ${fmtY.format(we)}`
    }
  } else if (view === 'month') {
    const s = new Intl.DateTimeFormat(intlLocale, { month: 'long', year: 'numeric' }).format(current)
    periodLabel = s.charAt(0).toUpperCase() + s.slice(1)
  } else {
    periodLabel = current.getFullYear().toString()
  }

  const handleDayClick = (date: Date) => {
    const d = new Date(date)
    d.setHours(0, 0, 0, 0)
    setSelectedDay(d)
  }

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Toolbar */}
      <div className="shrink-0 flex items-center justify-between px-6 py-3 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div className="flex items-center gap-2">
          <button
            onClick={goToday}
            className="px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            {t('calendar.today')}
          </button>
          <button
            onClick={() => navigate(-1)}
            className="p-1.5 rounded-md text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            aria-label="Previous"
          >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="w-5 h-5">
              <path fillRule="evenodd" d="M11.78 5.22a.75.75 0 0 1 0 1.06L8.06 10l3.72 3.72a.75.75 0 1 1-1.06 1.06l-4.25-4.25a.75.75 0 0 1 0-1.06l4.25-4.25a.75.75 0 0 1 1.06 0Z" clipRule="evenodd" />
            </svg>
          </button>
          <button
            onClick={() => navigate(1)}
            className="p-1.5 rounded-md text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            aria-label="Next"
          >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="w-5 h-5">
              <path fillRule="evenodd" d="M8.22 5.22a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.75.75 0 1 1-1.06-1.06L11.94 10 8.22 6.28a.75.75 0 0 1 0-1.06Z" clipRule="evenodd" />
            </svg>
          </button>
          <span className="text-base font-semibold text-gray-900 dark:text-white ml-1 select-none">
            {periodLabel}
          </span>
        </div>
        <div className="flex rounded-lg border border-gray-300 dark:border-gray-600 overflow-hidden">
          {(['week', 'month', 'year'] as ViewMode[]).map(v => (
            <button
              key={v}
              onClick={() => setView(v)}
              className={`px-4 py-1.5 text-sm font-medium transition-colors ${
                view === v
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
              }`}
            >
              {viewLabels[v]}
            </button>
          ))}
        </div>
      </div>

      {/* Calendar body */}
      <div className="flex-1 overflow-hidden bg-gray-50 dark:bg-gray-900">
        {view === 'week' && (
          <WeekView current={current} today={today} intlLocale={intlLocale} onDayClick={handleDayClick} />
        )}
        {view === 'month' && (
          <MonthView current={current} today={today} intlLocale={intlLocale} onDayClick={handleDayClick} />
        )}
        {view === 'year' && (
          <YearView
            current={current} today={today} intlLocale={intlLocale}
            onMonthClick={(y, m) => { setView('month'); setCurrent(new Date(y, m, 1)) }}
            onDayClick={handleDayClick}
          />
        )}
      </div>

      {/* Day detail panel */}
      <CalendarDayPanel
        day={selectedDay}
        intlLocale={intlLocale}
        onClose={() => setSelectedDay(null)}
      />
    </div>
  )
}
