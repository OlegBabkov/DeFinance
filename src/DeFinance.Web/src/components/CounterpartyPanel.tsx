import { type Counterparty } from '../api/counterparties'

interface Props {
  counterparty: Counterparty | null
  onClose: () => void
}

export function CounterpartyPanel({ counterparty, onClose }: Props) {
  const open = counterparty !== null

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        className={`fixed inset-0 z-30 bg-black/20 dark:bg-black/40 transition-opacity duration-300 ${open ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'}`}
      />

      {/* Panel */}
      <div
        className={`fixed top-12 right-0 bottom-0 w-96 z-40 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 shadow-xl flex flex-col transform transition-transform duration-300 ease-in-out ${open ? 'translate-x-0' : 'translate-x-full'}`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-700 shrink-0">
          <div className="min-w-0">
            <span className="text-sm font-semibold text-gray-800 dark:text-gray-100 truncate block">
              {counterparty?.name ?? 'Counterparty Details'}
            </span>
            {counterparty?.type && (
              <span className="text-xs text-gray-400 dark:text-gray-500">{counterparty.type}</span>
            )}
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none ml-3 shrink-0"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5">
          <p className="text-xs text-gray-400 dark:text-gray-500">No content yet.</p>
        </div>
      </div>
    </>
  )
}
