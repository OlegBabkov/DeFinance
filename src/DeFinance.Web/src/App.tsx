import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { ThemeProvider } from './ThemeContext'
import { NotificationProvider } from './NotificationContext'
import { Notifications } from './components/Notifications'
import { MainCurrencyProvider } from './MainCurrencyContext'
import { Sidebar } from './components/Sidebar'
import { TopBar } from './components/TopBar'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { AccountsPage } from './pages/AccountsPage'
import { CategoriesPage } from './pages/CategoriesPage'
import { CurrenciesPage } from './pages/CurrenciesPage'
import { CounterpartiesPage } from './pages/CounterpartiesPage'
import { AdministrationPage } from './pages/AdministrationPage'
import { TransactionsPage } from './pages/TransactionsPage'
import { clearToken, decodeUsername, isTokenExpired, loadToken, saveToken } from './api/auth'

function getInitialUsername(): string | null {
  const token = loadToken()
  if (!token || isTokenExpired(token)) { clearToken(); return null }
  return decodeUsername(token)
}

function App() {
  const [username, setUsername] = useState<string | null>(getInitialUsername)

  const handleLogin = (name: string, token?: string) => {
    if (token) saveToken(token)
    setUsername(name)
  }

  const handleLogout = () => {
    clearToken()
    setUsername(null)
  }

  return (
    <ThemeProvider>
      <NotificationProvider>
        <Notifications />
        {username === null ? (
          <LoginPage onLogin={name => handleLogin(name)} />
        ) : (
          <MainCurrencyProvider>
          <BrowserRouter>
            <div className="flex h-screen bg-gray-50 dark:bg-gray-900">
              <Sidebar />
              <div className="flex-1 flex flex-col overflow-hidden">
                <TopBar username={username} onLogout={handleLogout} />
                <main className="flex-1 overflow-hidden">
                  <Routes>
                    <Route path="/" element={<DashboardPage />} />
                    <Route path="/accounts" element={<AccountsPage />} />
                    <Route path="/categories" element={<CategoriesPage />} />
                    <Route path="/currencies" element={<CurrenciesPage />} />
                    <Route path="/counterparties" element={<CounterpartiesPage />} />
                    <Route path="/transactions" element={<TransactionsPage />} />
                    <Route path="/administration" element={<AdministrationPage />} />
                  </Routes>
                </main>
              </div>
            </div>
          </BrowserRouter>
          </MainCurrencyProvider>
        )}
      </NotificationProvider>
    </ThemeProvider>
  )
}

export default App
