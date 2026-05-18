import type { SortDirection } from '../api/common'

interface Props {
  label: string
  field: string
  sortBy: string | null
  sortDirection: SortDirection
  onSort: (field: string) => void
  className?: string
}

export function SortableHeader({ label, field, sortBy, sortDirection, onSort, className = '' }: Props) {
  const active = sortBy === field
  return (
    <th
      onClick={() => onSort(field)}
      className={`px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400 cursor-pointer select-none hover:text-gray-700 dark:hover:text-gray-200 whitespace-nowrap ${className}`}
    >
      {label}
      <span className="ml-1 inline-block w-2.5 text-center opacity-70">
        {active ? (sortDirection === 'Asc' ? '↑' : '↓') : ''}
      </span>
    </th>
  )
}
