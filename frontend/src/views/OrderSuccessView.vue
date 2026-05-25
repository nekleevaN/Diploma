<template>
  <div class="max-w-md mx-auto px-4 py-16 text-center">
    <div class="card p-8">
      <template v-if="loading">
        <div class="flex justify-center mb-4">
          <AppIcon name="refresh" size="w-10 h-10 text-gray-300 animate-spin" />
        </div>
        <p class="text-sm text-gray-400">Перевіряємо статус оплати...</p>
      </template>

      <template v-else-if="status === 'hold' || status === 'success'">
        <div class="flex justify-center mb-4">
          <AppIcon name="check-circle" size="w-14 h-14 text-teal-500" />
        </div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Оплата підтверджена!</h1>
        <p class="text-sm text-gray-500 mb-1">
          Кошти заморожено — продавець отримає їх після підтвердження доставки.
        </p>
        <p class="text-sm text-gray-500 mb-6">
          Адресу доставки ми вже маємо — очікуйте відправку від продавця.
        </p>
        <RouterLink to="/orders" class="btn-primary w-full block text-center">
          Мої замовлення
        </RouterLink>
      </template>

      <template v-else-if="status === 'processing' || status === 'created'">
        <div class="flex justify-center mb-4">
          <AppIcon name="clock" size="w-14 h-14 text-yellow-400" />
        </div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Обробляється...</h1>
        <p class="text-sm text-gray-500 mb-6">Очікуємо підтвердження від Monobank. Це займає кілька секунд.</p>
        <button @click="syncStatus" :disabled="syncing"
          class="btn-primary w-full flex items-center justify-center gap-2">
          <AppIcon v-if="syncing" name="refresh" size="w-4 h-4 animate-spin" />
          Оновити статус
        </button>
      </template>

      <template v-else-if="status === 'failure' || status === 'expired'">
        <div class="flex justify-center mb-4">
          <AppIcon name="x-circle" size="w-14 h-14 text-red-400" />
        </div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">
          {{ status === 'expired' ? 'Час оплати вийшов' : 'Оплата не пройшла' }}
        </h1>
        <p class="text-sm text-gray-500 mb-6">Спробуйте ще раз або оберіть інший спосіб оплати.</p>
        <RouterLink to="/" class="btn-secondary w-full block text-center">
          Повернутись до каталогу
        </RouterLink>
      </template>

      <template v-else>
        <div class="flex justify-center mb-4">
          <AppIcon name="check-circle" size="w-14 h-14 text-teal-500" />
        </div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Дякуємо за покупку!</h1>
        <RouterLink to="/orders" class="btn-primary w-full block text-center mt-4">
          Мої замовлення
        </RouterLink>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { paymentApi } from '@/api/payment'
import AppIcon from '@/components/AppIcon.vue'

const route = useRoute()
const orderId = route.params.orderId as string

const loading = ref(true)
const syncing = ref(false)
const status = ref<string>('')

async function syncStatus() {
  syncing.value = true
  try {
    const { data } = await paymentApi.syncStatus(orderId)
    status.value = data.status
  } catch {
    status.value = 'error'
  } finally {
    syncing.value = false
  }
}

onMounted(async () => {
  await syncStatus()
  loading.value = false
})
</script>
