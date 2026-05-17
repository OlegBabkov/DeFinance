import { useEffect, useState } from 'react'
import { currenciesApi, type Currency } from '../api/currencies'

export function CurrenciesPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    currenciesApi
      .getAll()
      .then(setCurrencies)
      .catch(() => setError('Failed to load currencies'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="p-8 text-gray-500">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Currencies</h1>
      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              {['Symbol', 'Code', 'Name'].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {currencies.map(currency => (
              <tr key={currency.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-bold text-gray-700 w-12">{currency.symbol}</td>
                <td className="px-4 py-3 font-mono text-gray-900">{currency.code}</td>
                <td className="px-4 py-3 text-gray-600">{currency.name}</td>
              </tr>
            ))}
            {currencies.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-8 text-center text-gray-400">
                  No currencies.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
