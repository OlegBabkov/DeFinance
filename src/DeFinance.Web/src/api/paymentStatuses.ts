import client from './client'
import type { PagedQuery, PagedResult } from './common'

export interface PaymentStatus {
  id: string
  name: string
  description: string | null
  color: string | null
  isActive: boolean
}

export interface CreatePaymentStatusRequest {
  name: string
  description: string | null
  color: string | null
}

export interface UpdatePaymentStatusRequest {
  name: string
  description: string | null
  color: string | null
}

export interface PaymentStatusQuery extends PagedQuery {}

export const paymentStatusesApi = {
  getAll: (params?: PaymentStatusQuery) =>
    client.get<PagedResult<PaymentStatus>>('/payment-statuses', { params }).then(r => r.data),
  getById: (id: string) => client.get<PaymentStatus>(`/payment-statuses/${id}`).then(r => r.data),
  create: (req: CreatePaymentStatusRequest) => client.post<PaymentStatus>('/payment-statuses', req).then(r => r.data),
  update: (id: string, req: UpdatePaymentStatusRequest) => client.put<PaymentStatus>(`/payment-statuses/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<PaymentStatus>(`/payment-statuses/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<PaymentStatus>(`/payment-statuses/${id}/deactivate`).then(r => r.data),
}
