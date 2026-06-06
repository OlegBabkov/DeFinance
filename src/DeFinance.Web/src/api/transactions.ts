import client from './client'
import type { PagedResult, SortDirection } from './common'

export interface TransactionListResult extends PagedResult<Transaction> {
  totalSum: number
  totalAmountInCurrency: number
}

export interface TransactionCurrency {
  id: string; code: string; name: string; symbol: string; isActive: boolean
}

export interface TransactionAccount {
  id: string; name: string; type: string; balance: number
  currencyId: string; currency: TransactionCurrency | null; isActive: boolean
}

export interface TransactionCategory {
  id: string; name: string; type: string
  color: string | null; icon: string | null
  parentId: string | null; parentName: string | null
  paymentObligation: string | null
  isActive: boolean
}

export interface TransactionCounterparty {
  id: string; name: string; type: string; contactInfo: string | null; isActive: boolean
}

export interface TransactionPaymentStatus {
  id: string; name: string; description: string | null; color: string | null; isActive: boolean
}

export interface Transaction {
  id: string
  dateTime: string
  sum: number
  exchangeRate: number
  amountInCurrency: number
  inCurrencyId: string
  inCurrency: TransactionCurrency | null
  accountId: string
  account: TransactionAccount | null
  categoryId: string
  category: TransactionCategory | null
  counterpartyId: string | null
  counterparty: TransactionCounterparty | null
  paymentStatusId: string
  paymentStatus: TransactionPaymentStatus | null
  notes: string | null
}

export interface TransactionQuery {
  dateFrom?: string
  dateTo?: string
  accountId?: string
  categoryId?: string
  counterpartyId?: string
  paymentStatusId?: string
  inCurrencyId?: string
  notes?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: SortDirection
}

export interface CreateTransactionRequest {
  dateTime: string
  sum: number
  exchangeRate: number
  inCurrencyId: string
  accountId: string
  categoryId: string
  counterpartyId: string | null
  paymentStatusId: string
  notes: string | null
}

export type UpdateTransactionRequest = CreateTransactionRequest & { id: string }

export const transactionsApi = {
  getAll: (params?: TransactionQuery) =>
    client.get<TransactionListResult>('/transactions', { params }).then(r => r.data),
  getById: (id: string) =>
    client.get<Transaction>(`/transactions/${id}`).then(r => r.data),
  getBalanceBefore: (id: string) =>
    client.get<number>(`/transactions/${id}/balance-before`).then(r => r.data),
  create: (req: CreateTransactionRequest) =>
    client.post<Transaction>('/transactions', req).then(r => r.data),
  update: (id: string, req: UpdateTransactionRequest) =>
    client.put<Transaction>(`/transactions/${id}`, req).then(r => r.data),
  remove: (id: string) =>
    client.delete(`/transactions/${id}`),
}
