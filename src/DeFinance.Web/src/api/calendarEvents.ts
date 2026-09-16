import client from './client'
import type { PagedResult } from './common'

export type CalendarEventType = 'Event' | 'Payment'

export interface CalendarEvent {
  id: string
  date: string           // YYYY-MM-DD
  eventType: CalendarEventType
  // Event fields
  title: string | null
  timeFrom: string | null  // HH:mm
  timeTo: string | null    // HH:mm
  // Payment fields
  accountId: string | null
  accountName: string | null
  accountCurrencySymbol: string | null
  categoryId: string | null
  categoryName: string | null
  categoryColor: string | null
  categoryIcon: string | null
  counterpartyId: string | null
  counterpartyName: string | null
  paymentStatusId: string | null
  paymentStatusName: string | null
  paymentStatusColor: string | null
  inCurrencyId: string | null
  sum: number | null
  exchangeRate: number | null
  // Common
  notes: string | null
}

export interface CreateCalendarEventRequest {
  date: string
  eventType: CalendarEventType
  title?: string | null
  timeFrom?: string | null
  timeTo?: string | null
  accountId?: string | null
  categoryId?: string | null
  counterpartyId?: string | null
  paymentStatusId?: string | null
  inCurrencyId?: string | null
  sum?: number | null
  exchangeRate?: number | null
  notes?: string | null
}

export const calendarEventsApi = {
  getByDate: (date: string) =>
    client.get<PagedResult<CalendarEvent>>('/calendar-events', { params: { date, pageSize: 200 } }).then(r => r.data),
  create: (req: CreateCalendarEventRequest) =>
    client.post<CalendarEvent>('/calendar-events', req).then(r => r.data),
  remove: (id: string) =>
    client.delete(`/calendar-events/${id}`),
}
