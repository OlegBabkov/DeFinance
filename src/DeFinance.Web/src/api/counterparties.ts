import client from './client'
import type { PagedQuery, PagedResult } from './common'

export type CounterpartyType = 'Person' | 'Company' | 'Other'

export interface Counterparty {
  id: string
  name: string
  type: CounterpartyType
  contactInfo: string | null
  isActive: boolean
}

export interface CreateCounterpartyRequest {
  name: string
  type: CounterpartyType
  contactInfo: string | null
}

export interface UpdateCounterpartyRequest {
  name: string
  type: CounterpartyType
  contactInfo: string | null
}

export interface CounterpartyQuery extends PagedQuery {
  type?: CounterpartyType
}

export const counterpartiesApi = {
  getAll: (params?: CounterpartyQuery) =>
    client.get<PagedResult<Counterparty>>('/counterparties', { params }).then(r => r.data),
  getById: (id: string) => client.get<Counterparty>(`/counterparties/${id}`).then(r => r.data),
  create: (req: CreateCounterpartyRequest) => client.post<Counterparty>('/counterparties', req).then(r => r.data),
  update: (id: string, req: UpdateCounterpartyRequest) => client.put<Counterparty>(`/counterparties/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Counterparty>(`/counterparties/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Counterparty>(`/counterparties/${id}/deactivate`).then(r => r.data),
}
