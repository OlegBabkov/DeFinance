import client from './client'

export interface PlanFactLineRow {
  name: string
  amount: number
}

export interface PlanFactCategoryRow {
  categoryId: string
  categoryName: string
  plan: number
  fact: number
  lines: PlanFactLineRow[]
}

export interface PlanFactMonthData {
  year: number
  month: number
  openingBalance: number
  incomeCategories: PlanFactCategoryRow[]
  expenseCategories: PlanFactCategoryRow[]
}

export interface PlanFactSummaryResponse {
  months: PlanFactMonthData[]
}

export const planFactApi = {
  getSummary: (year: number, months: number[]): Promise<PlanFactSummaryResponse> => {
    const params = new URLSearchParams()
    params.append('year', year.toString())
    months.forEach(m => params.append('months', m.toString()))
    return client.get<PlanFactSummaryResponse>(`/plan-fact/summary?${params.toString()}`).then((r: { data: PlanFactSummaryResponse }) => r.data)
  },

  upsertEntry: (
    categoryId: string,
    year: number,
    month: number,
    plannedAmount: number,
    lines: PlanFactLineRow[],
  ): Promise<void> =>
    client.put('/plan-fact/entry', { categoryId, year, month, plannedAmount, lines }).then(() => undefined),
}
