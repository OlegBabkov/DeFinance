import client from './client'
import type { PagedQuery, PagedResult } from './common'

export type AccountType = 'Checking' | 'Savings' | 'Credit' | 'Cash' | 'Investment'

export interface AccountCurrency {
  id: string
  code: string
  name: string
  symbol: string
  isActive: boolean
}

export interface Account {
  id: string
  name: string
  type: AccountType
  balance: number
  currencyId: string
  currency: AccountCurrency | null
  isActive: boolean
  sortOrder: number
}

export interface CreateAccountRequest {
  name: string
  type: AccountType
  initialBalance: number
  currencyId: string
}

export interface UpdateAccountRequest {
  name: string
}

export interface AccountQuery extends PagedQuery {
  type?: AccountType
  currencyId?: string
}

export const accountsApi = {
  getAll: (params?: AccountQuery) =>
    client.get<PagedResult<Account>>('/accounts', { params }).then(r => r.data),
  getById: (id: string) => client.get<Account>(`/accounts/${id}`).then(r => r.data),
  create: (req: CreateAccountRequest) => client.post<Account>('/accounts', req).then(r => r.data),
  update: (id: string, req: UpdateAccountRequest) => client.put<Account>(`/accounts/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Account>(`/accounts/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Account>(`/accounts/${id}/deactivate`).then(r => r.data),
  reorder: (orderedIds: string[]) => client.put('/accounts/reorder', { orderedIds }),
}
