import { useRef, useState } from 'react'
import { UserProfileCard } from './UserProfileCard'

interface Props {
  username: string
  onLogout: () => void
  onUsernameChange: (username: string) => void
}

export function TopBar({ username, onLogout, onUsernameChange }: Props) {
  const initials = username.slice(0, 2).toUpperCase()
  const [cardOpen, setCardOpen] = useState(false)
  const avatarRef = useRef<HTMLButtonElement>(null)

  return (
    <header className="h-[62px] shrink-0 flex items-center justify-end px-6 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div className="flex items-center gap-3">
        <div className="relative">
          <button
            ref={avatarRef}
            onClick={() => setCardOpen(o => !o)}
            className="flex items-center gap-2 rounded-lg px-2 py-1 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-semibold select-none">
              {initials}
            </div>
            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{username}</span>
          </button>

          {cardOpen && (
            <UserProfileCard
              onClose={() => setCardOpen(false)}
              onUsernameChange={onUsernameChange}
              anchorRef={avatarRef}
            />
          )}
        </div>

        <button
          onClick={onLogout}
          className="text-xs text-gray-400 hover:text-red-500 dark:hover:text-red-400 transition-colors px-2 py-1 rounded hover:bg-gray-100 dark:hover:bg-gray-700"
        >
          Sign out
        </button>
      </div>
    </header>
  )
}
