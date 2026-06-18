import { useEffect, useRef, useState } from 'react'
import { authApi, type UserInfo } from '../api/auth'
import { useNotify } from '../NotificationContext'
import { Spinner } from './Spinner'

interface Props {
  onClose: () => void
  onUsernameChange: (username: string) => void
  anchorRef: React.RefObject<HTMLElement | null>
}

export function UserProfileCard({ onClose, onUsernameChange, anchorRef }: Props) {
  const notify = useNotify()
  const cardRef = useRef<HTMLDivElement>(null)

  const [user, setUser] = useState<UserInfo | null>(null)
  const [loading, setLoading] = useState(true)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [profileSaving, setProfileSaving] = useState(false)

  const [currentPw, setCurrentPw] = useState('')
  const [newPw, setNewPw] = useState('')
  const [confirmPw, setConfirmPw] = useState('')
  const [pwSaving, setPwSaving] = useState(false)

  useEffect(() => {
    authApi.me().then(u => {
      setUser(u)
      setUsername(u.username ?? '')
      setEmail(u.email ?? '')
      setPhone(u.phoneNumber ?? '')
    }).catch(() => {}).finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (
        cardRef.current &&
        !cardRef.current.contains(e.target as Node) &&
        anchorRef.current &&
        !anchorRef.current.contains(e.target as Node)
      ) {
        onClose()
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [onClose, anchorRef])

  useEffect(() => {
    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', handleKey)
    return () => window.removeEventListener('keydown', handleKey)
  }, [onClose])

  async function handleSaveProfile() {
    if (!username.trim() || !email.trim()) return
    setProfileSaving(true)
    try {
      const updated = await authApi.updateMe({ username: username.trim(), email: email.trim(), phoneNumber: phone.trim() || undefined })
      setUser(updated)
      onUsernameChange(updated.username)
      notify('Profile updated', 'success')
    } catch {
      notify('Failed to update profile', 'error')
    } finally {
      setProfileSaving(false)
    }
  }

  async function handleChangePassword() {
    if (!currentPw || !newPw) return
    if (newPw !== confirmPw) { notify('New passwords do not match', 'error'); return }
    if (newPw.length < 6) { notify('New password must be at least 6 characters', 'error'); return }
    setPwSaving(true)
    try {
      await authApi.changePassword({ currentPassword: currentPw, newPassword: newPw })
      setCurrentPw('')
      setNewPw('')
      setConfirmPw('')
      notify('Password changed', 'success')
    } catch {
      notify('Current password is incorrect', 'error')
    } finally {
      setPwSaving(false)
    }
  }

  return (
    <div
      ref={cardRef}
      className="absolute right-0 top-full mt-2 w-80 bg-white dark:bg-gray-800 rounded-xl shadow-xl border border-gray-200 dark:border-gray-700 z-50 overflow-hidden"
    >
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-gray-700">
        <span className="text-sm font-semibold text-gray-800 dark:text-gray-100">Account</span>
        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 text-base leading-none"
        >
          ✕
        </button>
      </div>

      {loading ? (
        <div className="px-4 py-8 flex justify-center text-gray-400 dark:text-gray-500"><Spinner /></div>
      ) : (
        <div className="px-4 py-4 space-y-5">

          {/* Avatar + joined date */}
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-indigo-600 flex items-center justify-center text-white text-sm font-semibold select-none shrink-0">
              {username.slice(0, 2).toUpperCase()}
            </div>
            <div>
              <p className="text-sm font-medium text-gray-800 dark:text-gray-100">{user?.username}</p>
              {user?.createdAt && (
                <p className="text-xs text-gray-400 dark:text-gray-500">
                  Member since {new Date(user.createdAt).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })}
                </p>
              )}
            </div>
          </div>

          {/* Profile fields */}
          <div className="space-y-3">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Profile</p>
            <Field label="Username" value={username} onChange={setUsername} />
            <Field label="Email" type="email" value={email} onChange={setEmail} />
            <Field label="Phone" value={phone} onChange={setPhone} placeholder="Optional" />
            <button
              onClick={handleSaveProfile}
              disabled={profileSaving || !username.trim() || !email.trim()}
              className="w-full py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs font-medium transition-colors"
            >
              {profileSaving ? 'Saving…' : 'Save Profile'}
            </button>
          </div>

          {/* Divider */}
          <div className="border-t border-gray-100 dark:border-gray-700" />

          {/* Change password */}
          <div className="space-y-3">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Change Password</p>
            <Field label="Current password" type="password" value={currentPw} onChange={setCurrentPw} />
            <Field label="New password" type="password" value={newPw} onChange={setNewPw} />
            <Field label="Confirm new password" type="password" value={confirmPw} onChange={setConfirmPw} />
            <button
              onClick={handleChangePassword}
              disabled={pwSaving || !currentPw || !newPw || !confirmPw}
              className="w-full py-1.5 rounded-lg bg-gray-700 hover:bg-gray-600 dark:bg-gray-600 dark:hover:bg-gray-500 disabled:opacity-50 text-white text-xs font-medium transition-colors"
            >
              {pwSaving ? 'Changing…' : 'Change Password'}
            </button>
          </div>

        </div>
      )}
    </div>
  )
}

interface FieldProps {
  label: string
  value: string
  onChange: (v: string) => void
  type?: string
  placeholder?: string
}

function Field({ label, value, onChange, type = 'text', placeholder }: FieldProps) {
  return (
    <div>
      <label className="block text-xs text-gray-500 dark:text-gray-400 mb-1">{label}</label>
      <input
        type={type}
        value={value}
        onChange={e => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full px-3 py-1.5 text-sm rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent placeholder-gray-400"
      />
    </div>
  )
}
