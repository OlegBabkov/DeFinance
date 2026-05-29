import { useState, useRef, useEffect } from 'react'

const EMOJI_GROUPS: { label: string; emojis: string[] }[] = [
  { label: 'Finance', emojis: ['💰','💳','💵','💸','🏦','📈','📉','💹','🪙','💲','🏧','💼','📊','📋','🧾'] },
  { label: 'Food & Drink', emojis: ['🍔','🍕','🍣','🍜','🍱','🥗','🥩','🍺','🍷','☕','🧃','🥤','🍰','🧁','🥐','🛒'] },
  { label: 'Transport', emojis: ['🚗','🚕','🚌','🚂','✈️','⛽','🚲','🛵','🛳️','🚙','🚘','🏎️','🛺','🚐'] },
  { label: 'Shopping', emojis: ['👕','👗','👠','🕶️','🎁','💄','🧴','🏪','🛍️','🧸','👟','👜','🧣','🪞'] },
  { label: 'Home', emojis: ['🏠','🏡','🛋️','🔧','💡','🛁','🧹','🔑','🪴','🪟','🛏️','🚿','🧰','🪛'] },
  { label: 'Health', emojis: ['💊','🏥','🏋️','🧘','🩺','💉','🦷','🧪','❤️','🏃','🩹','🧬','🫀','💪'] },
  { label: 'Entertainment', emojis: ['🎬','🎮','📚','🎵','🎭','🎨','🏆','🎲','🎯','🎸','🎤','🎧','🎻','🏅'] },
  { label: 'Work', emojis: ['💻','🖥️','📱','✏️','📝','📞','🖨️','⌨️','🗂️','📎','🖇️','📐','📏','🗓️'] },
  { label: 'Education', emojis: ['🎓','📖','🏫','✏️','🔬','🔭','🗺️','🧮','📐','🖊️','📓','📒','📔'] },
  { label: 'Misc', emojis: ['⚡','🔔','📌','⏰','🌐','⚙️','🔒','🌿','🌺','☀️','🌙','🌈','🔥','💧','🎀','🎉','🪄','🌟'] },
]

interface Props {
  value: string
  onChange: (emoji: string) => void
}

export function EmojiPicker({ value, onChange }: Props) {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const select = (emoji: string) => {
    onChange(emoji)
    setOpen(false)
  }

  return (
    <div ref={ref} className="relative inline-flex items-center gap-2">
      <button
        type="button"
        onClick={() => setOpen(o => !o)}
        className="flex items-center justify-center w-10 h-10 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 text-xl transition-colors"
        title="Pick emoji"
      >
        {value || '😀'}
      </button>
      {value && (
        <button
          type="button"
          onClick={() => onChange('')}
          className="text-xs text-gray-400 hover:text-red-500 transition-colors"
          title="Clear"
        >
          ✕
        </button>
      )}
      {open && (
        <div className="absolute top-full left-0 mt-1 z-50 w-72 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl p-3 overflow-y-auto max-h-80">
          {EMOJI_GROUPS.map(group => (
            <div key={group.label} className="mb-3 last:mb-0">
              <div className="text-xs font-medium text-gray-400 dark:text-gray-500 mb-1 uppercase tracking-wide">
                {group.label}
              </div>
              <div className="grid grid-cols-8 gap-0.5">
                {group.emojis.map(emoji => (
                  <button
                    key={emoji}
                    type="button"
                    onClick={() => select(emoji)}
                    className={`flex items-center justify-center h-8 w-8 rounded text-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700 ${
                      value === emoji ? 'bg-indigo-100 dark:bg-indigo-900/50 ring-1 ring-indigo-400' : ''
                    }`}
                  >
                    {emoji}
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
