<template>
  <div v-if="show"
    class="w-full bg-amber-50 border-b border-amber-200 px-4 py-2.5">
    <div class="max-w-7xl mx-auto flex items-center justify-between gap-3 flex-wrap">
      <p class="text-sm text-amber-800 flex items-center gap-2">
        <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
        </svg>
        Підтвердьте пошту: <strong>{{ auth.email }}</strong>
      </p>
      <button
        @click="resend"
        :disabled="resending || cooldown > 0"
        class="text-xs font-medium text-amber-700 underline hover:text-amber-900
               disabled:no-underline disabled:text-amber-500 transition-colors shrink-0">
        <span v-if="cooldown > 0">Надіслано ({{ cooldown }}с)</span>
        <span v-else-if="resending">Надсилаємо...</span>
        <span v-else>Надіслати знову</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const auth = useAuthStore()

const show = computed(() =>
  auth.isAuthenticated && !auth.isEmailConfirmed)

const resending = ref(false)
const cooldown  = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

async function resend() {
  if (resending.value || cooldown.value > 0) return
  resending.value = true
  try {
    await authApi.resendVerification()
    startCooldown()
  } catch {  }
  finally { resending.value = false }
}

function startCooldown() {
  cooldown.value = 60
  timer = setInterval(() => {
    if (--cooldown.value <= 0 && timer) { clearInterval(timer); timer = null }
  }, 1000)
}

onUnmounted(() => { if (timer) clearInterval(timer) })
</script>
