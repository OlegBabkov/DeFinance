interface Props {
  username: string
}

export function TopBar({ username }: Props) {
  const initials = username.slice(0, 2).toUpperCase()

  return (
    <header className="h-12 shrink-0 flex items-center justify-end px-6 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div className="flex items-center gap-2">
        <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-semibold select-none">
          {initials}
        </div>
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{username}</span>
      </div>
    </header>
  )
}
