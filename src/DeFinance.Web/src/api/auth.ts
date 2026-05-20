import client from './client'

export interface LoginRequest { username: string; password: string }
export interface LoginResponse { token: string; username: string }
export interface RegisterRequest { username: string; password: string; confirmPassword: string; email: string }

export interface UserInfo {
  id: string
  username: string
  email: string
  phoneNumber?: string
  createdAt: string
  isActive: boolean
}

export interface UpdateMeRequest { username: string; email: string; phoneNumber?: string }
export interface ChangePasswordRequest { currentPassword: string; newPassword: string }

export const authApi = {
  login: (req: LoginRequest) =>
    client.post<LoginResponse>('/auth/login', req).then(r => r.data),
  register: (req: RegisterRequest) =>
    client.post('/auth/register', req).then(r => r.data),
  me: () =>
    client.get<UserInfo>('/auth/me').then(r => r.data),
  updateMe: (req: UpdateMeRequest) =>
    client.put<UserInfo>('/auth/me', req).then(r => r.data),
  changePassword: (req: ChangePasswordRequest) =>
    client.post('/auth/change-password', req),
}

export const TOKEN_KEY = 'definance_jwt'

export function saveToken(token: string) { localStorage.setItem(TOKEN_KEY, token) }
export function loadToken(): string | null { return localStorage.getItem(TOKEN_KEY) }
export function clearToken() { localStorage.removeItem(TOKEN_KEY) }

export function decodeUsername(token: string): string | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.unique_name ?? payload.sub ?? null
  } catch { return null }
}

export function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.exp * 1000 < Date.now()
  } catch { return true }
}
