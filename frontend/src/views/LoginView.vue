<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-sm">
      <div class="card p-8">
        <h1 class="text-2xl font-bold text-gray-900 mb-1">Вхід</h1>
        <p class="text-sm text-gray-500 mb-6">Введіть дані вашого акаунту</p>

        <form @submit.prevent="submit" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input v-model="form.email" type="email" class="input" placeholder="you@example.com" required />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Пароль</label>
            <input v-model="form.password" type="password" class="input" placeholder="••••••••" required />
            <div class="flex justify-end mt-1">
              <RouterLink to="/forgot-password" class="text-xs text-teal-600 hover:underline">
                Забули пароль?
              </RouterLink>
            </div>
          </div>

          
          <div v-if="emailNotVerified"
            class="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2.5 space-y-1.5">
            <p class="text-sm text-amber-800">Підтвердьте email перед входом</p>
            <button type="button" @click="resendVerification"
              :disabled="resendLoading || resendCooldown > 0"
              class="text-xs text-teal-600 underline hover:text-teal-800 disabled:text-gray-400 disabled:no-underline">
              <span v-if="resendCooldown > 0">Надіслати знову ({{ resendCooldown }}с)</span>
              <span v-else-if="resendLoading">Надсилаємо...</span>
              <span v-else-if="resendDone">✓ Лист надіслано</span>
              <span v-else>Надіслати лист підтвердження</span>
            </button>
          </div>

          <div v-else-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</div>

          <button type="submit" class="btn-primary w-full" :disabled="loading">
            <span v-if="loading">Входимо...</span>
            <span v-else>Увійти</span>
          </button>
        </form>

        
        <div class="flex items-center gap-3 my-5">
          <div class="flex-1 h-px bg-ivory-400" />
          <span class="text-xs text-gray-400">або</span>
          <div class="flex-1 h-px bg-ivory-400" />
        </div>

        
        <button type="button" @click="googleSignIn" :disabled="googleLoading"
          class="w-full flex items-center justify-center gap-3 py-2.5 border border-gray-200
                 rounded-xl text-sm font-medium text-gray-700 bg-white hover:bg-gray-50
                 transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed">
          <svg v-if="!googleLoading" class="w-5 h-5" viewBox="0 0 24 24">
            <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
            <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          <svg v-else class="w-4 h-4 animate-spin text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
          </svg>
          Продовжити з Google
        </button>

        <p class="mt-5 text-center text-sm text-gray-500">
          Немає акаунту?
          <RouterLink to="/register" class="text-teal-600 hover:underline font-medium">Зареєструватися</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const auth   = useAuthStore()
const router = useRouter()
const route  = useRoute()

const form    = ref({ email: '', password: '' })
const loading = ref(false)
const error   = ref('')

const emailNotVerified = ref(false)
const resendLoading    = ref(false)
const resendDone       = ref(false)
const resendCooldown   = ref(0)
let cooldownTimer: ReturnType<typeof setInterval> | null = null

async function submit() {
  error.value = ''
  emailNotVerified.value = false
  loading.value = true
  try {
    await auth.login(form.value.email, form.value.password)
    const redirect = route.query.redirect as string | undefined
    router.push(redirect ?? '/')
  } catch (e: any) {
    const code = e?.response?.data?.code
    if (code === 'EMAIL_NOT_VERIFIED') {
      emailNotVerified.value = true
    } else {
      error.value = e?.response?.data?.error ?? 'Помилка входу'
    }
  } finally {
    loading.value = false
  }
}

async function resendVerification() {
  resendLoading.value = true
  resendDone.value    = false
  try {
    await authApi.resendVerification()
    resendDone.value = true
    startCooldown()
  } catch {
    sessionStorage.setItem('pendingEmail', form.value.email)
    router.push({ name: 'verify-email-sent' })
  } finally {
    resendLoading.value = false
  }
}

function startCooldown() {
  resendCooldown.value = 60
  cooldownTimer = setInterval(() => {
    if (--resendCooldown.value <= 0 && cooldownTimer) {
      clearInterval(cooldownTimer)
      cooldownTimer = null
    }
  }, 1000)
}

const googleLoading = ref(false)

async function googleSignIn() {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined
  if (!clientId) { error.value = 'Google OAuth не налаштовано'; return }

  googleLoading.value = true
  error.value = ''

  try {
    if (!(window as any).google) {
      await new Promise<void>((resolve, reject) => {
        const s = document.createElement('script')
        s.src = 'https://accounts.google.com/gsi/client'
        s.onload = () => resolve()
        s.onerror = reject
        document.head.appendChild(s)
      })
    }

    await new Promise<void>((resolve, reject) => {
      (window as any).google.accounts.id.initialize({
        client_id: clientId,
        callback: async (resp: { credential: string }) => {
          try {
            const { data } = await authApi.googleAuth(resp.credential)
            auth.setAuth(data.token, data.userId)
            const redirect = route.query.redirect as string | undefined
            router.push(redirect ?? (data.isNewUser ? '/welcome' : '/'))
            resolve()
          } catch (e: any) {
            const d = e?.response?.data
            if (e?.response?.status === 409)
              error.value = d?.error ?? 'Email зареєстровано через пароль. Увійдіть звичайним способом.'
            else
              error.value = d?.error ?? 'Помилка Google авторизації'
            reject(e)
          }
        }
      });
      (window as any).google.accounts.id.prompt((n: any) => {
        if (n.isNotDisplayed() || n.isSkippedMoment()) reject(new Error('closed'))
      })
    })
  } catch {  }
  finally { googleLoading.value = false }
}

onUnmounted(() => { if (cooldownTimer) clearInterval(cooldownTimer) })
</script>
