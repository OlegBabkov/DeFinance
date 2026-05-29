export function Spinner({ size = 'md' }: { size?: 'sm' | 'md' }) {
  const dot = size === 'sm' ? 'w-1.5 h-1.5' : 'w-2.5 h-2.5'
  const gap  = size === 'sm' ? 'gap-1'       : 'gap-1.5'
  return (
    <div className={`flex items-center ${gap}`}>
      {([0, 150, 300] as const).map((delay, i) => (
        <div
          key={i}
          className={`${dot} rounded-full bg-current animate-bounce`}
          style={{ animationDelay: `${delay}ms` }}
        />
      ))}
    </div>
  )
}
