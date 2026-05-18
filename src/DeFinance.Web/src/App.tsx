import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { ThemeProvider } from './ThemeContext'
import { Sidebar } from './components/Sidebar'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { AccountsPage } from './pages/AccountsPage'
import { CategoriesPage } from './pages/CategoriesPage'
import { CurrenciesPage } from './pages/CurrenciesPage'
import { CounterpartiesPage } from './pages/CounterpartiesPage'
import { AdministrationPage } from './pages/AdministrationPage'

function App() {
  const [loggedIn, setLoggedIn] = useState(false)

  return (
    <ThemeProvider>
      {!loggedIn ? (
        <LoginPage onLogin={() => setLoggedIn(true)} />
      ) : (
        <BrowserRouter>
          <div className="flex h-screen bg-gray-50 dark:bg-gray-900">
            <Sidebar />
            <main className="flex-1 overflow-hidden">
              <Routes>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/accounts" element={<AccountsPage />} />
                <Route path="/categories" element={<CategoriesPage />} />
                <Route path="/currencies" element={<CurrenciesPage />} />
                <Route path="/counterparties" element={<CounterpartiesPage />} />
                <Route path="/administration" element={<AdministrationPage />} />
              </Routes>
            </main>
          </div>
        </BrowserRouter>
      )}
    </ThemeProvider>
  )
}

export default App
