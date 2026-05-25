<template>
  <div class="max-w-2xl mx-auto px-4 py-8">
    <ReviewModal
      v-if="activeReview"
      :review-id="activeReview.reviewId"
      :review-type="activeReview.reviewType"
      :ad-title="activeReview.adTitle"
      @close="activeReview = null"
      @submitted="onReviewSubmitted" />

    <h1 class="text-xl font-bold text-gray-900 mb-2">Замовлення</h1>

    <div class="flex border-b border-gray-200 mb-6">
      <button v-for="tab in tabs" :key="tab.id"
        @click="activeTab = tab.id"
        :class="['px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
          activeTab === tab.id ? 'border-teal-500 text-teal-600' : 'border-transparent text-gray-500 hover:text-gray-700']">
        {{ tab.label }}
        <span v-if="tab.id === 'seller' && pendingCount > 0"
          class="ml-1.5 bg-orange-500 text-white text-xs rounded-full px-1.5 py-0.5">{{ pendingCount }}</span>
      </button>
    </div>

    <div v-if="loading" class="space-y-3">
      <div v-for="i in 3" :key="i" class="card p-4 animate-pulse space-y-2">
        <div class="h-4 bg-gray-200 rounded w-3/4" />
        <div class="h-3 bg-gray-200 rounded w-1/2" />
      </div>
    </div>

    <div v-else-if="orders.length === 0" class="text-center py-12 text-gray-400">
      <div class="flex justify-center mb-2"><AppIcon name="package" size="w-8 h-8" /></div>
      <p class="text-sm">{{ activeTab === 'buyer' ? 'Ви ще нічого не купували' : 'Активних замовлень немає' }}</p>
    </div>

    <div v-else class="space-y-3">
      <div v-for="order in orders" :key="order.orderId" class="card p-4">
        <div class="flex items-start justify-between gap-3">
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold text-gray-900 truncate">{{ order.adTitle }}</p>
            <p class="text-base font-bold text-gray-900 mt-0.5">₴{{ order.amount.toLocaleString() }}</p>
            <p class="text-xs text-gray-400 mt-1">{{ formatDate(order.createdAt) }}</p>
          </div>
          <span :class="['text-xs font-medium px-2.5 py-1 rounded-full shrink-0', statusClass(order.status)]">
            {{ statusLabel(order.status) }}
          </span>
        </div>

        
        <div v-if="activeTab === 'seller' && order.status === 'Hold'" class="mt-3 pt-3 border-t border-gray-100 space-y-3">
          <p class="text-xs text-orange-700 flex items-center gap-1">
            <AppIcon name="money" size="w-3.5 h-3.5" /> Кошти заморожено. Вкажіть своє відділення НП і згенеруйте ТТН.
          </p>

          
          <div v-if="deliveries[order.orderId]" class="bg-teal-50 rounded-lg p-3 space-y-1">
            <p class="text-xs font-medium text-teal-700 flex items-center gap-1"><AppIcon name="location" size="w-3.5 h-3.5" /> Адреса покупця:</p>
            <p class="text-xs text-blue-800">{{ deliveries[order.orderId].recipientCityName }}, {{ deliveries[order.orderId].recipientWarehouseAddress }}</p>
            <p class="text-xs text-teal-600">{{ deliveries[order.orderId].recipientName }}, {{ deliveries[order.orderId].recipientPhone }}</p>
          </div>
          <div v-else class="text-xs text-gray-400">Покупець ще не вказав адресу доставки</div>

          
          <div v-if="deliveries[order.orderId]?.ttn" class="space-y-2">
            <div class="bg-teal-150 border border-teal-150 rounded-lg p-3">
              <p class="text-xs font-medium text-teal-500">ТТН згенеровано:</p>
              <p class="text-lg font-bold text-green-900 font-mono mt-0.5">{{ deliveries[order.orderId].ttn }}</p>
              <p class="text-xs text-teal-500 mt-1">
                <AppIcon name="check-circle" size="w-3.5 h-3.5" class="inline mr-0.5" />Принесіть товар у відділення НП і продиктуйте цей номер.<br>
                Система автоматично відстежить посилку і зарахує кошти після отримання.
              </p>
            </div>
            
            <DeliveryProgress :status="deliveries[order.orderId].status" :description="deliveries[order.orderId].trackingStatusDescription" />
          </div>

          
          <div v-else-if="deliveries[order.orderId]?.recipientWarehouseAddress">
            <div v-if="senderForms[order.orderId]" class="space-y-2">
              <CityWarehousePicker @selected="(a) => senderAddress[order.orderId] = a" />
              <input v-model="senderForms[order.orderId].name" type="text" class="input text-sm" placeholder="Ваше ПІБ" />
              <input v-model="senderForms[order.orderId].phone" type="tel" class="input text-sm" placeholder="+380..." />
              <button @click="saveSenderAndGenerateTTN(order.orderId)" :disabled="processing === order.orderId"
                class="w-full py-2 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600 disabled:opacity-50">
                <template v-if="processing === order.orderId">...</template>
              <template v-else><AppIcon name="package" size="w-3.5 h-3.5" class="inline mr-1" />Згенерувати ТТН</template>
              </button>
              <button @click="delete senderForms[order.orderId]" class="text-xs text-gray-400 hover:text-gray-600">Скасувати</button>
            </div>
            <button v-else @click="senderForms[order.orderId] = { name: '', phone: '' }"
              class="w-full py-1.5 px-3 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600 flex items-center justify-center gap-1">
              <AppIcon name="package" size="w-3.5 h-3.5" /> Вказати відділення відправки і згенерувати ТТН
            </button>
          </div>

          
          <div v-if="!deliveries[order.orderId]?.ttn" class="pt-1">
            <button @click="cancel(order.orderId)" :disabled="processing === order.orderId"
              class="text-xs text-red-500 hover:text-red-700">Скасувати замовлення</button>
          </div>
          <p v-if="syncMessages[order.orderId]" :class="['text-xs mt-2', syncMessageClass(syncMessages[order.orderId])]">
            {{ syncMessageText(syncMessages[order.orderId]) }}
          </p>
        </div>

        
        <div v-if="activeTab === 'buyer' && order.hasDelivery && (order.status === 'Hold' || order.status === 'Completed' || order.status === 'AwaitingConfirmation')"
          class="mt-3 pt-3 border-t border-gray-100">
          <div v-if="!deliveries[order.orderId]">
            <RouterLink :to="`/delivery/${order.orderId}`" class="text-xs text-teal-600 hover:underline flex items-center gap-1">
              <AppIcon name="package" size="w-3.5 h-3.5" /> Вказати відділення для отримання →
            </RouterLink>
          </div>
          <div v-else>
            <div v-if="deliveries[order.orderId].ttn" class="space-y-2">
              <div class="bg-teal-50 rounded-lg p-3">
                <p class="text-xs font-medium text-teal-700">ТТН:</p>
                <p class="text-base font-bold text-blue-900 font-mono">{{ deliveries[order.orderId].ttn }}</p>
              </div>
              <DeliveryProgress :status="deliveries[order.orderId].status" :description="deliveries[order.orderId].trackingStatusDescription" />
            </div>
            <div v-else class="text-xs text-gray-400">
              Очікуємо формування ТТН від продавця...
            </div>
          </div>
        </div>

        
        <div v-if="activeTab === 'buyer' && order.status === 'AwaitingConfirmation'"
          class="mt-3 pt-3 border-t border-gray-100 space-y-2">
          <p class="text-xs font-medium text-gray-700 flex items-center gap-1">
            <AppIcon name="check-circle" size="w-3.5 h-3.5" class="text-teal-500" /> Посилку отримано. Підтвердіть отримання або запросіть відшкодування.
          </p>
          <div class="flex gap-2">
            <button @click="confirmReceipt(order.orderId)" :disabled="processing === order.orderId"
              class="flex-1 py-2 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600 disabled:opacity-50 flex items-center justify-center gap-1">
              <AppIcon name="check-circle" size="w-3.5 h-3.5" />
              Товар отримано
            </button>
            <button @click="refundOrder(order.orderId)" :disabled="processing === order.orderId"
              class="flex-1 py-2 bg-red-50 text-red-600 border border-red-200 text-xs font-medium rounded-lg hover:bg-red-100 disabled:opacity-50 flex items-center justify-center gap-1">
              <AppIcon name="money" size="w-3.5 h-3.5" />
              Відшкодувати кошти
            </button>
          </div>
          <p v-if="syncMessages[order.orderId]" :class="['text-xs mt-1', syncMessageClass(syncMessages[order.orderId])]">
            {{ syncMessageText(syncMessages[order.orderId]) }}
          </p>
        </div>

        
        <div v-if="activeTab === 'buyer' && order.status === 'Pending'" class="mt-3 pt-3 border-t border-gray-100 space-y-2">
          <p class="text-xs text-gray-400">Очікується підтвердження оплати від Monobank</p>
          <button @click="cancel(order.orderId)" :disabled="processing === order.orderId"
            class="text-xs text-red-500 hover:text-red-700">Скасувати замовлення</button>
        </div>

        <div v-if="order.status === 'Completed'" class="mt-2 space-y-2">
          <div class="flex items-center gap-1.5 text-xs text-teal-500">
            <AppIcon name="check-circle" size="w-3.5 h-3.5" /> Угода завершена, кошти перераховані продавцю
          </div>
          
          <div v-if="reviewedOrderIds.has(order.orderId)"
            class="text-xs text-gray-400 flex items-center gap-1.5">
            <AppIcon name="check-circle" size="w-3.5 h-3.5" class="text-teal-400" /> Відгук залишено
          </div>
          
          <button v-else
            @click="openReview(order)"
            :disabled="initingReview === order.orderId"
            class="text-xs text-teal-600 hover:text-teal-800 border border-teal-300 rounded-lg px-3 py-1.5 flex items-center gap-1.5 transition-colors bg-teal-50 hover:bg-teal-100 disabled:opacity-50">
            <svg v-if="initingReview !== order.orderId" class="w-3.5 h-3.5 text-teal-500" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
            </svg>
            <svg v-else class="w-3.5 h-3.5 animate-spin text-teal-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            {{ initingReview === order.orderId ? 'Завантаження...' : 'Залишити відгук' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, reactive } from 'vue'
import { paymentApi, type OrderDto } from '@/api/payment'
import { deliveryApi, type DeliveryDto } from '@/api/delivery'
import { reviewsApi, type PendingReviewDto } from '@/api/reviews'
import { useAuthStore } from '@/stores/auth'
import CityWarehousePicker from '@/components/CityWarehousePicker.vue'
import DeliveryProgress from '@/components/DeliveryProgress.vue'
import ReviewModal from '@/components/ReviewModal.vue'
import AppIcon from '@/components/AppIcon.vue'

const activeTab = ref<'buyer' | 'seller'>('seller')
const tabs = [
  { id: 'seller' as const, label: 'Замовлення мені' },
  { id: 'buyer' as const, label: 'Мої купівлі' }
]

const orders = ref<OrderDto[]>([])
const loading = ref(false)
const processing = ref<string | null>(null)
const syncMessages = reactive<Record<string, string>>({})

const pendingReviews = reactive<Record<string, PendingReviewDto>>({})
const reviewedOrderIds = reactive<Set<string>>(new Set())
const activeReview = ref<{ orderId: string; adTitle: string; reviewId: string; reviewType: string } | null>(null)
const initingReview = ref<string | null>(null)
const auth = useAuthStore()

async function loadReviewState() {
  try {
    const [pending, submitted] = await Promise.all([
      reviewsApi.getMyPending(),
      reviewsApi.getMySubmitted(),
    ])
    for (const r of pending.data) {
      pendingReviews[r.orderId] = r
    }
    for (const orderId of submitted.data) {
      reviewedOrderIds.add(orderId)
    }
  } catch {  }
}

async function openReview(order: OrderDto) {
  const existing = pendingReviews[order.orderId]
  if (existing) {
    activeReview.value = {
      orderId: order.orderId,
      adTitle: order.adTitle,
      reviewId: existing.reviewId,
      reviewType: existing.reviewType,
    }
    return
  }

  initingReview.value = order.orderId
  try {
    const isBuyer = order.buyerId === auth.userId
    const { data } = await reviewsApi.initOrderReviews(order.orderId, {
      buyerId:    order.buyerId,
      sellerId:   order.sellerId,
      buyerName:  isBuyer ? (auth.displayName || auth.firstName || 'Покупець') : 'Покупець',
      sellerName: !isBuyer ? (auth.displayName || auth.firstName || 'Продавець') : 'Продавець',
    })
    const reviewType = isBuyer ? 'BuyerToSeller' : 'SellerToBuyer'
    activeReview.value = {
      orderId:    order.orderId,
      adTitle:    order.adTitle,
      reviewId:   data.reviewId,
      reviewType,
    }
  } catch (e: any) {
    alert(e?.response?.data?.error ?? 'Не вдалося відкрити форму відгуку')
  } finally {
    initingReview.value = null
  }
}

function onReviewSubmitted() {
  if (activeReview.value) {
    delete pendingReviews[activeReview.value.orderId]
    reviewedOrderIds.add(activeReview.value.orderId)
  }
  activeReview.value = null
}

const pendingCount = computed(() =>
  orders.value.filter(o => o.status === 'Hold').length)

function statusLabel(s: string) {
  return {
    Pending: 'Очікує оплати', Hold: 'Оплачено (HOLD)',
    Completed: 'Завершено', Cancelled: 'Скасовано',
    Refunded: 'Повернено', Failed: 'Помилка', Expired: 'Прострочено',
    AwaitingConfirmation: 'Очікує підтвердження'
  }[s] ?? s
}

function syncMessageClass(msg: string) {
  if (msg.startsWith('ok:')) return 'text-teal-500'
  if (msg.startsWith('warn:')) return 'text-orange-500'
  return 'text-red-500'
}

function syncMessageText(msg: string) {
  const colon = msg.indexOf(':')
  return colon >= 0 ? msg.slice(colon + 1) : msg
}

function statusClass(s: string) {
  return {
    Pending: 'bg-yellow-100 text-yellow-700',
    Hold: 'bg-orange-100 text-orange-700',
    Completed: 'bg-teal-150 text-teal-700 border border-teal-300',
    Cancelled: 'bg-gray-100 text-gray-500',
    Refunded: 'bg-teal-50 text-teal-700',
    Failed: 'bg-red-100 text-red-700',
    Expired: 'bg-gray-100 text-gray-500',
    AwaitingConfirmation: 'bg-blue-100 text-blue-700'
  }[s] ?? 'bg-gray-100 text-gray-700'
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })
}

const deliveries = reactive<Record<string, DeliveryDto>>({})
const senderForms = reactive<Record<string, { name: string; phone: string }>>({})
const senderAddress = reactive<Record<string, { cityRef: string; cityName: string; warehouseRef: string; warehouseAddress: string } | null>>({})

async function load() {
  loading.value = true
  try {
    const res = activeTab.value === 'seller'
      ? await paymentApi.getMySellerOrders()
      : await paymentApi.getMyBuyerOrders()
    orders.value = res.data
    await refreshDeliveries(res.data)
  } finally {
    loading.value = false
  }
}

async function silentRefresh() {
  try {
    const res = activeTab.value === 'seller'
      ? await paymentApi.getMySellerOrders()
      : await paymentApi.getMyBuyerOrders()

    for (const fresh of res.data) {
      const existing = orders.value.find(o => o.orderId === fresh.orderId)
      if (existing && existing.status !== fresh.status) {
        Object.assign(existing, fresh)
      }
    }

    await refreshDeliveries(res.data)
  } catch {  }
}

async function refreshDeliveries(orderList: typeof orders.value) {
  for (const order of orderList.filter(o => o.status === 'Hold' || o.status === 'Completed' || o.status === 'AwaitingConfirmation')) {
    try {
      const d = await deliveryApi.getDelivery(order.orderId)
      if (deliveries[order.orderId]) {
        Object.assign(deliveries[order.orderId], d.data)
      } else {
        deliveries[order.orderId] = d.data
      }
    } catch {  }
  }
}

async function saveSenderAndGenerateTTN(orderId: string) {
  const form = senderForms[orderId]
  const addr = senderAddress[orderId]
  if (!form || !addr) {
    syncMessages[orderId] = 'warn:Оберіть відділення відправки на карті вище'
    return
  }
  if (!form.name.trim() || !form.phone.trim()) {
    syncMessages[orderId] = 'warn:Введіть ваше ПІБ і телефон'
    return
  }
  processing.value = orderId
  syncMessages[orderId] = ''
  try {
    await deliveryApi.setSenderAddress(orderId, {
      ...addr,
      senderName: form.name,
      senderPhone: form.phone
    })
    const res = await deliveryApi.generateTTN(orderId)
    deliveries[orderId] = { ...deliveries[orderId], ttn: res.data.ttn, status: 'TTNCreated' }
    delete senderForms[orderId]
    syncMessages[orderId] = `ok:ТТН згенеровано: ${res.data.ttn}`
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    syncMessages[orderId] = `err:${err.response?.data?.error ?? 'Помилка генерації ТТН. Перевір дані.'}`
  } finally {
    processing.value = null
  }
}

async function trackOrder(orderId: string) {
  processing.value = orderId
  try {
    const res = await deliveryApi.track(orderId)
    deliveries[orderId] = res.data
  } finally {
    processing.value = null
  }
}

async function finalize(orderId: string) {
  processing.value = orderId
  syncMessages[orderId] = ''
  try {
    await paymentApi.finalizeOrder(orderId)
    await load()
    syncMessages[orderId] = 'ok:Кошти успішно зараховано на рахунок!'
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    syncMessages[orderId] = `err:${err.response?.data?.error ?? 'Помилка підтвердження доставки'}`
  } finally {
    processing.value = null
  }
}

async function cancel(orderId: string) {
  processing.value = orderId
  try {
    await paymentApi.cancelOrder(orderId)
    await load()
  } finally {
    processing.value = null
  }
}

async function confirmReceipt(orderId: string) {
  processing.value = orderId
  syncMessages[orderId] = ''
  try {
    await paymentApi.confirmReceipt(orderId)
    await load()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    syncMessages[orderId] = `err:${err.response?.data?.error ?? 'Помилка підтвердження'}`
  } finally {
    processing.value = null
  }
}

async function refundOrder(orderId: string) {
  processing.value = orderId
  syncMessages[orderId] = ''
  try {
    await paymentApi.refundOrder(orderId)
    await load()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    syncMessages[orderId] = `err:${err.response?.data?.error ?? 'Помилка відшкодування'}`
  } finally {
    processing.value = null
  }
}

async function syncStatus(orderId: string) {
  processing.value = orderId
  try {
    const { data } = await paymentApi.syncStatus(orderId)
    syncMessages[orderId] = data.message
    await load()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    syncMessages[orderId] = err.response?.data?.error ?? 'Помилка перевірки статусу'
  } finally {
    processing.value = null
  }
}

let refreshTimer: ReturnType<typeof setInterval> | null = null

watch(activeTab, load)
onMounted(() => {
  load()
  loadReviewState()
  refreshTimer = setInterval(silentRefresh, 30000)
})
onUnmounted(() => {
  if (refreshTimer) clearInterval(refreshTimer)
})
</script>
