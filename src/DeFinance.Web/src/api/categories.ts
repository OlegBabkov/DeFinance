import client from './client'

export type CategoryType = 'Income' | 'Expense' | 'Transfer'

export interface Category {
  id: string
  name: string
  type: CategoryType
  color: string | null
  icon: string | null
  parentId: string | null
  isActive: boolean
}

export interface CreateCategoryRequest {
  name: string
  type: number
  color: string | null
  icon: string | null
  parentId: string | null
}

export interface UpdateCategoryRequest {
  id: string
  name: string
  color: string | null
  icon: string | null
}

export const categoriesApi = {
  getAll: () => client.get<Category[]>('/categories').then(r => r.data),
  getById: (id: string) => client.get<Category>(`/categories/${id}`).then(r => r.data),
  create: (req: CreateCategoryRequest) => client.post<Category>('/categories', req).then(r => r.data),
  update: (id: string, req: UpdateCategoryRequest) => client.put<Category>(`/categories/${id}`, req).then(r => r.data),
  activate: (id: string) => client.patch<Category>(`/categories/${id}/activate`).then(r => r.data),
  deactivate: (id: string) => client.patch<Category>(`/categories/${id}/deactivate`).then(r => r.data),
}
