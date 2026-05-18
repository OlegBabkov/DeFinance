import client from './client'
import type { PagedResult, SortDirection } from './common'

export interface TransactionCurrency {
  id: string; code: string; name: string; symbol: string; isActive: boolean
}

export interface TransactionAccount {
  id: string; name: string; type: string; balance: number
  currencyId: string; currency: TransactionCurrency | null; isActive: boolean
}

export interface TransactionCategory {
  id: string; name: string; type: string
  color: string | null; icon: string | null; parentId: string | null; isActive: boolean
}

export interface TransactionCounterparty {
  id: string; name: string; type: string; contactInfo: string | null; isActive: boolean
}

export interface TransactionPaymentStatus {
  id: string; name: string; description: string | null; isActive: boolean
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

export const transactionsApi = {
  getAll: (params?: TransactionQuery) =>
    client.get<PagedResult<Transaction>>('/transactions', { params }).then(r => r.data),
  getById: (id: string) =>
    client.get<Transaction>(`/transactions/${id}`).then(r => r.data),
}
