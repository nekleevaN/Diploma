<template>
  <div class="max-w-xl mx-auto px-4 py-8">
    <div class="text-center mb-6">
      <div class="flex justify-center mb-3"><AppIcon name="package" size="w-12 h-12" class="text-gray-400" /></div>
      <h1 class="text-xl font-bold text-gray-900">Оформлення доставки</h1>
      <p class="text-sm text-gray-500 mt-1">Оберіть відділення Нової Пошти для отримання товару</p>
    </div>

    <div class="card p-6 space-y-5">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Ваше ім'я (для ТТН)</label>
        <input v-model="form.recipientName" type="text" class="input" placeholder="Прізвище Ім'я По-батькові" required />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Ваш номер телефону</label>
        <input v-model="form.recipientPhone" type="tel" class="input" placeholder="+380501234567" required />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-3">Відділення отримання</label>
        <CityWarehousePicker @selected="onWarehouseSelected" />
      </div>

      <div v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</div>

      <button @click="submit" :disabled="!isReady || saving" class="btn-primary w-full">
        <span v-if="saving">Зберігаємо...</span>
        <span v-else>Підтвердити адресу доставки</span>
      </button>
    </div>

    <p class="text-xs text-center text-gray-400 mt-4">
      Після підтвердження продавець отримає сповіщення і зможе сформувати ТТН
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { deliveryApi } from '@/api/delivery'
import CityWarehousePicker from '@/components/CityWarehousePicker.vue'
import AppIcon from '@/components/AppIcon.vue'

const route = useRoute()
const router = useRouter()
const orderId = route.params.orderId as string

const form = ref({ recipientName: '', recipientPhone: '' })
const selectedAddress = ref<{ cityRef: string; cityName: string; warehouseRef: string; warehouseAddress: string } | null>(null)
const saving = ref(false)
const error = ref('')

const isReady = computed(() =>
  !!selectedAddress.value &&
  form.value.recipientName.trim().length > 2 &&
  form.value.recipientPhone.trim().length >= 10)

function onWarehouseSelected(addr: typeof selectedAddress.value) {
  selectedAddress.value = addr
}

async function submit() {
  if (!selectedAddress.value) return
  saving.value = true
  error.value = ''
  try {
    await deliveryApi.setRecipientAddress(orderId, {
      ...selectedAddress.value,
      recipientName: form.value.recipientName,
      recipientPhone: form.value.recipientPhone
    })
    router.push(`/orders`)
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка збереження адреси'
  } finally {
    saving.value = false
  }
}
</script>
