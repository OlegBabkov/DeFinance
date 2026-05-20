import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
import { currenciesApi, type Currency } from './api/currencies'

const STORAGE_KEY = 'definance_main_currency_id'

interface MainCurrencyContextValue {
  currencies: Currency[]
  mainCurrency: Currency | null
  setMainCurrencyId: (id: string) => void
}

const MainCurrencyContext = createContext<MainCurrencyContextValue | null>(null)

export function MainCurrencyProvider({ children }: { children: ReactNode }) {
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [mainCurrencyId, setMainCurrencyIdState] = useState<string>(
    () => localStorage.getItem(STORAGE_KEY) ?? ''
  )

  useEffect(() => {
    currenciesApi.getAll({ isActive: true, pageSize: 100 })
      .then(r => {
        setCurrencies(r.items)
        const stored = localStorage.getItem(STORAGE_KEY)
        if (!stored || !r.items.some(c => c.id === stored)) {
          const fallback = r.items.find(c => c.code === 'EUR') ?? r.items[0]
          if (fallback) {
            localStorage.setItem(STORAGE_KEY, fallback.id)
            setMainCurrencyIdState(fallback.id)
          }
        }
      })
      .catch(() => {})
  }, [])

  const mainCurrency = currencies.find(c => c.id === mainCurrencyId) ?? null

  const setMainCurrencyId = (id: string) => {
    localStorage.setItem(STORAGE_KEY, id)
    setMainCurrencyIdState(id)
  }

  return (
    <MainCurrencyContext.Provider value={{ currencies, mainCurrency, setMainCurrencyId }}>
      {children}
    </MainCurrencyContext.Provider>
  )
}

export function useMainCurrency() {
  const ctx = useContext(MainCurrencyContext)
  if (!ctx) throw new Error('useMainCurrency must be used within MainCurrencyProvider')
  return ctx
}
