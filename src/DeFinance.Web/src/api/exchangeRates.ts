import client from './client'

export interface ExchangeRateLatest {
  currencyId: string
  currencyCode: string
  rate: number
  previousRate: number | null
  date: string
}

export interface ExchangeRateHistory {
  date: string
  rate: number
}

export const exchangeRatesApi = {
  getLatest: () =>
    client.get<ExchangeRateLatest[]>('/exchange-rates/latest').then(r => r.data),
  getHistory: (currencyId: string, days = 30) =>
    client.get<ExchangeRateHistory[]>(`/exchange-rates/${currencyId}/history`, { params: { days } }).then(r => r.data),
  sync: () =>
    client.post<{ synced: number }>('/exchange-rates/sync').then(r => r.data),
}
