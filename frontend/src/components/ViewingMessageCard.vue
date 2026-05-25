<template>
  
  <div v-if="data.type === 'viewing_proposal'" class="mt-2 rounded-2xl overflow-hidden border border-teal-200 bg-teal-50 max-w-xs shadow-sm">
    <div class="px-4 py-3">
      <div class="flex items-center gap-2 mb-2">
        <AppIcon name="calendar" size="w-4 h-4" class="text-teal-600 shrink-0" />
        <p class="text-xs font-semibold text-teal-700">Пропозиція часу перегляду</p>
      </div>
      <p class="text-base font-bold text-blue-900">{{ formatDateTime(data.dateTime) }}</p>

      
      <div v-if="canRespond" class="mt-3 space-y-2">
        <p class="text-xs text-teal-600">Ваша відповідь:</p>
        <div class="flex gap-2">
          <button @click="respond('accept')" :disabled="loading"
            class="flex-1 py-2 text-xs font-medium bg-teal-500 text-white rounded-xl hover:bg-teal-600 disabled:opacity-50 transition-colors flex items-center justify-center gap-1">
            <AppIcon name="check-circle" size="w-3.5 h-3.5" class="inline" /> Погоджуюсь
          </button>
          <button @click="respond('decline')" :disabled="loading"
            class="py-2 px-3 text-xs font-medium bg-red-50 text-red-600 rounded-xl hover:bg-red-100 disabled:opacity-50 transition-colors flex items-center justify-center">
            <AppIcon name="x-circle" size="w-3.5 h-3.5" />
          </button>
        </div>
        <button @click="showReschedule = !showReschedule" :disabled="loading"
          class="w-full py-2 text-xs font-medium bg-white border border-teal-200 text-teal-700 rounded-xl hover:bg-teal-50 disabled:opacity-50 transition-colors flex items-center justify-center gap-1">
          <AppIcon name="calendar" size="w-3.5 h-3.5" /> Запропонувати інший час
        </button>

        
        <div v-if="showReschedule" class="space-y-2 pt-1">
          <div class="flex gap-2">
            <input v-model="newDate" type="date" :min="today" class="input text-xs flex-1" />
            <input v-model="newTime" type="time" class="input text-xs w-24" />
          </div>
          <button @click="respond('reschedule')" :disabled="loading || !newDate || !newTime"
            class="w-full py-2 text-xs font-medium bg-teal-500 text-white rounded-xl hover:bg-teal-600 disabled:opacity-50">
            Запропонувати цей час
          </button>
        </div>
      </div>

      
      <p v-else class="text-xs text-teal-500 mt-2">Очікується відповідь...</p>
    </div>
  </div>

  
  <div v-else-if="data.type === 'viewing_accepted'" class="mt-2 rounded-2xl overflow-hidden border border-teal-150 bg-teal-150 max-w-xs shadow-sm">
    <div class="px-4 py-3">
      <div class="flex items-center gap-2 mb-1">
        <AppIcon name="check-circle" size="w-4 h-4" class="text-teal-500 shrink-0" />
        <p class="text-xs font-semibold text-teal-500">Перегляд підтверджено!</p>
      </div>
      <p class="text-sm font-bold text-green-900">{{ formatDateTime(data.dateTime) }}</p>
      <p class="text-xs text-teal-500 mt-1.5 flex items-center gap-1"><AppIcon name="bell" size="w-3.5 h-3.5" class="inline" /> Довіреній особі надіслано сповіщення в Telegram</p>
    </div>
  </div>

  
  <div v-else-if="data.type === 'viewing_declined'" class="mt-2 rounded-2xl overflow-hidden border border-gray-200 bg-gray-50 max-w-xs shadow-sm">
    <div class="px-4 py-2.5 flex items-center gap-2">
      <AppIcon name="x-circle" size="w-4 h-4" class="text-gray-400 shrink-0" />
      <p class="text-xs text-gray-500">Пропозицію перегляду відхилено</p>
    </div>
  </div>

  
  <div v-else-if="data.type === 'viewing_followup'" class="mt-2 rounded-2xl overflow-hidden border border-purple-200 bg-purple-50 max-w-sm shadow-sm">
    <div class="px-4 py-3">
      <div class="flex items-center gap-2 mb-2">
        <AppIcon name="info" size="w-4 h-4" class="text-purple-600 shrink-0" />
        <p class="text-xs font-semibold text-purple-700">Як пройшов перегляд?</p>
      </div>
      <p class="text-xs text-purple-600 mb-3">«{{ data.adTitle }}»</p>
      <p v-if="followUpError" class="text-xs text-red-500 mb-2">{{ followUpError }}</p>
      <div v-if="isBuyer" class="space-y-2">
        <button @click="handleBuy" :disabled="loading"
          class="w-full py-2.5 text-xs font-medium bg-teal-500 text-white rounded-xl hover:bg-teal-600 disabled:opacity-50 transition-colors text-left px-3 flex items-center gap-1.5">
          <AppIcon name="buy" size="w-3.5 h-3.5" />
          <span v-if="loading">...</span>
          <span v-else>Хочу купити (оплата через платформу)</span>
        </button>
        <button @click="handleBuyDelivery" :disabled="loading"
          class="w-full py-2.5 text-xs font-medium bg-teal-500 text-white rounded-xl hover:bg-teal-600 disabled:opacity-50 transition-colors text-left px-3 flex items-center gap-1.5">
          <AppIcon name="package" size="w-3.5 h-3.5" /> Хочу купити + доставка Нової Пошти
        </button>
        <button @click="handleCancelled" :disabled="loading"
          class="w-full py-2.5 text-xs font-medium bg-gray-100 text-gray-600 rounded-xl hover:bg-gray-200 disabled:opacity-50 transition-colors text-left px-3 flex items-center gap-1.5">
          <AppIcon name="x-circle" size="w-3.5 h-3.5" /> Угода не відбудеться
        </button>
      </div>
      <p v-else class="text-xs text-purple-400 mt-1">Очікується відповідь покупця...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { viewingApi } from '@/api/viewing'
import { paymentApi } from '@/api/payment'
import { catalogApi } from '@/api/catalog'
import { useAuthStore } from '@/stores/auth'
import AppIcon from '@/components/AppIcon.vue'

const props = defineProps<{
  rawContent: string
  senderId: string
  currentUserId: string
  responderId: string
  senderName?: string
  adId?: string
  partnerId?: string
}>()

const emit = defineEmits<{ responded: [] }>()

const auth = useAuthStore()
const router = useRouter()
const loading = ref(false)
const showReschedule = ref(false)
const newDate = ref('')
const newTime = ref('')
const today = new Date().toISOString().split('T')[0]

const data = computed(() => {
  try { return JSON.parse(props.rawContent) }
  catch { return {} }
})

const canRespond = computed(() => {
  if (data.value.type !== 'viewing_proposal') return false
  const responder = data.value.responderId ?? props.responderId
  return !!responder && props.currentUserId === responder
})

function formatDateTime(iso: string) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleDateString('uk-UA', { weekday: 'long', day: 'numeric', month: 'long' }) +
         ' о ' + d.toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' })
}

async function respond(action: 'accept' | 'decline' | 'reschedule') {
  loading.value = true
  try {
    await viewingApi.respond(data.value.viewingId, {
      action,
      newDateTime: action === 'reschedule' && newDate.value && newTime.value
        ? new Date(`${newDate.value}T${newTime.value}`).toISOString()
        : undefined,
      responderTrustedTelegramId: auth.trustedContactTelegramId ?? undefined,
      responderTrustedEmail: auth.trustedContactEmail || undefined,
      proposerName: props.senderName ?? undefined
    })
    showReschedule.value = false
    emit('responded')
  } finally {
    loading.value = false
  }
}

const isBuyer = computed(() => data.value.type === 'viewing_followup' && props.senderId === props.currentUserId)
const followUpError = ref<string | null>(null)

async function handleBuy() {
  const advertisementId = data.value.advertisementId || props.adId
  const sellerId = data.value.sellerId || props.partnerId
  followUpError.value = null
  if (!advertisementId || !sellerId) {
    followUpError.value = `Не вдалось визначити оголошення. adId=${advertisementId}, sellerId=${sellerId}`
    return
  }
  loading.value = true
  try {
    await viewingApi.followUp(data.value.viewingId, 'buy')
    const { data: ad } = await catalogApi.getById(advertisementId)
    const { data: order } = await paymentApi.createOrder(
      advertisementId,
      sellerId,
      data.value.adTitle,
      (ad as any).price,
      false
    )
    if (!order.pageUrl) {
      followUpError.value = 'Monobank не повернув посилання на оплату'
      return
    }
    window.location.href = order.pageUrl
  } catch (e: any) {
    followUpError.value = e?.response?.data?.error ?? e?.message ?? 'Невідома помилка'
  } finally {
    loading.value = false
  }
}

function handleBuyDelivery() {
  const advertisementId = data.value.advertisementId || props.adId
  if (!advertisementId) return
  viewingApi.followUp(data.value.viewingId, 'buy_delivery')
  router.push(`/checkout/${advertisementId}`)
}

async function handleCancelled() {
  loading.value = true
  try {
    await viewingApi.followUp(data.value.viewingId, 'cancelled')
    emit('responded')
  } finally {
    loading.value = false
  }
}
</script>
