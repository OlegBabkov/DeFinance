import client from './client'

export type CategoryType = 'Income' | 'Expense'
export type CategoryPaymentObligation = 'SepaTransfer' | 'Mandatory' | 'NonMandatory'

export const PAYMENT_OBLIGATION_LABELS: Record<CategoryPaymentObligation, string> = {
  SepaTransfer: 'SEPA Transfer',
  Mandatory: 'Mandatory',
  NonMandatory: 'Non-Mandatory',
}

export interface Category {
  id: string
  name: string
  type: CategoryType
  color: string | null
  icon: string | null
  parentId: string | null
  paymentObligation: CategoryPaymentObligation | null
  isActive: boolean
}

export interface CreateCategoryRequest {
  name: string
  type: CategoryType
  color: string | null
  icon: string | null
  parentId: string | null
  paymentObligation: CategoryPaymentObligation | null
}

export interface UpdateCategoryRequest {
  name: string
  color: string | null
  icon: string | null
  paymentObligation: CategoryPaymentObligation | null
}

export const categoriesApi = {
  getAll: () => client.get<Category[]>('/categories').then(r => r.data),
  getById: (id: string) => client.get<Category>(`/categories/${id}`).then(r => r.data),
  create: (req: CreateCategoryRequest) => client.post<Category>('/categories', req).then(r => r.data),
  update: (id: string, req: UpdateCategoryRequest) => client.put<Category>(`/categories/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Category>(`/categories/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Category>(`/categories/${id}/deactivate`).then(r => r.data),
}
