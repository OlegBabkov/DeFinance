import client from './client'

export interface Currency {
  id: string
  code: string
  name: string
  symbol: string
}

export const currenciesApi = {
  getAll: () => client.get<Currency[]>('/currencies').then(r => r.data),
  getById: (id: string) => client.get<Currency>(`/currencies/${id}`).then(r => r.data),
}
