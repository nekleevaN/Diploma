<template>
  <div class="max-w-2xl mx-auto px-4 py-8">
    <h1 class="text-xl font-bold text-gray-900 mb-2">Торги</h1>

    
    <div class="flex border-b border-gray-200 mb-6">
      <button v-for="tab in tabs" :key="tab.id"
        @click="activeTab = tab.id"
        :class="['px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
          activeTab === tab.id
            ? 'border-teal-500 text-teal-600'
            : 'border-transparent text-gray-500 hover:text-gray-700']">
        {{ tab.label }}
        <span v-if="tab.id === 'seller' && pendingCount > 0"
          class="ml-1.5 bg-red-500 text-white text-xs rounded-full px-1.5 py-0.5">
          {{ pendingCount }}
        </span>
      </button>
    </div>

    
    <div v-if="activeTab === 'buyer'">
      <div v-if="loadingBuyer" class="text-center py-8 text-gray-400 text-sm">Завантаження...</div>
      <div v-else-if="buyerOffers.length === 0" class="text-center py-8 text-gray-400">
        <div class="flex justify-center mb-2"><AppIcon name="handshake" size="w-8 h-8" /></div>
        <p class="text-sm">Ви ще не робили пропозицій</p>
      </div>
      <template v-else>
        
        <div v-if="activeBuyerOffers.length > 0" class="space-y-3 mb-6">
          <div v-for="offer in activeBuyerOffers" :key="offer.offerId" class="card p-4">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-sm font-medium text-gray-900">
                Оголошення: <RouterLink :to="`/ads/${offer.advertisementId}`" class="text-teal-600 hover:underline">переглянути →</RouterLink>
              </p>
              <p class="text-base font-bold text-gray-900 mt-1">₴{{ offer.offeredPrice.toLocaleString() }}</p>
              <p class="text-xs text-gray-400">{{ formatDate(offer.createdAt) }}</p>
            </div>
            <span :class="statusClass(offer.status)" class="text-xs font-medium px-2.5 py-1 rounded-full">
              {{ statusLabel(offer.status) }}
            </span>
          </div>
          
          <div v-if="offer.status === 'Accepted'" class="mt-3 bg-teal-150 border border-teal-150 rounded-xl p-3">
            <p class="text-xs font-medium text-teal-500 mb-2 flex items-center gap-1"><AppIcon name="check-circle" size="w-3.5 h-3.5" /> Продавець прийняв вашу ціну ₴{{ offer.offeredPrice.toLocaleString() }}</p>
            <RouterLink :to="`/ads/${offer.advertisementId}`"
              class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600">
              <AppIcon name="buy" size="w-3.5 h-3.5" /> Перейти до оплати
            </RouterLink>
          </div>

          
          <div v-if="offer.status === 'CounterOffered'" class="mt-3 bg-teal-50 border border-teal-200 rounded-xl p-3 space-y-2">
            <p class="text-xs font-medium text-teal-700">Продавець пропонує зустрічну ціну:</p>
            <div class="flex items-center gap-3">
              <span class="text-sm text-gray-400 line-through">₴{{ offer.offeredPrice.toLocaleString() }}</span>
              <span class="text-lg font-bold text-blue-900">₴{{ offer.counterPrice?.toLocaleString() }}</span>
            </div>
            <p v-if="offer.sellerNote" class="text-xs text-teal-600 italic">{{ offer.sellerNote }}</p>
            <div class="flex gap-2 pt-1">
              <button @click="acceptCounter(offer)" :disabled="accepting === offer.offerId"
                class="flex-1 py-1.5 px-3 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600 disabled:opacity-50 flex items-center justify-center gap-1">
                <template v-if="accepting === offer.offerId">...</template>
                <template v-else><AppIcon name="check-circle" size="w-3.5 h-3.5" /> Прийняти ₴{{ offer.counterPrice?.toLocaleString() }}</template>
              </button>
              <button @click="rejectCounter(offer.offerId)" :disabled="accepting === offer.offerId"
                class="py-1.5 px-3 bg-red-50 text-red-600 text-xs font-medium rounded-lg hover:bg-red-100 disabled:opacity-50">
                Відхилити
              </button>
            </div>
          </div>
          <p v-if="offer.sellerNote && offer.status === 'Rejected'" class="mt-2 text-xs text-gray-500">
            Причина: {{ offer.sellerNote }}
          </p>
        </div>
        </div>

        
        <div v-if="activeBuyerOffers.length === 0" class="text-center py-6 text-gray-400">
          <div class="flex justify-center mb-2"><AppIcon name="check-circle" size="w-6 h-6" /></div>
          <p class="text-sm">Немає активних пропозицій</p>
        </div>

        
        <div v-if="archiveBuyerOffers.length > 0">
          <button @click="showArchive = !showArchive"
            class="flex items-center gap-2 text-xs text-gray-400 hover:text-gray-600 mb-2">
            <span>{{ showArchive ? '▼' : '▶' }}</span>
            Архів ({{ archiveBuyerOffers.length }})
          </button>
          <div v-if="showArchive" class="space-y-2">
            <div v-for="offer in archiveBuyerOffers" :key="offer.offerId"
              class="bg-gray-50 rounded-xl p-3 flex items-center justify-between gap-3 opacity-70">
              <div class="min-w-0">
                <RouterLink :to="`/ads/${offer.advertisementId}`" class="text-xs text-teal-500 hover:underline truncate block">
                  переглянути оголошення →
                </RouterLink>
                <p class="text-sm font-medium text-gray-700 mt-0.5">₴{{ offer.offeredPrice.toLocaleString() }}</p>
                <p class="text-xs text-gray-400">{{ formatDate(offer.createdAt) }}</p>
              </div>
              <span :class="statusClass(offer.status)" class="text-xs font-medium px-2 py-0.5 rounded-full shrink-0">
                {{ statusLabel(offer.status) }}
              </span>
            </div>
          </div>
        </div>
      </template>
    </div>

    
    <div v-if="activeTab === 'seller'">
      <div v-if="loadingSeller" class="text-center py-8 text-gray-400 text-sm">Завантаження...</div>
      <div v-else-if="sellerOffers.length === 0" class="text-center py-8 text-gray-400">
        <div class="flex justify-center mb-2"><AppIcon name="chat" size="w-8 h-8" /></div>
        <p class="text-sm">Пропозицій поки немає</p>
      </div>
      <div v-else class="space-y-3">
        <div v-for="offer in sellerOffers.filter(o => o.status === 'Pending' || o.status === 'CounterOffered')" :key="offer.offerId" class="card p-4">
          <div class="flex justify-between items-start mb-3">
            <div>
              <p class="text-sm text-gray-500">Від: <span class="font-medium text-gray-900">{{ offer.buyerName }}</span></p>
              <p class="text-base font-bold text-gray-900 mt-0.5">₴{{ offer.offeredPrice.toLocaleString() }}</p>
              <p class="text-xs text-gray-400">{{ formatDate(offer.createdAt) }}</p>
            </div>
            <span :class="statusClass(offer.status)" class="text-xs font-medium px-2.5 py-1 rounded-full">
              {{ statusLabel(offer.status) }}
            </span>
          </div>

          <div v-if="offer.status === 'Pending' || offer.status === 'CounterOffered'">
            
            <div v-if="respondingOffer === offer.offerId" class="space-y-2 mb-3">
              <input v-model.number="counterPrice" type="number" class="input" placeholder="Зустрічна ціна (₴)" :min="1" />
              <input v-model="sellerNote" type="text" class="input" placeholder="Коментар (необов'язково)" />
            </div>
            <div class="flex gap-2">
              <button @click="respond(offer.offerId, 'accept')" :disabled="responding"
                class="flex-1 py-1.5 px-3 bg-teal-500 text-white text-xs font-medium rounded-lg hover:bg-teal-600 transition-colors disabled:opacity-50 flex items-center justify-center gap-1">
                <AppIcon name="check-circle" size="w-3.5 h-3.5" /> Прийняти
              </button>
              <button @click="toggleCounter(offer.offerId)" :disabled="responding"
                :class="['flex-1 py-1.5 px-3 text-xs font-medium rounded-lg transition-colors disabled:opacity-50',
                  respondingOffer === offer.offerId ? 'bg-teal-500 text-white hover:bg-teal-600' : 'btn-secondary']">
                <template v-if="respondingOffer === offer.offerId">Надіслати ↑</template>
                <template v-else><span class="flex items-center gap-1"><AppIcon name="chat" size="w-3.5 h-3.5" /> Зустрічна</span></template>
              </button>
              <button @click="respond(offer.offerId, 'reject')" :disabled="responding"
                class="flex-1 py-1.5 px-3 bg-red-100 text-red-700 text-xs font-medium rounded-lg hover:bg-red-200 transition-colors disabled:opacity-50 flex items-center justify-center gap-1">
                <AppIcon name="x-circle" size="w-3.5 h-3.5" /> Відхилити
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { offersApi, type OfferDto } from '@/api/offers'
import { catalogApi } from '@/api/catalog'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import AppIcon from '@/components/AppIcon.vue'
import type { AdListItem } from '@/types'

const auth = useAuthStore()
const notifications = useNotificationsStore()
const activeTab = ref<'buyer' | 'seller'>('seller')
const tabs = [
  { id: 'seller' as const, label: 'Пропозиції на мої оголошення' },
  { id: 'buyer' as const, label: 'Мої пропозиції' }
]

const buyerOffers = ref<OfferDto[]>([])
const sellerOffers = ref<OfferDto[]>([])
const loadingBuyer = ref(false)
const loadingSeller = ref(false)
const respondingOffer = ref<string | null>(null)
const counterPrice = ref<number | undefined>(undefined)
const accepting = ref<string | null>(null)
const sellerNote = ref('')
const responding = ref(false)
const showArchive = ref(false)

const pendingCount = computed(() =>
  sellerOffers.value.filter(o => o.status === 'Pending').length)

const activeBuyerOffers = computed(() =>
  buyerOffers.value.filter(o => o.status === 'Pending' || o.status === 'CounterOffered'))

const archiveBuyerOffers = computed(() =>
  buyerOffers.value.filter(o => o.status === 'Accepted' || o.status === 'Rejected'))

function statusLabel(status: string) {
  return { Pending: 'Очікує', Accepted: 'Прийнято', Rejected: 'Відхилено', CounterOffered: 'Зустрічна' }[status] ?? status
}

function statusClass(status: string) {
  return {
    Pending: 'bg-yellow-100 text-yellow-700',
    Accepted: 'bg-teal-150 text-teal-700 border border-teal-300',
    Rejected: 'bg-red-100 text-red-700',
    CounterOffered: 'bg-teal-50 text-teal-700'
  }[status] ?? 'bg-gray-100 text-gray-700'
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })
}

function toggleCounter(offerId: string) {
  if (respondingOffer.value === offerId) {
    respond(offerId, 'counter')
  } else {
    respondingOffer.value = offerId
    counterPrice.value = undefined
    sellerNote.value = ''
  }
}

async function respond(offerId: string, action: 'accept' | 'reject' | 'counter') {
  responding.value = true
  try {
    await offersApi.respond(offerId, action, counterPrice.value, sellerNote.value || undefined)
    respondingOffer.value = null
    await loadSellerOffers()
    notifications.refresh()
  } finally {
    responding.value = false
  }
}

async function acceptCounter(offer: OfferDto) {
  if (!offer.counterPrice) return
  accepting.value = offer.offerId
  try {
    const { data } = await offersApi.acceptCounter(offer.offerId)
    const idx = buyerOffers.value.findIndex(o => o.offerId === offer.offerId)
    if (idx >= 0) {
      buyerOffers.value[idx] = {
        ...buyerOffers.value[idx],
        status: 'Accepted',
        offeredPrice: data.agreedPrice,
        counterPrice: null
      }
    }
    setTimeout(() => { window.location.href = `/ads/${offer.advertisementId}` }, 800)
  } catch {
    window.location.href = `/ads/${offer.advertisementId}`
  } finally {
    accepting.value = null
  }
}

async function rejectCounter(offerId: string) {
  accepting.value = offerId
  try {
    await offersApi.respond(offerId, 'reject', undefined, 'Покупець відхилив зустрічну пропозицію')
    const res = await offersApi.getMyOffers()
    buyerOffers.value = res.data
  } finally {
    accepting.value = null
  }
}

async function loadSellerOffers() {
  loadingSeller.value = true
  try {
    const adsRes = await catalogApi.getAll({ page: 1, pageSize: 50 })
    const myAds = adsRes.data.items.filter((a: AdListItem) => (a as any).sellerId === auth.userId)
    const allOffers: OfferDto[] = []
    for (const ad of myAds) {
      try {
        const res = await offersApi.getAdOffers(ad.id)
        allOffers.push(...res.data)
      } catch {  }
    }
    sellerOffers.value = allOffers.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  } finally {
    loadingSeller.value = false
  }
}

onMounted(async () => {
  loadingBuyer.value = true
  await Promise.all([
    offersApi.getMyOffers().then(r => { buyerOffers.value = r.data }).finally(() => { loadingBuyer.value = false }),
    loadSellerOffers()
  ])
})
</script>
