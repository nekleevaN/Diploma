<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-md">
      <div class="card p-8 text-center">

        
        <template v-if="loading">
          <div class="flex justify-center mb-4">
            <svg class="w-10 h-10 animate-spin text-teal-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
          </div>
          <p class="text-sm text-gray-500">Перевіряємо посилання...</p>
        </template>

        
        <template v-else-if="expired">
          <div class="flex justify-center mb-4">
            <div class="w-14 h-14 rounded-full bg-red-50 flex items-center justify-center">
              <svg class="w-7 h-7 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
          </div>
          <h1 class="text-lg font-bold text-gray-900 mb-2">Посилання застаріло</h1>
          <p class="text-sm text-gray-500 mb-5">
            Термін дії посилання (24 год) вичерпано. Запросіть нове.
          </p>
          <button @click="requestNew" :disabled="requesting"
            class="btn-primary w-full flex items-center justify-center gap-2">
            <svg v-if="requesting" class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            {{ requesting ? 'Надсилаємо...' : 'Надіслати нове посилання' }}
          </button>
          <RouterLink to="/login" class="block mt-3 text-sm text-gray-400 hover:text-gray-600">
            Повернутись до входу
          </RouterLink>
        </template>

        
        <template v-else-if="errorMsg">
          <div class="flex justify-center mb-4">
            <div class="w-14 h-14 rounded-full bg-red-50 flex items-center justify-center">
              <svg class="w-7 h-7 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                  d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
          </div>
          <h1 class="text-lg font-bold text-gray-900 mb-2">Помилка підтвердження</h1>
          <p class="text-sm text-gray-500 mb-5">{{ errorMsg }}</p>
          <RouterLink to="/login" class="btn-primary block text-center">Повернутись до входу</RouterLink>
        </template>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const route    = useRoute()
const router   = useRouter()
const auth     = useAuthStore()
const loading  = ref(true)
const expired  = ref(false)
const requesting = ref(false)
const errorMsg = ref('')

onMounted(async () => {
  const token = route.query.token as string
  if (!token) { errorMsg.value = 'Токен відсутній'; loading.value = false; return }

  try {
    const data = await auth.confirmEmail(token)
    router.replace({ name: 'welcome' })
  } catch (e: any) {
    const code = e?.response?.data?.code
    if (code === 'TOKEN_EXPIRED') expired.value = true
    else errorMsg.value = e?.response?.data?.error ?? 'Посилання недійсне'
  } finally {
    loading.value = false
  }
})

async function requestNew() {
  requesting.value = true
  try {
    await authApi.resendVerification()
    router.push({ name: 'verify-email-sent' })
  } catch { requesting.value = false }
}
</script>
