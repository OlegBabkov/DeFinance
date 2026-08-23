import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import i18n from '../i18n'
import { authApi, type UserInfo } from '../api/auth'
import { useNotify } from '../NotificationContext'
import { Spinner } from './Spinner'

const ACCEPTED_PHOTO_TYPES = 'image/jpeg,image/png,image/webp,image/gif'

interface Props {
  onClose: () => void
  onUsernameChange: (username: string) => void
  onPhotoChange: (url: string | null) => void
  anchorRef: React.RefObject<HTMLElement | null>
}

export function UserProfileCard({ onClose, onUsernameChange, onPhotoChange, anchorRef }: Props) {
  const { t } = useTranslation()
  const notify = useNotify()
  const cardRef = useRef<HTMLDivElement>(null)

  const [user, setUser] = useState<UserInfo | null>(null)
  const [loading, setLoading] = useState(true)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [profileSaving, setProfileSaving] = useState(false)

  const [language, setLanguage] = useState<'en' | 'de' | 'fr'>(
    () => (localStorage.getItem('lang') as 'en' | 'de' | 'fr') ?? 'en'
  )

  function handleLanguageChange(lang: 'en' | 'de' | 'fr') {
    setLanguage(lang)
    localStorage.setItem('lang', lang)
    i18n.changeLanguage(lang)
  }

  const [currentPw, setCurrentPw] = useState('')
  const [newPw, setNewPw] = useState('')
  const [confirmPw, setConfirmPw] = useState('')
  const [pwSaving, setPwSaving] = useState(false)
  const [photoSaving, setPhotoSaving] = useState(false)
  const photoInputRef = useRef<HTMLInputElement>(null)

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
      notify(t('userProfile.notify.profileUpdated'), 'success')
    } catch {
      notify(t('userProfile.notify.profileUpdateFailed'), 'error')
    } finally {
      setProfileSaving(false)
    }
  }

  async function handlePhotoChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setPhotoSaving(true)
    try {
      const updated = await authApi.uploadPhoto(file)
      setUser(updated)
      onPhotoChange(updated.photoUrl ?? null)
      notify(t('userProfile.notify.photoUpdated'), 'success')
    } catch {
      notify(t('userProfile.notify.photoUploadFailed'), 'error')
    } finally {
      setPhotoSaving(false)
      if (photoInputRef.current) photoInputRef.current.value = ''
    }
  }

  async function handleRemovePhoto() {
    setPhotoSaving(true)
    try {
      await authApi.deletePhoto()
      setUser(u => u ? { ...u, photoUrl: undefined } : u)
      onPhotoChange(null)
      notify(t('userProfile.notify.photoRemoved'), 'success')
    } catch {
      notify(t('userProfile.notify.photoRemoveFailed'), 'error')
    } finally {
      setPhotoSaving(false)
    }
  }

  async function handleChangePassword() {
    if (!currentPw || !newPw) return
    if (newPw !== confirmPw) { notify(t('userProfile.notify.passwordsDoNotMatch'), 'error'); return }
    if (newPw.length < 6) { notify(t('userProfile.notify.passwordTooShort'), 'error'); return }
    setPwSaving(true)
    try {
      await authApi.changePassword({ currentPassword: currentPw, newPassword: newPw })
      setCurrentPw('')
      setNewPw('')
      setConfirmPw('')
      notify(t('userProfile.notify.passwordChanged'), 'success')
    } catch {
      notify(t('userProfile.notify.currentPasswordIncorrect'), 'error')
    } finally {
      setPwSaving(false)
    }
  }

  return (
    <div
      ref={cardRef}
      className="absolute right-0 top-full mt-2 w-80 max-h-[calc(100vh-80px)] flex flex-col bg-white dark:bg-gray-800 rounded-xl shadow-xl border border-gray-200 dark:border-gray-700 z-50 overflow-hidden"
    >
      {/* Header — always visible */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-gray-700 shrink-0">
        <span className="text-sm font-semibold text-gray-800 dark:text-gray-100">{t('userProfile.title')}</span>
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
        <div className="px-4 py-4 space-y-5 overflow-y-auto">

          {/* Avatar + joined date */}
          <div className="flex items-center gap-3">
            <div className="relative group shrink-0">
              <div className="w-14 h-14 rounded-full overflow-hidden bg-indigo-600 flex items-center justify-center text-white text-lg font-semibold select-none">
                {user?.photoUrl
                  ? <img src={user.photoUrl} alt={username} className="w-full h-full object-cover" />
                  : username.slice(0, 2).toUpperCase()}
              </div>
              <button
                onClick={() => photoInputRef.current?.click()}
                disabled={photoSaving}
                title={t('userProfile.changePhoto')}
                className="absolute inset-0 rounded-full bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white text-xs transition-opacity disabled:opacity-30"
              >
                {photoSaving ? <Spinner /> : '📷'}
              </button>
              <input
                ref={photoInputRef}
                type="file"
                accept={ACCEPTED_PHOTO_TYPES}
                className="hidden"
                onChange={handlePhotoChange}
              />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-800 dark:text-gray-100">{user?.username}</p>
              {user?.createdAt && (
                <p className="text-xs text-gray-400 dark:text-gray-500">
                  {t('userProfile.memberSince')} {new Date(user.createdAt).toLocaleDateString(i18n.language, { month: 'short', year: 'numeric' })}
                </p>
              )}
              {user?.photoUrl && (
                <button
                  onClick={handleRemovePhoto}
                  disabled={photoSaving}
                  className="text-xs text-red-400 hover:text-red-500 disabled:opacity-50 mt-0.5"
                >
                  {t('userProfile.removePhoto')}
                </button>
              )}
            </div>
          </div>

          {/* Profile fields */}
          <div className="space-y-3">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">{t('userProfile.section.profile')}</p>

            {/* Language selector */}
            <div className="flex flex-wrap items-center gap-2">
              {([
                { code: 'en', label: t('userProfile.lang.english'), Flag: FlagUK },
                { code: 'de', label: t('userProfile.lang.german'),  Flag: FlagDE },
                { code: 'fr', label: t('userProfile.lang.french'),  Flag: FlagFR },
              ] as const).map(({ code, Flag, label }) => (
                <button
                  key={code}
                  onClick={() => handleLanguageChange(code)}
                  title={label}
                  className={`w-8 h-8 rounded-full overflow-hidden shrink-0 transition-all border-2
                    ${language === code
                      ? 'border-indigo-500 shadow-md shadow-indigo-500/40'
                      : 'border-transparent opacity-45 hover:opacity-75'
                    }`}
                >
                  <Flag />
                </button>
              ))}
              {([
                { Flag: FlagES, label: 'Español (coming soon)' },
                { Flag: FlagPL, label: 'Polski (coming soon)' },
                { Flag: FlagUA, label: 'Українська (coming soon)' },
                { Flag: FlagIT, label: 'Italiano (coming soon)' },
              ]).map(({ Flag, label }) => (
                <button
                  key={label}
                  disabled
                  title={label}
                  className="w-8 h-8 rounded-full overflow-hidden shrink-0 border-2 border-transparent opacity-30 cursor-not-allowed"
                >
                  <Flag />
                </button>
              ))}
            </div>

            <Field label={t('userProfile.field.username')} value={username} onChange={setUsername} />
            <Field label={t('userProfile.field.email')} type="email" value={email} onChange={setEmail} />
            <Field label={t('userProfile.field.phone')} value={phone} onChange={setPhone} placeholder={t('userProfile.field.phonePlaceholder')} />
            <button
              onClick={handleSaveProfile}
              disabled={profileSaving || !username.trim() || !email.trim()}
              className="w-full py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs font-medium transition-colors"
            >
              {profileSaving ? t('userProfile.button.savingProfile') : t('userProfile.button.saveProfile')}
            </button>
          </div>

          {/* Divider */}
          <div className="border-t border-gray-100 dark:border-gray-700" />

          {/* Change password */}
          <div className="space-y-3">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">{t('userProfile.section.changePassword')}</p>
            <Field label={t('userProfile.field.currentPassword')} type="password" value={currentPw} onChange={setCurrentPw} />
            <Field label={t('userProfile.field.newPassword')} type="password" value={newPw} onChange={setNewPw} />
            <Field label={t('userProfile.field.confirmNewPassword')} type="password" value={confirmPw} onChange={setConfirmPw} />
            <button
              onClick={handleChangePassword}
              disabled={pwSaving || !currentPw || !newPw || !confirmPw}
              className="w-full py-1.5 rounded-lg bg-gray-700 hover:bg-gray-600 dark:bg-gray-600 dark:hover:bg-gray-500 disabled:opacity-50 text-white text-xs font-medium transition-colors"
            >
              {pwSaving ? t('userProfile.button.changingPassword') : t('userProfile.button.changePassword')}
            </button>
          </div>

        </div>
      )}
    </div>
  )
}

function FlagUK() {
  return (
    <svg viewBox="0 0 60 30" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="60" height="30" fill="#012169" />
      <path d="M0,0 L60,30 M60,0 L0,30" stroke="#fff" strokeWidth="6" />
      <path d="M0,0 L60,30 M60,0 L0,30" stroke="#C8102E" strokeWidth="4" />
      <path d="M30,0 V30 M0,15 H60" stroke="#fff" strokeWidth="10" />
      <path d="M30,0 V30 M0,15 H60" stroke="#C8102E" strokeWidth="6" />
    </svg>
  )
}

function FlagDE() {
  return (
    <svg viewBox="0 0 5 3" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="5" height="1" fill="#000" />
      <rect width="5" height="1" y="1" fill="#D00" />
      <rect width="5" height="1" y="2" fill="#FFCE00" />
    </svg>
  )
}

function FlagFR() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="1" height="2" fill="#002395" />
      <rect width="1" height="2" x="1" fill="#FFF" />
      <rect width="1" height="2" x="2" fill="#ED2939" />
    </svg>
  )
}

function FlagES() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="2" fill="#AA151B" />
      <rect width="3" height="1" y="0.5" fill="#F1BF00" />
    </svg>
  )
}

function FlagPL() {
  return (
    <svg viewBox="0 0 8 5" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="8" height="2.5" fill="#FFF" />
      <rect width="8" height="2.5" y="2.5" fill="#DC143C" />
    </svg>
  )
}

function FlagUA() {
  return (
    <svg viewBox="0 0 2 1" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="2" height="0.5" fill="#005BBB" />
      <rect width="2" height="0.5" y="0.5" fill="#FFD500" />
    </svg>
  )
}

function FlagIT() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="1" height="2" fill="#009246" />
      <rect width="1" height="2" x="1" fill="#FFF" />
      <rect width="1" height="2" x="2" fill="#CE2B37" />
    </svg>
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
