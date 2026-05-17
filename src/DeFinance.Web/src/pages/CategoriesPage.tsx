import { useEffect, useState } from 'react'
import { categoriesApi, type Category } from '../api/categories'

const TYPE_COLOR: Record<string, string> = {
  Income: 'bg-green-100 text-green-700',
  Expense: 'bg-red-100 text-red-700',
  Transfer: 'bg-blue-100 text-blue-700',
}

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    categoriesApi
      .getAll()
      .then(setCategories)
      .catch(() => setError('Failed to load categories'))
      .finally(() => setLoading(false))
  }, [])

  const toggle = async (category: Category) => {
    const updated = category.isActive
      ? await categoriesApi.deactivate(category.id)
      : await categoriesApi.activate(category.id)
    setCategories(prev => prev.map(c => (c.id === updated.id ? updated : c)))
  }

  const parentName = (parentId: string | null) => {
    if (!parentId) return null
    return categories.find(c => c.id === parentId)?.name ?? '—'
  }

  if (loading) return <div className="p-8 text-gray-500">Loading…</div>
  if (error) return <div className="p-8 text-red-500">{error}</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Categories</h1>
      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              {['Name', 'Type', 'Color', 'Parent', 'Status', ''].map(h => (
                <th key={h} className="px-4 py-3 text-left font-medium text-gray-500">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {categories.map(cat => (
              <tr key={cat.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-gray-900">
                  {cat.icon && <span className="mr-1">{cat.icon}</span>}
                  {cat.name}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${TYPE_COLOR[cat.type]}`}
                  >
                    {cat.type}
                  </span>
                </td>
                <td className="px-4 py-3">
                  {cat.color ? (
                    <div className="flex items-center gap-2">
                      <span
                        className="inline-block w-4 h-4 rounded-full border border-gray-200"
                        style={{ backgroundColor: cat.color }}
                      />
                      <span className="text-gray-500 font-mono text-xs">{cat.color}</span>
                    </div>
                  ) : (
                    <span className="text-gray-300">—</span>
                  )}
                </td>
                <td className="px-4 py-3 text-gray-500">{parentName(cat.parentId) ?? '—'}</td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                      cat.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
                    }`}
                  >
                    {cat.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => toggle(cat)}
                    className="text-xs text-indigo-600 hover:underline"
                  >
                    {cat.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </td>
              </tr>
            ))}
            {categories.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-gray-400">
                  No categories yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
