import client from './client'
import type { PagedResult, SortDirection } from './common'
import type { CategoryPaymentObligation } from './categories'

export type PaymentFrequency = 'Weekly' | 'Monthly' | 'Quarterly' | 'Yearly'

export const FREQUENCY_LABELS: Record<PaymentFrequency, string> = {
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  Quarterly: 'Quarterly',
  Yearly: 'Yearly',
}

const DAY_OF_WEEK = ['', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

export function formatDayOfPeriod(frequency: PaymentFrequency, day: number): string {
  if (frequency === 'Weekly') return DAY_OF_WEEK[day] ?? `Day ${day}`
  return `Day ${day}`
}

export interface MandatoryPaymentCurrency {
  id: string; code: string; name: string; symbol: string; isActive: boolean
}

export interface MandatoryPaymentAccount {
  id: string; name: string; type: string; balance: number
  currencyId: string; currency: MandatoryPaymentCurrency | null; isActive: boolean
}

export interface MandatoryPaymentCategory {
  id: string; name: string; type: string
  color: string | null; icon: string | null
  parentId: string | null; parentName: string | null
  paymentObligation: CategoryPaymentObligation | null
  isActive: boolean
}

export interface MandatoryPayment {
  id: string
  name: string
  amount: number
  currencyId: string
  currency: MandatoryPaymentCurrency | null
  accountId: string
  account: MandatoryPaymentAccount | null
  categoryId: string | null
  category: MandatoryPaymentCategory | null
  frequency: PaymentFrequency
  dayOfPeriod: number
  notes: string | null
  isActive: boolean
}

export interface MandatoryPaymentQuery {
  search?: string
  isActive?: boolean
  currencyId?: string
  accountId?: string
  categoryId?: string
  frequency?: PaymentFrequency
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: SortDirection
}

export interface CreateMandatoryPaymentRequest {
  name: string
  amount: number
  currencyId: string
  accountId: string
  categoryId: string | null
  frequency: PaymentFrequency
  dayOfPeriod: number
  notes: string | null
}

export type UpdateMandatoryPaymentRequest = CreateMandatoryPaymentRequest & { id: string }

export const mandatoryPaymentsApi = {
  getAll: (params?: MandatoryPaymentQuery) =>
    client.get<PagedResult<MandatoryPayment>>('/mandatory-payments', { params }).then(r => r.data),
  getById: (id: string) =>
    client.get<MandatoryPayment>(`/mandatory-payments/${id}`).then(r => r.data),
  create: (req: CreateMandatoryPaymentRequest) =>
    client.post<MandatoryPayment>('/mandatory-payments', req).then(r => r.data),
  update: (id: string, req: UpdateMandatoryPaymentRequest) =>
    client.put<MandatoryPayment>(`/mandatory-payments/${id}`, req).then(r => r.data),
  activate: (id: string) =>
    client.patch<MandatoryPayment>(`/mandatory-payments/${id}/activate`).then(r => r.data),
  deactivate: (id: string) =>
    client.patch<MandatoryPayment>(`/mandatory-payments/${id}/deactivate`).then(r => r.data),
}
