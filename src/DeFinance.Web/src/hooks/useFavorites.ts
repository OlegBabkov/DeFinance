import { useState } from 'react'

const KEYS = {
  categories:    'definance_fav_categories',
  counterparties: 'definance_fav_counterparties',
}

function loadSet(key: string): Set<string> {
  try {
    const raw = localStorage.getItem(key)
    return raw ? new Set(JSON.parse(raw) as string[]) : new Set()
  } catch {
    return new Set()
  }
}

function saveSet(key: string, set: Set<string>) {
  localStorage.setItem(key, JSON.stringify([...set]))
}

export function useFavorites(type: 'categories' | 'counterparties') {
  const key = KEYS[type]
  const [favorites, setFavorites] = useState<Set<string>>(() => loadSet(key))

  const isFavorite = (id: string) => favorites.has(id)

  /** Returns true if the item was added, false if removed. */
  const toggle = (id: string): boolean => {
    const adding = !favorites.has(id)
    setFavorites(prev => {
      const next = new Set(prev)
      adding ? next.add(id) : next.delete(id)
      saveSet(key, next)
      return next
    })
    return adding
  }

  return { favorites, toggle, isFavorite }
}

/** Sort items so favourites appear first, preserving original relative order. */
export function sortByFavorites<T extends { id: string }>(items: T[], favorites: Set<string>): T[] {
  return [...items].sort((a, b) => {
    const af = favorites.has(a.id) ? 0 : 1
    const bf = favorites.has(b.id) ? 0 : 1
    return af - bf
  })
}
