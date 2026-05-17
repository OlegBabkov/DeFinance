import client from './client'

export interface Currency {
  id: string
  code: string
  name: string
  symbol: string
  isActive: boolean
}

export interface CreateCurrencyRequest {
  code: string
  name: string
  symbol: string
}

export interface UpdateCurrencyRequest {
  name: string
  symbol: string
}

export const currenciesApi = {
  getAll: () => client.get<Currency[]>('/currencies').then(r => r.data),
  getById: (id: string) => client.get<Currency>(`/currencies/${id}`).then(r => r.data),
  create: (req: CreateCurrencyRequest) => client.post<Currency>('/currencies', req).then(r => r.data),
  update: (id: string, req: UpdateCurrencyRequest) => client.put<Currency>(`/currencies/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Currency>(`/currencies/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Currency>(`/currencies/${id}/deactivate`).then(r => r.data),
}
