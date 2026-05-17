import client from './client'

export type AccountType = 'Checking' | 'Savings' | 'Credit' | 'Cash' | 'Investment'

export interface Account {
  id: string
  name: string
  type: AccountType
  balance: number
  currencyId: string
  isActive: boolean
}

export interface CreateAccountRequest {
  name: string
  type: number
  initialBalance: number
  currencyId: string
}

export interface UpdateAccountRequest {
  id: string
  name: string
}

export const accountsApi = {
  getAll: () => client.get<Account[]>('/accounts').then(r => r.data),
  getById: (id: string) => client.get<Account>(`/accounts/${id}`).then(r => r.data),
  create: (req: CreateAccountRequest) => client.post<Account>('/accounts', req).then(r => r.data),
  update: (id: string, req: UpdateAccountRequest) => client.put<Account>(`/accounts/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Account>(`/accounts/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Account>(`/accounts/${id}/deactivate`).then(r => r.data),
}
