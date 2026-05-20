import { useContext } from 'react'
import { NotificationContext, type NotificationType } from '../NotificationContext'

const cls: Record<NotificationType, string> = {
  success: 'bg-green-500/80 dark:bg-green-600/80',
  info:    'bg-blue-500/80 dark:bg-blue-600/80',
  error:   'bg-red-500/80 dark:bg-red-600/80',
}

export function Notifications() {
  const ctx = useContext(NotificationContext)
  if (!ctx || ctx.notifications.length === 0) return null
  const { notifications, dismiss } = ctx

  return (
    <div className="fixed top-4 right-4 z-50 flex flex-col gap-2 pointer-events-none">
      {notifications.map(n => (
        <div
          key={n.id}
          className={`${cls[n.type]} text-white px-4 py-3 rounded-lg shadow-lg backdrop-blur-sm flex items-center gap-3 pointer-events-auto min-w-[240px] max-w-sm`}
        >
          <span className="text-sm font-medium flex-1">{n.message}</span>
          <button
            onClick={() => dismiss(n.id)}
            className="text-white/70 hover:text-white text-lg leading-none shrink-0"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  )
}
