import { useEffect, useState } from 'react'
import { transactionsApi, type Transaction } from '../api/transactions'
import { Spinner } from './Spinner'

interface Props {
  transaction: Transaction | null
  onClose: () => void
}

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

function fmtNum(n: number, decimals = 2) {
  return n.toLocaleString('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

interface DetailRowProps { label: string; value: React.ReactNode }
function DetailRow({ label, value }: DetailRowProps) {
  return (
    <tr>
      <td className="py-1.5 pr-4 text-xs font-medium text-gray-400 dark:text-gray-500 whitespace-nowrap align-top w-36">
        {label}
      </td>
      <td className="py-1.5 text-sm text-gray-800 dark:text-gray-200 break-words">
        {value}
      </td>
    </tr>
  )
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <p className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider mb-3">
      {children}
    </p>
  )
}

export function TransactionPanel({ transaction, onClose }: Props) {
  const open = transaction !== null
  const tx = transaction

  const [balanceBefore, setBalanceBefore] = useState<number | null>(null)
  const [balanceLoading, setBalanceLoading] = useState(false)

  useEffect(() => {
    if (!tx) { setBalanceBefore(null); return }
    setBalanceLoading(true)
    transactionsApi.getBalanceBefore(tx.id)
      .then(setBalanceBefore)
      .catch(() => setBalanceBefore(null))
      .finally(() => setBalanceLoading(false))
  }, [tx?.id])

  const accountCurrency = tx?.account?.currency
  const mainCurrencySymbol = tx?.inCurrency?.symbol ?? ''

  const detailRows: { label: string; value: React.ReactNode }[] = tx ? [
    {
      label: 'Date',
      value: <span className="font-mono text-xs">{fmtDate(tx.dateTime)}</span>,
    },
    {
      label: 'Sum',
      value: (
        <span className="font-mono">
          <span className="text-xs text-gray-400 dark:text-gray-500 mr-0.5">{accountCurrency?.symbol ?? ''}</span>
          {fmtNum(tx.sum)}
          {accountCurrency && <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">{accountCurrency.code}</span>}
        </span>
      ),
    },
    ...(tx.exchangeRate !== 1 ? [{
      label: 'Exch. Rate',
      value: <span className="font-mono text-xs">{fmtNum(tx.exchangeRate, 4)}</span>,
    }] : []),
    {
      label: 'In Main Currency',
      value: (
        <span className="font-mono">
          <span className="text-xs text-gray-400 dark:text-gray-500 mr-0.5">{mainCurrencySymbol}</span>
          {fmtNum(tx.amountInCurrency)}
        </span>
      ),
    },
    {
      label: 'Category',
      value: tx.category ? (
        <span className="flex items-center gap-1.5">
          {tx.category.color && <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: tx.category.color }} />}
          {tx.category.icon && <span>{tx.category.icon}</span>}
          {tx.category.name}
          <span className="ml-1 text-xs text-gray-400 dark:text-gray-500">({tx.category.type})</span>
        </span>
      ) : '—',
    },
    ...(tx.counterparty ? [{
      label: 'Counterparty',
      value: tx.counterparty.name,
    }] : []),
    {
      label: 'Payment Status',
      value: tx.paymentStatus ? (
        tx.paymentStatus.color
          ? <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium" style={{ backgroundColor: tx.paymentStatus.color + '25', color: tx.paymentStatus.color }}>{tx.paymentStatus.name}</span>
          : <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-indigo-100 text-indigo-700 dark:bg-indigo-900 dark:text-indigo-300">{tx.paymentStatus.name}</span>
      ) : '—',
    },
    ...(tx.notes ? [{
      label: 'Notes',
      value: <span className="text-gray-600 dark:text-gray-400 italic">{tx.notes}</span>,
    }] : []),
  ] : []

  const categoryRows: { label: string; value: React.ReactNode }[] = tx?.category ? [
    {
      label: 'Name',
      value: (
        <span className="flex items-center gap-1.5">
          {tx.category.color && <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: tx.category.color }} />}
          {tx.category.icon && <span>{tx.category.icon}</span>}
          <span className="font-medium">{tx.category.name}</span>
        </span>
      ),
    },
    ...(tx.category.parentName ? [{
      label: 'Parent',
      value: tx.category.parentName,
    }] : []),
    {
      label: 'Obligation',
      value: tx.category.paymentObligation
        ? <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300">{tx.category.paymentObligation}</span>
        : <span className="text-gray-300 dark:text-gray-600">—</span>,
    },
    {
      label: 'Status',
      value: tx.category.isActive
        ? <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300">Active</span>
        : <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400">Inactive</span>,
    },
  ] : []

  const counterpartyRows: { label: string; value: React.ReactNode }[] = tx?.counterparty ? [
    {
      label: 'Name',
      value: <span className="font-medium">{tx.counterparty.name}</span>,
    },
    {
      label: 'Type',
      value: tx.counterparty.type,
    },
    ...(tx.counterparty.contactInfo ? [{
      label: 'Contact Info',
      value: <span className="text-gray-600 dark:text-gray-400">{tx.counterparty.contactInfo}</span>,
    }] : []),
    {
      label: 'Status',
      value: tx.counterparty.isActive
        ? <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300">Active</span>
        : <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400">Inactive</span>,
    },
  ] : []

  const accountRows: { label: string; value: React.ReactNode }[] = tx?.account ? [
    {
      label: 'Name',
      value: <span className="font-medium">{tx.account.name}</span>,
    },
    {
      label: 'Currency',
      value: accountCurrency
        ? <span>{accountCurrency.symbol} {accountCurrency.code} <span className="text-xs text-gray-400 dark:text-gray-500">— {accountCurrency.name}</span></span>
        : '—',
    },
    {
      label: 'Current Balance',
      value: (
        <span className="font-mono">
          <span className="text-xs text-gray-400 dark:text-gray-500 mr-0.5">{accountCurrency?.symbol ?? ''}</span>
          {fmtNum(tx.account.balance)}
        </span>
      ),
    },
    {
      label: 'Balance before',
      value: balanceLoading
        ? <span className="text-gray-400 dark:text-gray-500"><Spinner size="sm" /></span>
        : balanceBefore !== null
          ? (
            <span className="font-mono">
              <span className="text-xs text-gray-400 dark:text-gray-500 mr-0.5">{accountCurrency?.symbol ?? ''}</span>
              {fmtNum(balanceBefore)}
            </span>
          )
          : <span className="text-gray-300 dark:text-gray-600">—</span>,
    },
  ] : []

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
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-100">Transaction Details</span>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors text-base leading-none"
          >
            ✕
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-5">
          {tx && (
            <>
              {/* — Details block — */}
              <div>
                <SectionTitle>Details</SectionTitle>
                <table className="w-full border-collapse">
                  <tbody>
                    {detailRows.map(r => <DetailRow key={r.label} label={r.label} value={r.value} />)}
                  </tbody>
                </table>
              </div>

              <div className="border-t border-gray-200 dark:border-gray-700" />

              {/* — Category block — */}
              {categoryRows.length > 0 && (
                <div>
                  <SectionTitle>Category</SectionTitle>
                  <table className="w-full border-collapse">
                    <tbody>
                      {categoryRows.map(r => <DetailRow key={r.label} label={r.label} value={r.value} />)}
                    </tbody>
                  </table>
                </div>
              )}

              <div className="border-t border-gray-200 dark:border-gray-700" />

              {/* — Account block — */}
              {accountRows.length > 0 && (
                <div>
                  <SectionTitle>Account</SectionTitle>
                  <table className="w-full border-collapse">
                    <tbody>
                      {accountRows.map(r => <DetailRow key={r.label} label={r.label} value={r.value} />)}
                    </tbody>
                  </table>
                </div>
              )}

              {/* — Counterparty block — */}
              {counterpartyRows.length > 0 && (
                <>
                  <div className="border-t border-gray-200 dark:border-gray-700" />
                  <div>
                    <SectionTitle>Counterparty</SectionTitle>
                    <table className="w-full border-collapse">
                      <tbody>
                        {counterpartyRows.map(r => <DetailRow key={r.label} label={r.label} value={r.value} />)}
                      </tbody>
                    </table>
                  </div>
                </>
              )}

              {/* — Technical Info block — */}
              <div className="border-t border-gray-200 dark:border-gray-700" />
              <div>
                <SectionTitle>Technical Info</SectionTitle>
                <table className="w-full border-collapse">
                  <tbody>
                    <DetailRow
                      label="Transaction ID"
                      value={<span className="font-mono text-xs text-gray-500 dark:text-gray-400 break-all">{tx.id}</span>}
                    />
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>
      </div>
    </>
  )
}
