export const PAGE_SIZES = [10, 25, 50, 100] as const
export type PageSize = typeof PAGE_SIZES[number]
export type SortDirection = 'Asc' | 'Desc'

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface PagedQuery {
  search?: string
  isActive?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: SortDirection
}
