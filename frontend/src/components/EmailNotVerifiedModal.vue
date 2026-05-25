<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="evStore.showModal"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
        @click.self="evStore.closeModal()">
        <div class="bg-white rounded-2xl p-6 max-w-sm w-full shadow-xl text-center">
          <div class="text-4xl mb-3">📧</div>
          <h2 class="text-base font-bold text-gray-900 mb-2">Підтвердьте email</h2>
          <p class="text-sm text-gray-500 mb-1">
            Щоб виконати цю дію, підтвердьте вашу пошту:
          </p>
          <p class="text-sm font-medium text-gray-800 mb-5">{{ auth.email }}</p>

          <button
            @click="resend"
            :disabled="resending || cooldown > 0"
            class="btn-primary w-full mb-3 flex items-center justify-center gap-2">
            <svg v-if="resending" class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            <span v-if="cooldown > 0">Надіслати знову ({{ cooldown }}с)</span>
            <span v-else-if="resending">Надсилаємо...</span>
            <span v-else-if="sent">✓ Лист надіслано</span>
            <span v-else>Надіслати лист знову</span>
          </button>

          <button @click="evStore.closeModal()"
            class="w-full text-sm text-gray-400 hover:text-gray-600 transition-colors">
            Закрити
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import { useEmailVerificationStore } from '@/stores/emailVerification'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const evStore = useEmailVerificationStore()
const auth    = useAuthStore()

const resending = ref(false)
const sent      = ref(false)
const cooldown  = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

async function resend() {
  if (resending.value || cooldown.value > 0) return
  resending.value = true
  try {
    await authApi.resendVerification()
    sent.value = true
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
