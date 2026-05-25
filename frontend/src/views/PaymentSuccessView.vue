<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4">
    <div class="text-center max-w-sm">

      
      <template v-if="checking">
        <div class="flex justify-center mb-4 animate-spin"><AppIcon name="refresh" size="w-12 h-12" class="text-gray-400" /></div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Перевіряємо оплату...</h1>
        <p class="text-gray-500 text-sm">Отримуємо підтвердження від Monobank</p>
      </template>

      
      <template v-else-if="status === 'hold' || status === 'success'">
        <div class="flex justify-center mb-4"><AppIcon name="check-circle" size="w-14 h-14" class="text-teal-500" /></div>
        <h1 class="text-2xl font-bold text-gray-900 mb-2">Оплата підтверджена!</h1>
        <template v-if="hasDelivery">
          <p class="text-gray-500 text-sm mb-2">Кошти заморожено — продавець отримає їх після підтвердження доставки.</p>
          <p class="text-gray-600 text-sm font-medium mb-6">Вкажіть відділення Нової Пошти для отримання товару.</p>
          <div class="space-y-3">
            <RouterLink v-if="orderId" :to="`/delivery/${orderId}`" class="btn-primary w-full block text-center flex items-center justify-center gap-1.5">
              <AppIcon name="package" size="w-4 h-4" /> Вказати відділення НП
            </RouterLink>
            <RouterLink to="/orders" class="btn-secondary w-full block text-center">
              Пізніше
            </RouterLink>
          </div>
        </template>
        <template v-else>
          <p class="text-gray-500 text-sm mb-6">Товар передано при зустрічі. Кошти будуть перераховані продавцю автоматично.</p>
          <RouterLink to="/orders" class="btn-primary w-full block text-center">
            Перейти до замовлень
          </RouterLink>
        </template>
      </template>

      
      <template v-else-if="status === 'processing' || status === 'created'">
        <div class="flex justify-center mb-4"><AppIcon name="clock" size="w-12 h-12" class="text-gray-400" /></div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Обробляється...</h1>
        <p class="text-gray-500 text-sm mb-4">Monobank обробляє платіж. Зазвичай це займає декілька секунд.</p>
        <button @click="checkStatus" :disabled="checking" class="btn-secondary w-full flex items-center justify-center gap-1.5">
          <AppIcon name="refresh" size="w-4 h-4" /> Перевірити ще раз
        </button>
      </template>

      
      <template v-else-if="status === 'failure' || status === 'expired'">
        <div class="flex justify-center mb-4"><AppIcon name="x-circle" size="w-12 h-12" class="text-red-500" /></div>
        <h1 class="text-xl font-bold text-gray-900 mb-2">Оплата не пройшла</h1>
        <p class="text-gray-500 text-sm mb-4">
          {{ status === 'expired' ? 'Час оплати вийшов.' : 'Сталася помилка при обробці платежу.' }}
        </p>
        <RouterLink to="/" class="btn-secondary w-full block text-center">Повернутись до оголошень</RouterLink>
      </template>

      
      <template v-else>
        <div class="flex justify-center mb-4"><AppIcon name="check-circle" size="w-14 h-14" class="text-teal-500" /></div>
        <h1 class="text-2xl font-bold text-gray-900 mb-2">Повертаємось...</h1>
        <RouterLink to="/orders" class="btn-primary w-full block text-center mt-4">Мої замовлення</RouterLink>
      </template>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { paymentApi } from '@/api/payment'
import AppIcon from '@/components/AppIcon.vue'

const route = useRoute()
const orderId = computed(() => route.query.orderId as string | undefined)
const hasDelivery = computed(() => route.query.hasDelivery !== 'false')
const checking = ref(false)
const status = ref<string | null>(null)

async function checkStatus() {
  if (!orderId.value) return
  checking.value = true
  try {
    const { data } = await paymentApi.syncStatus(orderId.value)
    status.value = data.status
  } catch {
    status.value = 'error'
  } finally {
    checking.value = false
  }
}

onMounted(() => {
  if (orderId.value) {
    checkStatus()
  }
})
</script>
