import { useState, type Dispatch, type SetStateAction } from 'react'

export function usePersistedState<T>(key: string, initial: T): [T, Dispatch<SetStateAction<T>>] {
  const [state, setState] = useState<T>(() => {
    try {
      const stored = localStorage.getItem(key)
      return stored !== null ? (JSON.parse(stored) as T) : initial
    } catch {
      return initial
    }
  })

  const setPersisted: Dispatch<SetStateAction<T>> = (action) => {
    setState(prev => {
      const next = typeof action === 'function' ? (action as (prev: T) => T)(prev) : action
      try { localStorage.setItem(key, JSON.stringify(next)) } catch {}
      return next
    })
  }

  return [state, setPersisted]
}
