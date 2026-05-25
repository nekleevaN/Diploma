import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'

function parseJwt(token: string) {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(base64).split('').map(c => '%' + c.charCodeAt(0).toString(16).padStart(2, '0')).join('')
    )
    return JSON.parse(json)
  } catch { return null }
}

export const useAuthStore = defineStore('auth', () => {
  const token               = ref<string | null>(localStorage.getItem('token'))
  const userId              = ref<string | null>(localStorage.getItem('userId'))
  const username            = ref<string | null>(localStorage.getItem('username'))
  const firstName           = ref<string | null>(localStorage.getItem('firstName'))
  const lastName            = ref<string | null>(localStorage.getItem('lastName'))
  const displayName         = ref<string | null>(localStorage.getItem('displayName'))
  const email               = ref<string | null>(localStorage.getItem('email'))
  const isEmailConfirmed    = ref<boolean>(localStorage.getItem('emailConfirmed') === 'true')
  const authProvider        = ref<string | null>(localStorage.getItem('authProvider'))
  const hasDiia             = ref<boolean>(localStorage.getItem('hasDiia') === 'true')
  const avatarUrl           = ref<string | null>(localStorage.getItem('avatarUrl'))
  const trustedContactTelegramId = ref<number | null>(
    localStorage.getItem('trustedTg') ? Number(localStorage.getItem('trustedTg')) : null
  )
  const trustedContactEmail = ref<string | null>(localStorage.getItem('trustedEmail'))
  const isPayoutEnabled = ref<boolean>(localStorage.getItem('payoutEnabled') === 'true')

  const isAuthenticated = computed(() => !!token.value)

  function setAuth(newToken: string, newUserId: string) {
    token.value  = newToken
    userId.value = newUserId

    const p = parseJwt(newToken)
    username.value         = p?.username ?? null
    firstName.value        = p?.first_name ?? null
    lastName.value         = p?.last_name ?? null
    displayName.value      = p?.display_name ?? null
    email.value            = p?.email ?? null
    isEmailConfirmed.value = p?.email_confirmed === 'true'
    authProvider.value     = p?.auth_provider ?? 'email'

    const badges: string[] = Array.isArray(p?.badge)
      ? p.badge : p?.badge ? [p.badge] : []
    hasDiia.value = badges.includes('DiiaVerified')

    const tgId = p?.trusted_telegram_id ? Number(p.trusted_telegram_id) : null
    trustedContactTelegramId.value = tgId
    trustedContactEmail.value = p?.trusted_email ?? null

    localStorage.setItem('token',         newToken)
    localStorage.setItem('userId',        newUserId)
    localStorage.setItem('username',      username.value ?? '')
    localStorage.setItem('firstName',     firstName.value ?? '')
    localStorage.setItem('lastName',      lastName.value ?? '')
    localStorage.setItem('displayName',   displayName.value ?? '')
    localStorage.setItem('email',         email.value ?? '')
    localStorage.setItem('emailConfirmed',String(isEmailConfirmed.value))
    localStorage.setItem('authProvider',  authProvider.value ?? 'email')
    localStorage.setItem('hasDiia',       String(hasDiia.value))
    if (tgId) localStorage.setItem('trustedTg', String(tgId))
    else localStorage.removeItem('trustedTg')
    if (trustedContactEmail.value) localStorage.setItem('trustedEmail', trustedContactEmail.value)
    else localStorage.removeItem('trustedEmail')
  }


  async function login(emailVal: string, password: string) {
    const { data } = await authApi.login(emailVal, password)
    setAuth(data.token, data.userId)
  }

  async function register(data: Parameters<typeof authApi.register>[0]) {
    const res = await authApi.register(data)
    const { token: jwt, userId: uid, message } = res.data
    setAuth(jwt, uid)
    return { userId: uid, message }
  }

  async function confirmEmail(token: string) {
    const { data } = await authApi.verifyEmail(token)
    setAuth(data.token, data.userId)
    return data
  }

  function markEmailConfirmed() {
    isEmailConfirmed.value = true
    localStorage.setItem('emailConfirmed', 'true')
  }

  function updateAvatar(url: string) {
    avatarUrl.value = url
    localStorage.setItem('avatarUrl', url)
  }

  function setTrustedTelegram(chatId: number | null) {
    trustedContactTelegramId.value = chatId
    if (chatId) localStorage.setItem('trustedTg', String(chatId))
    else localStorage.removeItem('trustedTg')
  }

  function setTrustedEmail(email: string | null) {
    trustedContactEmail.value = email
    if (email) localStorage.setItem('trustedEmail', email)
    else localStorage.removeItem('trustedEmail')
  }

  function setPayoutEnabled(enabled: boolean) {
    isPayoutEnabled.value = enabled
    localStorage.setItem('payoutEnabled', String(enabled))
  }

  function setDiiaVerified() {
    hasDiia.value = true
    localStorage.setItem('hasDiia', 'true')
  }

  function logout() {
    token.value = userId.value = username.value = null
    firstName.value = lastName.value = displayName.value = null
    email.value = authProvider.value = avatarUrl.value = null
    isEmailConfirmed.value = hasDiia.value = false
    ;[
      'token','userId','username','firstName','lastName','displayName',
      'email','emailConfirmed','authProvider','hasDiia','avatarUrl','trustedTg','trustedEmail'
    ].forEach(k => localStorage.removeItem(k))
  }

  return {
    token, userId, username, firstName, lastName, displayName,
    email, isEmailConfirmed, authProvider,
    hasDiia, avatarUrl, trustedContactTelegramId, trustedContactEmail, isPayoutEnabled,
    isAuthenticated,
    setAuth, login, register, confirmEmail, markEmailConfirmed,
    logout, setDiiaVerified, updateAvatar, setTrustedTelegram, setTrustedEmail, setPayoutEnabled,
  }
})
