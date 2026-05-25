<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4" @click.self="$emit('close')">
      <div class="card w-full max-w-sm p-6">
        <h2 class="text-lg font-bold text-gray-900 mb-1 flex items-center gap-1.5"><AppIcon name="money" size="w-5 h-5" /> Запропонувати ціну</h2>
        <p class="text-sm text-gray-500 mb-4">
          Поточна ціна: <span class="font-semibold text-gray-800">₴{{ currentPrice.toLocaleString() }}</span>
        </p>

        <div class="space-y-3">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Ваша пропозиція (₴)</label>
            <input
              v-model.number="offeredPrice"
              type="number"
              :min="1"
              :max="currentPrice - 1"
              class="input"
              placeholder="Введіть ціну"
            />
            <p v-if="offeredPrice >= currentPrice" class="text-xs text-red-500 mt-1">
              Пропозиція має бути меншою за поточну ціну
            </p>
          </div>

          <div v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</div>

          <div class="flex gap-2">
            <button @click="submit" :disabled="loading || !offeredPrice || offeredPrice <= 0 || offeredPrice >= currentPrice"
              class="btn-primary flex-1">
              <span v-if="loading">...</span>
              <span v-else>Надіслати</span>
            </button>
            <button @click="$emit('close')" class="btn-secondary flex-1">Скасувати</button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { offersApi } from '@/api/offers'
import AppIcon from '@/components/AppIcon.vue'

const props = defineProps<{ adId: string; currentPrice: number }>()
const emit = defineEmits<{ close: []; submitted: [offerId: string] }>()

const offeredPrice = ref(Math.floor(props.currentPrice * 0.9))
const loading = ref(false)
const error = ref('')

async function submit() {
  if (!offeredPrice.value || offeredPrice.value >= props.currentPrice) return
  loading.value = true
  error.value = ''
  try {
    const { data } = await offersApi.makeOffer(props.adId, offeredPrice.value)
    emit('submitted', data.offerId)
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string }; status?: number } }
    if (err.response?.status === 401) {
      error.value = 'Необхідно увійти в систему'
    } else {
      error.value = err.response?.data?.error ?? `Помилка надсилання (${err.response?.status ?? 'network'})`
    }
  } finally {
    loading.value = false
  }
}
</script>
