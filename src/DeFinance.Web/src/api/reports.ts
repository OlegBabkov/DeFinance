import client from './client'

export type ReportType = 'CashFlowStatement' | 'ExpenseCategoryBreakdown' | 'AccountBalanceSummary' | 'CounterpartySpending'
export type ReportPeriod = 'OneDay' | 'LastWeek' | 'CurrentMonth' | 'LastMonth' | 'LastTwoMonths' | 'LastHalfYear' | 'LastYear'
export type ReportStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed'

export interface Report {
  id: string
  type: ReportType
  period: ReportPeriod
  status: ReportStatus
  accountId: string | null
  categoryId: string | null
  fileName: string | null
  errorMessage: string | null
  createdAt: string
  completedAt: string | null
}

export interface GenerateReportRequest {
  type: ReportType
  period: ReportPeriod
  accountId?: string | null
  categoryIds?: string[]
  counterpartyIds?: string[]
}

export const REPORT_TYPE_LABELS: Record<ReportType, string> = {
  CashFlowStatement:        'Cash Flow Statement',
  ExpenseCategoryBreakdown: 'Expense Category Breakdown',
  AccountBalanceSummary:    'Account Balance Summary',
  CounterpartySpending:     'Counterparty Spending',
}

export const REPORT_PERIOD_LABELS: Record<ReportPeriod, string> = {
  OneDay:        'Last 24 Hours',
  LastWeek:      'Last 7 Days',
  CurrentMonth:  'Current Month',
  LastMonth:     'Last Month',
  LastTwoMonths: 'Last Two Months',
  LastHalfYear:  'Last Half Year',
  LastYear:      'Last Year',
}

export const reportsApi = {
  generate: (req: GenerateReportRequest) =>
    client.post<Report>('/reports', req).then(r => r.data),
  getAll: () =>
    client.get<Report[]>('/reports').then(r => r.data),
  getById: (id: string) =>
    client.get<Report>(`/reports/${id}`).then(r => r.data),
  downloadUrl: (id: string) => `/api/reports/${id}/download`,
}
