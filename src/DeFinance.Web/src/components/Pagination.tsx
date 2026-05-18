import { PAGE_SIZES, type PageSize } from '../api/common'

interface Props {
  page: number
  pageSize: PageSize
  totalCount: number
  totalPages: number
  onPageChange: (page: number) => void
  onPageSizeChange: (size: PageSize) => void
}

export function Pagination({ page, pageSize, totalCount, totalPages, onPageChange, onPageSizeChange }: Props) {
  const from = totalCount === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalCount)

  return (
    <div className="flex items-center justify-between px-4 py-2.5 border-t border-gray-200 dark:border-gray-700 shrink-0 text-xs text-gray-600 dark:text-gray-400">
      <span>
        {totalCount === 0 ? 'No results' : `Showing ${from}–${to} of ${totalCount}`}
      </span>
      <div className="flex items-center gap-5">
        <label className="flex items-center gap-1.5">
          Per page
          <select
            value={pageSize}
            onChange={e => onPageSizeChange(Number(e.target.value) as PageSize)}
            className="ml-0.5 rounded border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-200 text-xs py-0.5 px-1 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          >
            {PAGE_SIZES.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
        </label>
        <div className="flex items-center gap-1">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            className="px-2 py-0.5 rounded disabled:opacity-40 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:cursor-default transition-colors"
          >
            ‹ Prev
          </button>
          <span className="px-2 tabular-nums">
            {page} / {Math.max(1, totalPages)}
          </span>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            className="px-2 py-0.5 rounded disabled:opacity-40 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:cursor-default transition-colors"
          >
            Next ›
          </button>
        </div>
      </div>
    </div>
  )
}
