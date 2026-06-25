import { useRef, useState } from 'react'
import { UserProfileCard } from './UserProfileCard'

interface Props {
  username: string
  onLogout: () => void
  onUsernameChange: (username: string) => void
  photoUrl: string | null
  onPhotoChange: (url: string | null) => void
  onOpenCalculator: () => void
}

export function TopBar({ username, onLogout, onUsernameChange, photoUrl, onPhotoChange, onOpenCalculator }: Props) {
  const initials = username.slice(0, 2).toUpperCase()
  const [cardOpen, setCardOpen] = useState(false)
  const avatarRef = useRef<HTMLButtonElement>(null)

  return (
    <header className="h-[62px] shrink-0 flex items-center justify-end px-6 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div className="flex items-center gap-3">
        <button
          onClick={onOpenCalculator}
          title="Calculator"
          className="text-gray-400 hover:text-indigo-500 dark:hover:text-indigo-400 transition-colors px-2 py-1 rounded hover:bg-gray-100 dark:hover:bg-gray-700"
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
            <rect x="4" y="2" width="16" height="20" rx="2"/>
            <line x1="8" y1="6" x2="16" y2="6"/>
            <line x1="8" y1="10" x2="8" y2="10"/><line x1="12" y1="10" x2="12" y2="10"/><line x1="16" y1="10" x2="16" y2="10"/>
            <line x1="8" y1="14" x2="8" y2="14"/><line x1="12" y1="14" x2="12" y2="14"/><line x1="16" y1="14" x2="16" y2="14"/>
            <line x1="8" y1="18" x2="8" y2="18"/><line x1="12" y1="18" x2="12" y2="18"/><line x1="16" y1="18" x2="16" y2="18"/>
          </svg>
        </button>
        <div className="relative">
          <button
            ref={avatarRef}
            onClick={() => setCardOpen(o => !o)}
            className="flex items-center gap-2 rounded-lg px-2 py-1 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            <div className="w-7 h-7 rounded-full overflow-hidden bg-indigo-600 flex items-center justify-center text-white text-xs font-semibold select-none shrink-0">
              {photoUrl
                ? <img src={photoUrl} alt={username} className="w-full h-full object-cover" />
                : initials}
            </div>
            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{username}</span>
          </button>

          {cardOpen && (
            <UserProfileCard
              onClose={() => setCardOpen(false)}
              onUsernameChange={onUsernameChange}
              onPhotoChange={onPhotoChange}
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
