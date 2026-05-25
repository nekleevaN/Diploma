<template>
  <div>
    
    <div v-if="verified" class="flex items-center gap-3 p-4 bg-teal-150 border border-teal-150 rounded-xl">
      <div class="w-10 h-10 rounded-full bg-teal-150 flex items-center justify-center shrink-0"><AppIcon name="check-circle" size="w-5 h-5" class="text-teal-500" /></div>
      <div>
        <p class="text-sm font-semibold text-teal-600">Особу підтверджено через Дію</p>
        <p class="text-xs text-teal-500 mt-0.5">Ваш профіль отримав бейдж «🇺🇦 Дія»</p>
      </div>
    </div>

    
    <div v-else-if="!sessionId && !confirming" class="p-5 bg-gray-50 border border-gray-200 rounded-xl">
      <div class="flex items-start gap-4">
        <div class="w-12 h-12 rounded-xl bg-teal-500 flex items-center justify-center shrink-0">
          <span class="text-white text-2xl">🇺🇦</span>
        </div>
        <div class="flex-1">
          <h3 class="text-sm font-semibold text-gray-900">Верифікація через Дію</h3>
          <p class="text-xs text-gray-500 mt-1 leading-relaxed">
            Підтвердіть свою особу за допомогою застосунку Дія.
            Після верифікації ваш профіль отримає бейдж надійного продавця.
          </p>
          <div class="flex flex-wrap gap-3 mt-3">
            <div class="flex items-center gap-1.5 text-xs text-gray-500">
              <span class="w-5 h-5 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center text-xs font-bold">1</span>
              Натисни кнопку
            </div>
            <div class="flex items-center gap-1.5 text-xs text-gray-500">
              <span class="w-5 h-5 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center text-xs font-bold">2</span>
              Відскануй QR у Дії
            </div>
            <div class="flex items-center gap-1.5 text-xs text-gray-500">
              <span class="w-5 h-5 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center text-xs font-bold">3</span>
              Отримай бейдж
            </div>
          </div>
          <button @click="start" :disabled="loading"
            class="mt-4 inline-flex items-center gap-2 px-4 py-2 bg-teal-500 text-white text-sm font-medium rounded-lg hover:bg-teal-600 transition-colors disabled:opacity-50">
            <AppIcon v-if="loading" name="refresh" size="w-4 h-4" class="animate-spin" />
            <span v-else>🇺🇦</span>
            <span>{{ loading ? 'Підключення до Дії...' : 'Верифікувати через Дію' }}</span>
          </button>
          <p v-if="error" class="text-xs text-red-500 mt-2">{{ error }}</p>
        </div>
      </div>
    </div>

    
    <div v-else-if="sessionId && !confirming" class="p-5 border border-gray-200 rounded-xl">
      <div class="flex flex-col sm:flex-row gap-5 items-start">
        
        <div class="shrink-0">
          <div class="bg-white p-3 rounded-xl border-2 border-teal-500 inline-block">
            <canvas ref="qrCanvas" class="block" style="width:160px;height:160px" />
          </div>
          <p class="text-xs text-center text-gray-400 mt-1.5">Дійсний {{ expiresIn }}с</p>
        </div>

        
        <div class="flex-1">
          <h3 class="text-sm font-semibold text-gray-900 mb-3">Відскануйте QR-код у застосунку Дія</h3>
          <ol class="space-y-2.5">
            <li class="flex gap-2.5 text-xs text-gray-600">
              <span class="w-5 h-5 rounded-full bg-teal-500 text-white flex items-center justify-center font-bold shrink-0 text-xs">1</span>
              Відкрийте застосунок <strong>Дія</strong> на вашому телефоні
            </li>
            <li class="flex gap-2.5 text-xs text-gray-600">
              <span class="w-5 h-5 rounded-full bg-teal-500 text-white flex items-center justify-center font-bold shrink-0 text-xs">2</span>
              Натисніть <strong>«Сканувати QR-код»</strong> або відкрийте розділ авторизації
            </li>
            <li class="flex gap-2.5 text-xs text-gray-600">
              <span class="w-5 h-5 rounded-full bg-teal-500 text-white flex items-center justify-center font-bold shrink-0 text-xs">3</span>
              Наведіть камеру на QR-код і підтвердіть передачу даних
            </li>
          </ol>

          <div class="mt-4 p-3 bg-teal-50 rounded-lg">
            <p class="text-xs text-teal-700 flex items-center gap-1">
              <AppIcon name="shield" size="w-3.5 h-3.5" /> Дані передаються безпосередньо від Мінцифри. Trustee не зберігає ваші документи.
            </p>
          </div>

          <div class="mt-4 flex flex-col gap-2">
            <button @click="confirm" :disabled="loading"
              class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-teal-500 text-white text-sm font-medium rounded-lg hover:bg-teal-600 transition-colors disabled:opacity-50 w-full sm:w-auto">
              <AppIcon v-if="loading" name="refresh" size="w-4 h-4" class="animate-spin" />
              <AppIcon v-else name="check-circle" size="w-4 h-4" />
              {{ loading ? 'Перевірка...' : 'Я підтвердив(-ла) у застосунку Дія' }}
            </button>
            <button @click="cancel" class="text-xs text-gray-400 hover:text-gray-600 text-center sm:text-left">
              Скасувати
            </button>
          </div>
          <p v-if="error" class="text-xs text-red-500 mt-2">{{ error }}</p>
        </div>
      </div>
    </div>

    
    <div v-else class="p-5 bg-teal-50 border border-teal-200 rounded-xl flex items-center gap-3">
      <AppIcon name="refresh" size="w-5 h-5" class="animate-spin text-teal-600" />
      <p class="text-sm text-teal-700">Перевірка даних у реєстрах Дії...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import QRCode from 'qrcode'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'
import AppIcon from '@/components/AppIcon.vue'

const emit = defineEmits<{ verified: [] }>()

const auth = useAuthStore()
const verified = ref(auth.hasDiia)
const sessionId = ref<string | null>(null)
const loading = ref(false)
const confirming = ref(false)
const error = ref('')
const expiresIn = ref(300)
const qrCanvas = ref<HTMLCanvasElement | null>(null)
let timer: ReturnType<typeof setInterval> | null = null

async function start() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await authApi.startDiia()
    sessionId.value = data.sessionId
    expiresIn.value = 300
    await nextTick()
    await drawQR()
    startTimer()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка підключення до Дії'
  } finally {
    loading.value = false
  }
}

async function drawQR() {
  if (!qrCanvas.value || !sessionId.value) return
  const diiaUrl = `https:
  await QRCode.toCanvas(qrCanvas.value, diiaUrl, {
    width: 160,
    margin: 1,
    color: { dark: '#708238', light: '#ffffff' }
  })
}

function startTimer() {
  timer = setInterval(() => {
    expiresIn.value--
    if (expiresIn.value <= 0) {
      clearInterval(timer!)
      sessionId.value = null
      error.value = 'QR-код прострочено. Спробуйте знову.'
    }
  }, 1000)
}

async function confirm() {
  if (!sessionId.value) return
  loading.value = true
  confirming.value = true
  error.value = ''
  clearInterval(timer!)
  try {
    await authApi.confirmDiia(sessionId.value)
    verified.value = true
    auth.setDiiaVerified()
    sessionId.value = null
    emit('verified')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка верифікації'
    confirming.value = false
  } finally {
    loading.value = false
  }
}

function cancel() {
  clearInterval(timer!)
  sessionId.value = null
  error.value = ''
}

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>
