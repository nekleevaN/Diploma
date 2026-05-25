<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-10">
    <div class="w-full max-w-md">
      <div class="card p-8">
        <h1 class="text-2xl font-bold text-gray-900 mb-1">Створити акаунт</h1>
        <p class="text-sm text-gray-500 mb-6">Приєднуйтесь до trustee</p>

        <form @submit.prevent="submit" novalidate class="space-y-4">

          
          <div style="position:absolute;left:-9999px;opacity:0;pointer-events:none" aria-hidden="true">
            <input v-model="honeypot" name="hp_url" type="text" tabindex="-1" autocomplete="new-password" />
          </div>

          
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Ім'я *</label>
              <input
                v-model="form.firstName"
                @blur="onBlur('firstName'); capitalize('firstName')"
                type="text" maxlength="50" placeholder="Анна"
                :class="inputClass('firstName')" />
              <p v-if="touched.firstName && errors.firstName" class="mt-1 text-xs text-red-600">
                {{ errors.firstName }}
              </p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Прізвище *</label>
              <input
                v-model="form.lastName"
                @blur="onBlur('lastName'); capitalize('lastName')"
                type="text" maxlength="50" placeholder="Коваленко"
                :class="inputClass('lastName')" />
              <p v-if="touched.lastName && errors.lastName" class="mt-1 text-xs text-red-600">
                {{ errors.lastName }}
              </p>
            </div>
          </div>

          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email *</label>
            <div class="relative">
              <input
                v-model="form.email"
                @blur="onBlur('email')"
                @input="onEmailInput"
                type="email" placeholder="anna@example.com"
                :class="inputClass('email')" />
              <div class="absolute right-3 top-1/2 -translate-y-1/2">
                <svg v-if="emailChecking" class="w-4 h-4 animate-spin text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                </svg>
                <svg v-else-if="emailAvailable === true && touched.email && !errors.email" class="w-4 h-4 text-teal-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                </svg>
              </div>
            </div>
            <p v-if="touched.email && errors.email" class="mt-1 text-xs text-red-600">{{ errors.email }}</p>
          </div>

          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Пароль *</label>
            <input
              v-model="form.password"
              @blur="onBlur('password')"
              type="password" placeholder="Мін. 8 символів"
              :class="inputClass('password')" />
            
            <div v-if="form.password" class="mt-2">
              <div class="flex gap-1 mb-1">
                <div v-for="i in 3" :key="i"
                  :class="['h-1 flex-1 rounded-full transition-colors duration-300',
                    passwordStrength >= i ? strengthColor : 'bg-gray-200']" />
              </div>
              <p :class="['text-xs', strengthTextColor]">{{ strengthText }}</p>
            </div>
            <p v-if="touched.password && errors.password" class="mt-1 text-xs text-red-600">{{ errors.password }}</p>
          </div>

          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Підтвердження паролю *</label>
            <input
              v-model="form.passwordConfirm"
              @blur="onBlur('passwordConfirm')"
              type="password" placeholder="Повторіть пароль"
              :class="inputClass('passwordConfirm')" />
            <p v-if="touched.passwordConfirm && errors.passwordConfirm" class="mt-1 text-xs text-red-600">
              {{ errors.passwordConfirm }}
            </p>
          </div>

          
          <div class="space-y-2 pt-1">
            <label class="flex items-start gap-2.5 cursor-pointer">
              <input v-model="form.agreeToTerms" @change="onBlur('agreeToTerms')"
                type="checkbox" class="mt-0.5 rounded border-gray-300 shrink-0" />
              <span :class="['text-xs leading-relaxed', touched.agreeToTerms && errors.agreeToTerms ? 'text-red-600' : 'text-gray-600']">
                Я погоджуюсь з
                <a href="#" class="text-teal-600 underline">умовами користування</a>
                та
                <a href="#" class="text-teal-600 underline">політикою конфіденційності</a> *
              </span>
            </label>
            <p v-if="touched.agreeToTerms && errors.agreeToTerms" class="text-xs text-red-600 pl-5">
              {{ errors.agreeToTerms }}
            </p>

            <label class="flex items-start gap-2.5 cursor-pointer">
              <input v-model="form.wantsNewsletter" type="checkbox" class="mt-0.5 rounded border-gray-300 shrink-0" />
              <span class="text-xs text-gray-500 leading-relaxed">
                Хочу отримувати новини та пропозиції trustee
              </span>
            </label>
          </div>

          
          <div v-if="globalError" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">
            {{ globalError }}
          </div>

          
          <button type="submit" :disabled="loading || !canSubmit"
            class="register-btn w-full py-3 rounded-xl text-sm font-semibold text-white
                   transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed
                   flex items-center justify-center gap-2">
            <svg v-if="loading" class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            {{ loading ? 'Створення акаунту...' : 'Створити акаунт' }}
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
          <svg class="w-5 h-5" viewBox="0 0 24 24">
            <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
            <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          Продовжити з Google
        </button>

        <p class="mt-5 text-center text-sm text-gray-500">
          Вже є акаунт?
          <RouterLink to="/login" class="text-teal-600 hover:underline font-medium">Увійти</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  passwordConfirm: '',
  agreeToTerms: false,
  wantsNewsletter: false,
})

const honeypot     = ref('')
const formOpenedAt = Date.now()
const loading      = ref(false)
const googleLoading = ref(false)
const globalError  = ref('')

const touched = reactive<Record<string, boolean>>({})
const submitted = ref(false)

function onBlur(field: string) { touched[field] = true }

function capitalize(field: 'firstName' | 'lastName') {
  if (form[field]) form[field] = form[field].charAt(0).toUpperCase() + form[field].slice(1)
}

const emailChecking  = ref(false)
const emailAvailable = ref<boolean | null>(null)
let emailDebounce: ReturnType<typeof setTimeout> | null = null

function onEmailInput() {
  emailAvailable.value = null
  if (emailDebounce) clearTimeout(emailDebounce)
  if (!form.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) return

  emailDebounce = setTimeout(async () => {
    emailChecking.value = true
    try {
      const { data } = await authApi.checkEmail(form.email)
      emailAvailable.value = data.available
    } catch {  }
    finally { emailChecking.value = false }
  }, 500)
}

const passwordStrength = computed(() => {
  const p = form.password
  if (!p) return 0
  if (p.length < 8) return 1
  const hasLetter  = /[a-zA-Zа-яА-Я]/.test(p)
  const hasDigit   = /\d/.test(p)
  const hasUpper   = /[A-ZА-Я]/.test(p)
  const hasLower   = /[a-zа-я]/.test(p)
  const hasSpecial = /[!@#$%^&*()_+\-=\[\]{}|;':",.<>?/\\`~]/.test(p)

  if (p.length >= 10 && hasLetter && hasDigit && (hasUpper && hasLower || hasSpecial)) return 3
  if (p.length >= 8 && hasLetter && hasDigit) return 2
  return 1
})

const SC: Record<number, string> = { 1: 'bg-red-500', 2: 'bg-yellow-400', 3: 'bg-teal-500' }
const strengthColor = computed(() => SC[passwordStrength.value] ?? 'bg-gray-200')

const STC: Record<number, string> = { 1: 'text-red-600', 2: 'text-yellow-600', 3: 'text-teal-600' }
const strengthTextColor = computed(() => STC[passwordStrength.value] ?? 'text-gray-400')

const strengthText = computed(() => {
  const p = form.password
  if (passwordStrength.value === 3) return '✓ Чудовий пароль!'
  if (passwordStrength.value === 2) return 'Непоганий — додай спецсимвол для надійності'
  if (p.length < 8) return 'Надто короткий — мінімум 8 символів'
  if (!/[a-zA-Z]/.test(p)) return 'Додай літери'
  if (!/\d/.test(p)) return 'Додай цифри для надійності'
  return 'Слабкий пароль'
})

const nameRe = /^[\p{L}\s\-']{2,50}$/u

const errors = computed(() => ({
  firstName:      !nameRe.test(form.firstName.trim())
    ? 'Від 2 до 50 символів, лише літери' : '',
  lastName:       !nameRe.test(form.lastName.trim())
    ? 'Від 2 до 50 символів, лише літери' : '',
  email:          !form.email
    ? 'Email обов\'язковий'
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? 'Некоректний email'
      : emailAvailable.value === false
        ? 'Цей email вже зареєстровано'
        : '',
  password:       form.password.length < 8
    ? 'Мінімум 8 символів'
    : !/[a-zA-Z]/.test(form.password)
      ? 'Додай хоча б одну літеру'
      : !/\d/.test(form.password)
        ? 'Додай хоча б одну цифру'
        : '',
  passwordConfirm: form.password !== form.passwordConfirm
    ? 'Паролі не співпадають' : '',
  agreeToTerms:   !form.agreeToTerms
    ? 'Необхідно погодитись з умовами' : '',
}))

const isValid    = computed(() => Object.values(errors.value).every(e => !e))
const canSubmit  = computed(() => isValid.value && !emailChecking.value)

function inputClass(field: string) {
  const show = touched[field] || submitted.value
  const hasError = !!errors.value[field as keyof typeof errors.value]
  return [
    'input',
    show && hasError ? 'border-red-400 focus:border-red-500 focus:ring-red-400/20' : ''
  ].join(' ')
}

async function submit() {
  submitted.value = true
  Object.keys(errors.value).forEach(f => { touched[f] = true })

  if (!isValid.value) return

  loading.value = true
  globalError.value = ''

  try {
    const result = await auth.register({
      firstName:       form.firstName.trim(),
      lastName:        form.lastName.trim(),
      email:           form.email,
      password:        form.password,
      passwordConfirm: form.passwordConfirm,
      agreeToTerms:    form.agreeToTerms,
      wantsNewsletter: form.wantsNewsletter,
      website:         honeypot.value,
      formOpenedAt,
    })

    sessionStorage.setItem('pendingEmail', form.email)
    router.push({ name: 'verify-email-sent' })
  } catch (e: any) {
    const data = e?.response?.data
    if (e?.response?.status === 409)
      touched['email'] = true
    globalError.value = data?.error ?? 'Помилка реєстрації. Спробуйте ще раз.'
  } finally {
    loading.value = false
  }
}

async function googleSignIn() {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined
  if (!clientId) {
    globalError.value = 'Google OAuth не налаштовано (VITE_GOOGLE_CLIENT_ID відсутній)'
    return
  }

  googleLoading.value = true
  globalError.value   = ''

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
            router.push(data.isNewUser ? '/welcome' : '/')
            resolve()
          } catch (e: any) {
            const d = e?.response?.data
            if (e?.response?.status === 409)
              globalError.value = d?.error ?? 'Email вже зареєстровано через пароль'
            else
              globalError.value = d?.error ?? 'Помилка Google авторизації'
            reject(e)
          }
        }
      });
      (window as any).google.accounts.id.prompt((notification: any) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment())
          reject(new Error('Google popup closed'))
      })
    })
  } catch {  }
  finally { googleLoading.value = false }
}
</script>

<style scoped>
.register-btn { background-color: #708238; }
.register-btn:hover:not(:disabled) { background-color: #5d6c2e; }
</style>
