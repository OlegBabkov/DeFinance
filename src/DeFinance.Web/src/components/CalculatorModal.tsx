import { useState } from 'react'

interface Props {
  onApply: (value: string) => void
  onClose: () => void
}

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

export function CalculatorModal({ onApply, onClose }: Props) {
  const [expr, setExpr] = useState('')

  const result = evaluate(expr)
  const valid = result !== null

  const push = (s: string) => setExpr(e => e + s)
  const clear = () => setExpr('')
  const back = () => setExpr(e => e.slice(0, -1))

  const apply = () => {
    if (!valid) return
    onApply(parseFloat(result!.toFixed(2)).toString())
    onClose()
  }

  const base = 'h-10 w-full rounded-lg text-sm font-medium transition-colors focus:outline-none'
  const num = `${base} bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-600`
  const op  = `${base} bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-100 dark:hover:bg-indigo-900/50`
  const del = `${base} bg-red-50 dark:bg-red-900/20 text-red-500 dark:text-red-400 hover:bg-red-100 dark:hover:bg-red-900/40`
  const apl = `${base} bg-indigo-600 hover:bg-indigo-700 text-white disabled:opacity-40 col-span-3`

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-4 w-72">
        <div className="flex items-center justify-between mb-3">
          <span className="text-sm font-semibold text-gray-900 dark:text-gray-100">Calculator</span>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 leading-none">✕</button>
        </div>

        <input
          type="text"
          value={expr}
          onChange={e => setExpr(e.target.value.replace(/[^0-9+\-*/().\s]/g, ''))}
          onKeyDown={e => { if (e.key === 'Enter') apply(); if (e.key === 'Escape') onClose() }}
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
          <button className={apl} disabled={!valid} onClick={apply}>
            Apply {valid ? `= ${result!.toFixed(2)}` : ''}
          </button>
        </div>
      </div>
    </div>
  )
}
