import { createContext, useCallback, useContext, useRef, useState } from 'react'

export type NotificationType = 'success' | 'info' | 'error'

export interface AppNotification {
  id: number
  message: string
  type: NotificationType
}

interface NotificationContextValue {
  notifications: AppNotification[]
  notify: (message: string, type: NotificationType) => void
  dismiss: (id: number) => void
}

export const NotificationContext = createContext<NotificationContextValue | null>(null)

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const [notifications, setNotifications] = useState<AppNotification[]>([])
  const nextId = useRef(0)

  const dismiss = useCallback((id: number) => {
    setNotifications(ns => ns.filter(n => n.id !== id))
  }, [])

  const notify = useCallback((message: string, type: NotificationType) => {
    const id = nextId.current++
    setNotifications(ns => [...ns, { id, message, type }])
    setTimeout(() => dismiss(id), 4000)
  }, [dismiss])

  return (
    <NotificationContext.Provider value={{ notifications, notify, dismiss }}>
      {children}
    </NotificationContext.Provider>
  )
}

export function useNotify() {
  const ctx = useContext(NotificationContext)
  if (!ctx) throw new Error('useNotify must be used within NotificationProvider')
  return ctx.notify
}
