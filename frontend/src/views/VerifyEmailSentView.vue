<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-md">
      <div class="card p-8 text-center">
        
        <div class="flex justify-center mb-5">
          <div class="w-16 h-16 rounded-full bg-teal-50 flex items-center justify-center">
            <svg class="w-8 h-8 text-teal-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
            </svg>
          </div>
        </div>

        <h1 class="text-xl font-bold text-gray-900 mb-2">Перевірте пошту</h1>
        <p class="text-sm text-gray-500 mb-1">
          Ми надіслали лист на
        </p>
        <p class="text-sm font-semibold text-gray-800 mb-6">{{ email }}</p>
        <p class="text-xs text-gray-400 mb-6 leading-relaxed">
          Відкрийте лист і натисніть кнопку «Підтвердити email».<br>
          Перевірте папку «Спам», якщо лист не прийшов.
        </p>

        
        <div class="mb-4">
          <button v-if="!resendLoading && cooldown === 0"
            @click="resend"
            class="text-sm text-teal-600 hover:text-teal-700 hover:underline">
            Не отримали? Надіслати знову
          </button>
          <div v-else-if="cooldown > 0" class="text-sm text-gray-400">
            Повторний запит через {{ cooldown }}с
          </div>
          <div v-else class="text-sm text-gray-400 flex items-center justify-center gap-1.5">
            <svg class="w-3.5 h-3.5 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            Надсилаємо...
          </div>
          <p v-if="resendSuccess" class="text-xs text-teal-600 mt-2">✓ Лист надіслано повторно</p>
          <p v-if="resendError" class="text-xs text-red-600 mt-2">{{ resendError }}</p>
        </div>

        <div class="border-t border-ivory-400 pt-4">
          <RouterLink to="/register" class="text-sm text-gray-400 hover:text-gray-600">
            ← Змінити email
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'

const auth  = useAuthStore()
const email = ref(sessionStorage.getItem('pendingEmail') ?? auth.email ?? '')

const resendLoading = ref(false)
const resendSuccess = ref(false)
const resendError   = ref('')
const cooldown      = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

async function resend() {
  resendLoading.value = true
  resendSuccess.value = false
  resendError.value   = ''
  try {
    await authApi.resendVerification()
    resendSuccess.value = true
    startCooldown()
  } catch (e: any) {
    const msg = e?.response?.data?.error ?? 'Помилка'
    resendError.value = msg.startsWith('RATE_LIMIT:') ? msg.slice(11) : msg
  } finally {
    resendLoading.value = false
  }
}

function startCooldown() {
  cooldown.value = 60
  timer = setInterval(() => {
    if (--cooldown.value <= 0 && timer) clearInterval(timer)
  }, 1000)
}

onUnmounted(() => { if (timer) clearInterval(timer) })
</script>
