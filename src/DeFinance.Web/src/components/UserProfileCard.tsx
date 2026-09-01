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

  const [language, setLanguage] = useState<'en' | 'de' | 'fr' | 'uk' | 'es' | 'pl' | 'it' | 'sv' | 'no' | 'fi' | 'el' | 'bg' | 'cs' | 'hr' | 'sk' | 'sl'>(
    () => (localStorage.getItem('lang') as 'en' | 'de' | 'fr' | 'uk' | 'es' | 'pl' | 'it' | 'sv' | 'no' | 'fi' | 'el' | 'bg' | 'cs' | 'hr' | 'sk' | 'sl') ?? 'en'
  )

  function handleLanguageChange(lang: 'en' | 'de' | 'fr' | 'uk' | 'es' | 'pl' | 'it' | 'sv' | 'no' | 'fi' | 'el' | 'bg' | 'cs' | 'hr' | 'sk' | 'sl') {
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
                { code: 'en', label: t('userProfile.lang.english'),    Flag: FlagUK },
                { code: 'de', label: t('userProfile.lang.german'),     Flag: FlagDE },
                { code: 'fr', label: t('userProfile.lang.french'),     Flag: FlagFR },
                { code: 'uk', label: t('userProfile.lang.ukrainian'),  Flag: FlagUA },
                { code: 'es', label: t('userProfile.lang.spanish'),   Flag: FlagES },
                { code: 'pl', label: t('userProfile.lang.polish'),    Flag: FlagPL },
                { code: 'it', label: t('userProfile.lang.italian'),  Flag: FlagIT },
                { code: 'sv', label: t('userProfile.lang.swedish'),  Flag: FlagSE },
                { code: 'no', label: t('userProfile.lang.norwegian'), Flag: FlagNO },
                { code: 'fi', label: t('userProfile.lang.finnish'),   Flag: FlagFI },
                { code: 'el', label: t('userProfile.lang.greek'),      Flag: FlagGR },
                { code: 'bg', label: t('userProfile.lang.bulgarian'),  Flag: FlagBG },
                { code: 'cs', label: t('userProfile.lang.czech'),      Flag: FlagCZ },
                { code: 'hr', label: t('userProfile.lang.croatian'),   Flag: FlagHR },
                { code: 'sk', label: t('userProfile.lang.slovak'),     Flag: FlagSK },
                { code: 'sl', label: t('userProfile.lang.slovenian'),  Flag: FlagSI },
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
                { Flag: FlagRO, label: 'Română (coming soon)' },
                { Flag: FlagHU, label: 'Magyar (coming soon)' },
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

function FlagSE() {
  return (
    <svg viewBox="0 0 16 11" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="16" height="11" fill="#006AA7" />
      <rect x="5" width="2" height="11" fill="#FECC00" />
      <rect y="4" width="16" height="2" fill="#FECC00" />
    </svg>
  )
}

function FlagNO() {
  return (
    <svg viewBox="0 0 22 16" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="22" height="16" fill="#EF2B2D" />
      <rect x="6" width="4" height="16" fill="#fff" />
      <rect y="6" width="22" height="4" fill="#fff" />
      <rect x="7" width="2" height="16" fill="#002868" />
      <rect y="7" width="22" height="2" fill="#002868" />
    </svg>
  )
}

function FlagFI() {
  return (
    <svg viewBox="0 0 18 11" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="18" height="11" fill="#fff" />
      <rect x="5" width="3" height="11" fill="#003580" />
      <rect y="4" width="18" height="3" fill="#003580" />
    </svg>
  )
}

function FlagGR() {
  return (
    <svg viewBox="0 0 27 18" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="27" height="18" fill="#0D5EAF" />
      <rect y="2" width="27" height="2" fill="#fff" />
      <rect y="6" width="27" height="2" fill="#fff" />
      <rect y="10" width="27" height="2" fill="#fff" />
      <rect y="14" width="27" height="2" fill="#fff" />
      <rect width="10" height="10" fill="#0D5EAF" />
      <rect x="4" width="2" height="10" fill="#fff" />
      <rect y="4" width="10" height="2" fill="#fff" />
    </svg>
  )
}

function FlagBG() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="2" fill="#fff" />
      <rect y="0.667" width="3" height="0.667" fill="#00966E" />
      <rect y="1.333" width="3" height="0.667" fill="#D62612" />
    </svg>
  )
}

function FlagCZ() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="1" fill="#fff" />
      <rect y="1" width="3" height="1" fill="#D7141A" />
      <polygon points="0,0 1.2,1 0,2" fill="#11457E" />
    </svg>
  )
}

function FlagSI() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="0.667" fill="#003DA5" />
      <rect y="0.667" width="3" height="0.667" fill="#fff" />
      <rect y="1.333" width="3" height="0.667" fill="#E21B23" />
      <rect x="0.1" y="0.15" width="0.55" height="0.55" fill="#003DA5" rx="0.04" />
      <polygon points="0.375,0.18 0.42,0.32 0.57,0.32 0.45,0.41 0.49,0.56 0.375,0.47 0.26,0.56 0.3,0.41 0.18,0.32 0.33,0.32" fill="#FFDD00" />
    </svg>
  )
}

function FlagHR() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="0.667" fill="#FF0000" />
      <rect y="0.667" width="3" height="0.667" fill="#fff" />
      <rect y="1.333" width="3" height="0.667" fill="#0035AD" />
      <rect x="1.05" y="0.3" width="0.9" height="0.9" fill="#fff" />
      <rect x="1.05" y="0.3" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.2" y="0.3" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.35" y="0.3" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.5" y="0.3" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.65" y="0.3" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.05" y="0.45" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.2" y="0.45" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.35" y="0.45" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.5" y="0.45" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.65" y="0.45" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.05" y="0.6" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.2" y="0.6" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.35" y="0.6" width="0.15" height="0.15" fill="#FF0000" />
      <rect x="1.5" y="0.6" width="0.15" height="0.15" fill="#fff" />
      <rect x="1.65" y="0.6" width="0.15" height="0.15" fill="#FF0000" />
    </svg>
  )
}

function FlagSK() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="0.667" fill="#fff" />
      <rect y="0.667" width="3" height="0.667" fill="#0B4FD8" />
      <rect y="1.333" width="3" height="0.667" fill="#EE1C25" />
      <rect x="0.1" y="0.25" width="0.65" height="1.1" fill="#fff" rx="0.06" />
      <rect x="0.1" y="0.25" width="0.65" height="1.1" fill="#EE1C25" rx="0.06" />
      <rect x="0.2" y="0.35" width="0.45" height="0.6" fill="#fff" />
      <rect x="0.2" y="0.65" width="0.45" height="0.3" fill="#0B4FD8" />
    </svg>
  )
}

function FlagRO() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="1" height="2" fill="#002B7F" />
      <rect x="1" width="1" height="2" fill="#FCD116" />
      <rect x="2" width="1" height="2" fill="#CE1126" />
    </svg>
  )
}

function FlagHU() {
  return (
    <svg viewBox="0 0 3 2" className="w-full h-full" preserveAspectRatio="xMidYMid slice">
      <rect width="3" height="0.667" fill="#CE2939" />
      <rect y="0.667" width="3" height="0.667" fill="#fff" />
      <rect y="1.333" width="3" height="0.667" fill="#477050" />
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
