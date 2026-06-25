import { useEffect, useRef, useState } from 'react'

function evaluate(expr: string): number | null {
  const sanitized = expr.replace(/[^0-9+\-*/().\s]/g, '').trim()
  if (!sanitized) return null
  try {
    // eslint-disable-next-line no-new-func
    const result = new Function(`"use strict"; return (${sanitized})`)()
    if (typeof result === 'number' && isFinite(result)) return +result.toFixed(10)
    return null
  } catch {
    return null
  }
}

// Keypad shared by both variants
function Keypad({ expr, setExpr, onClose, onEquals }: {
  expr: string
  setExpr: (e: string | ((prev: string) => string)) => void
  onClose: () => void
  onEquals?: () => void
}) {
  const result = evaluate(expr)
  const valid = result !== null

  const push = (s: string) => setExpr(e => e + s)
  const clear = () => setExpr('')
  const back = () => setExpr(e => e.slice(0, -1))

  const base = 'h-10 w-full rounded-lg text-sm font-medium transition-colors focus:outline-none'
  const num = `${base} bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-600`
  const op  = `${base} bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-100 dark:hover:bg-indigo-900/50`
  const del = `${base} bg-red-50 dark:bg-red-900/20 text-red-500 dark:text-red-400 hover:bg-red-100 dark:hover:bg-red-900/40`

  return (
    <>
      <input
        type="text"
        value={expr}
        onChange={e => setExpr(e.target.value.replace(/[^0-9+\-*/().\s]/g, ''))}
        onKeyDown={e => { if (e.key === 'Enter' && onEquals) onEquals(); if (e.key === 'Escape') onClose() }}
        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 text-right text-base font-mono text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 mb-1"
        placeholder="0"
        autoFocus
      />
      <div className={`text-right text-xs font-mono px-1 mb-3 h-4 ${valid ? 'text-indigo-500 dark:text-indigo-400' : 'text-transparent'}`}>
        = {valid ? result!.toFixed(2) : '0'}
      </div>

      <div className="grid grid-cols-4 gap-1.5">
        <button className={num} onClick={() => push('7')}>7</button>
        <button className={num} onClick={() => push('8')}>8</button>
        <button className={num} onClick={() => push('9')}>9</button>
        <button className={op}  onClick={() => push('/')}>÷</button>

        <button className={num} onClick={() => push('4')}>4</button>
        <button className={num} onClick={() => push('5')}>5</button>
        <button className={num} onClick={() => push('6')}>6</button>
        <button className={op}  onClick={() => push('*')}>×</button>

        <button className={num} onClick={() => push('1')}>1</button>
        <button className={num} onClick={() => push('2')}>2</button>
        <button className={num} onClick={() => push('3')}>3</button>
        <button className={op}  onClick={() => push('-')}>−</button>

        <button className={num} onClick={() => push('0')}>0</button>
        <button className={num} onClick={() => push('.')}>.</button>
        <button className={del} onClick={back}>⌫</button>
        <button className={op}  onClick={() => push('+')}>+</button>

        <button className={del} onClick={clear}>C</button>
        {onEquals
          ? <button className={`${base} bg-indigo-600 hover:bg-indigo-700 text-white disabled:opacity-40 col-span-3`} disabled={!valid} onClick={onEquals}>=</button>
          : <div className="col-span-3" />
        }
      </div>
    </>
  )
}

// Modal variant — blocks background, has Apply callback
interface ModalProps {
  onApply: (value: string) => void
  onClose: () => void
}

export function CalculatorModal({ onApply, onClose }: ModalProps) {
  const [expr, setExpr] = useState('')
  const result = evaluate(expr)
  const valid = result !== null

  const apply = () => {
    if (!valid) return
    onApply(parseFloat(result!.toFixed(2)).toString())
    onClose()
  }

  const base = 'h-10 w-full rounded-lg text-sm font-medium transition-colors focus:outline-none'

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-4 w-72">
        <div className="flex items-center justify-between mb-3">
          <span className="text-sm font-semibold text-gray-900 dark:text-gray-100">Calculator</span>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 leading-none">✕</button>
        </div>
        <Keypad expr={expr} setExpr={setExpr} onClose={onClose} />
        <button
          className={`${base} bg-indigo-600 hover:bg-indigo-700 text-white disabled:opacity-40 col-span-3 mt-1.5 w-full`}
          disabled={!valid}
          onClick={apply}
        >
          Apply {valid ? `= ${result!.toFixed(2)}` : ''}
        </button>
      </div>
    </div>
  )
}

// Floating variant — draggable, non-blocking, no Apply
interface FloatingProps {
  onClose: () => void
}

export function FloatingCalculator({ onClose }: FloatingProps) {
  const [expr, setExpr] = useState('')
  const [pos, setPos] = useState({ x: window.innerWidth - 284, y: 80 })
  const dragging = useRef(false)
  const dragOffset = useRef({ x: 0, y: 0 })

  const result = evaluate(expr)
  const valid = result !== null

  const equals = () => {
    if (valid) setExpr(parseFloat(result!.toFixed(10)).toString())
  }

  const onMouseDown = (e: React.MouseEvent) => {
    dragging.current = true
    dragOffset.current = { x: e.clientX - pos.x, y: e.clientY - pos.y }
  }

  useEffect(() => {
    const onMove = (e: MouseEvent) => {
      if (!dragging.current) return
      setPos({ x: e.clientX - dragOffset.current.x, y: e.clientY - dragOffset.current.y })
    }
    const onUp = () => { dragging.current = false }
    document.addEventListener('mousemove', onMove)
    document.addEventListener('mouseup', onUp)
    return () => {
      document.removeEventListener('mousemove', onMove)
      document.removeEventListener('mouseup', onUp)
    }
  }, [])

  return (
    <div
      className="fixed z-[70] bg-white dark:bg-gray-800 rounded-xl shadow-2xl border border-gray-200 dark:border-gray-700 w-64 select-none"
      style={{ left: pos.x, top: pos.y }}
    >
      <div
        className="flex items-center justify-between px-3 py-2 border-b border-gray-200 dark:border-gray-700 cursor-grab active:cursor-grabbing"
        onMouseDown={onMouseDown}
      >
        <span className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide">Calculator</span>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 text-sm leading-none">✕</button>
      </div>
      <div className="p-3">
        <Keypad expr={expr} setExpr={setExpr} onClose={onClose} onEquals={equals} />
      </div>
    </div>
  )
}
