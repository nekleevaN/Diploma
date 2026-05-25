<template>
  <div class="max-w-3xl mx-auto px-4 py-8">
    <div v-if="loading" class="card p-8 animate-pulse space-y-4">
      <div class="h-6 bg-gray-200 rounded w-3/4" />
      <div class="h-4 bg-gray-200 rounded w-1/4" />
      <div class="h-24 bg-gray-200 rounded" />
    </div>

    <div v-else-if="ad" class="space-y-4">
      <RouterLink to="/" class="text-sm text-teal-600 hover:underline">← Назад до оголошень</RouterLink>

      <div class="card overflow-hidden">
        
        <Teleport to="body">
          <Transition name="fade">
            <div v-if="lightboxOpen"
              class="fixed inset-0 z-50 bg-black/95 flex flex-col"
              @keydown.esc="lightboxOpen = false"
              tabindex="0" ref="lightboxEl">

              
              <div class="flex items-center justify-between px-5 py-3 shrink-0">
                <span class="text-white/70 text-sm">{{ lightboxIndex + 1 }} / {{ ad.imageUrls?.length }}</span>
                <button @click="lightboxOpen = false"
                  class="text-white/70 hover:text-white transition-colors p-2 rounded-lg hover:bg-white/10">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>

              
              <div class="flex-1 flex items-center justify-center relative min-h-0 px-12">
                <img :src="ad.imageUrls![lightboxIndex]" :alt="ad.title"
                  class="max-h-full max-w-full object-contain select-none"
                  @click.stop />

                <button v-if="lightboxIndex > 0"
                  @click="lightboxIndex--"
                  class="absolute left-2 top-1/2 -translate-y-1/2 w-11 h-11 bg-white/10 hover:bg-white/25 text-white rounded-full flex items-center justify-center transition-colors text-2xl">
                  ‹
                </button>
                <button v-if="ad.imageUrls && lightboxIndex < ad.imageUrls.length - 1"
                  @click="lightboxIndex++"
                  class="absolute right-2 top-1/2 -translate-y-1/2 w-11 h-11 bg-white/10 hover:bg-white/25 text-white rounded-full flex items-center justify-center transition-colors text-2xl">
                  ›
                </button>
              </div>

              
              <div v-if="ad.imageUrls && ad.imageUrls.length > 1"
                class="shrink-0 flex gap-2 justify-center px-5 py-4 overflow-x-auto">
                <button v-for="(url, i) in ad.imageUrls" :key="i"
                  @click="lightboxIndex = i"
                  :class="['w-14 h-14 rounded-lg overflow-hidden shrink-0 transition-all border-2',
                    i === lightboxIndex ? 'border-teal-400 opacity-100' : 'border-transparent opacity-50 hover:opacity-80']">
                  <img :src="url" class="w-full h-full object-cover" />
                </button>
              </div>
            </div>
          </Transition>
        </Teleport>

        
        <div class="relative bg-gray-900 overflow-hidden" style="max-height:480px">
          <template v-if="ad.imageUrls && ad.imageUrls.length > 0">
            
            <div class="relative cursor-zoom-in group" @click="openLightbox(currentImage)">
              <img :src="ad.imageUrls[currentImage]" :alt="ad.title"
                class="w-full object-contain"
                style="max-height:440px;min-height:240px" />
              
              <div class="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-black/10">
                <span class="bg-black/50 text-white text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM10 7v3m0 0v3m0-3h3m-3 0H7"/>
                  </svg>
                  Відкрити
                </span>
              </div>
            </div>

            
            <button v-if="currentImage > 0" @click.stop="currentImage--"
              class="absolute left-2 top-1/2 -translate-y-1/2 w-9 h-9 bg-black/40 hover:bg-black/60 text-white rounded-full flex items-center justify-center text-xl transition-colors">
              ‹
            </button>
            <button v-if="currentImage < ad.imageUrls.length - 1" @click.stop="currentImage++"
              class="absolute right-2 top-1/2 -translate-y-1/2 w-9 h-9 bg-black/40 hover:bg-black/60 text-white rounded-full flex items-center justify-center text-xl transition-colors">
              ›
            </button>

            
            <span v-if="ad.imageUrls.length > 1"
              class="absolute bottom-3 right-3 bg-black/50 text-white text-xs px-2.5 py-1 rounded-full">
              {{ currentImage + 1 }} / {{ ad.imageUrls.length }}
            </span>
          </template>

          <div v-else class="flex flex-col items-center gap-2 text-gray-500 py-24">
            <span class="text-5xl">📷</span>
            <span class="text-sm">Фото відсутнє</span>
          </div>

          
          <span class="absolute top-3 left-3 bg-white/85 backdrop-blur-sm rounded-xl w-9 h-9 flex items-center justify-center shadow">
            <CategoryIcon :slug="categorySlug" size="w-5 h-5" class="text-gray-600" />
          </span>
        </div>

        
        <div v-if="ad.imageUrls && ad.imageUrls.length > 1"
          class="flex gap-2 px-4 py-3 bg-gray-50 border-b border-ivory-400 overflow-x-auto scrollbar-hide">
          <button v-for="(url, i) in ad.imageUrls" :key="i"
            @click="currentImage = i"
            :class="['w-16 h-16 rounded-lg overflow-hidden shrink-0 transition-all border-2',
              i === currentImage ? 'border-teal-500 opacity-100' : 'border-transparent opacity-60 hover:opacity-100']">
            <img :src="url" class="w-full h-full object-cover" />
          </button>
        </div>

        <div class="p-6">
          <div class="flex items-start justify-between gap-4 flex-wrap">
            <div>
              <span class="inline-flex items-center gap-1.5 text-xs font-medium text-teal-600 bg-teal-50 px-2.5 py-1 rounded-full">
                <CategoryIcon :slug="categorySlug" size="w-3.5 h-3.5" />
                {{ (ad as any).categoryLabel || ad.category }}
              </span>
              <h1 class="text-xl font-bold text-gray-900 mt-2">{{ ad.title }}</h1>
            </div>
            <div class="text-right">
              <p class="text-2xl font-bold text-gray-900">₴{{ ad.price.toLocaleString() }}</p>
              <p v-if="acceptedOffer" class="text-xs text-teal-500 font-medium mt-0.5">
                Ваша ціна: ₴{{ acceptedOffer.offeredPrice.toLocaleString() }}
              </p>
            </div>
          </div>

          
          <div class="flex flex-wrap gap-2 mt-3">
            <span v-if="(ad as any).condition" :class="['text-xs font-medium px-2.5 py-1 rounded-full', conditionBadge]">
              {{ conditionLabel }}
            </span>
            <span v-if="(ad as any).brand" class="text-xs font-medium px-2.5 py-1 rounded-full bg-teal-150 text-teal-700 border border-teal-300">
              {{ (ad as any).brand }}
            </span>
            <span v-if="(ad as any).size" class="text-xs font-medium px-2.5 py-1 rounded-full bg-teal-150 text-teal-700 border border-teal-300">
              Розмір {{ (ad as any).size }}
            </span>
            <span v-if="(ad as any).color" class="flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded-full bg-teal-150 text-teal-700 border border-teal-300">
              <span :style="colorStyle" class="inline-block w-3 h-3 rounded-full border border-teal-300"></span>
              {{ colorLabel }}
            </span>
          </div>

          <div class="flex items-center gap-2 mt-3 text-sm text-gray-500">
            <RouterLink :to="`/users/${ad.sellerId}`" class="font-medium text-gray-700 hover:text-teal-600">
              {{ sellerDisplayName || ad.sellerName }}
            </RouterLink>
            <span>·</span>
            <AppIcon name="star" size="w-3.5 h-3.5" class="text-yellow-400 fill-yellow-400" :stroke-width="0" />
            <span>{{ ad.sellerRating.toFixed(1) }}</span>
            <span v-if="ad.status !== 'Active'" class="ml-2 px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-500">{{ statusLabel }}</span>
          </div>

          <p class="mt-4 text-gray-600 text-sm leading-relaxed whitespace-pre-line">{{ ad.description }}</p>

          
          <div v-if="auth.isAuthenticated && ad.sellerId === auth.userId" class="mt-5 flex gap-2">
            <RouterLink :to="`/ads/${ad.id}/edit`"
              class="inline-flex items-center gap-1.5 px-4 py-2 bg-teal-50 text-teal-700 text-sm font-medium rounded-lg hover:bg-teal-50 transition-colors">
              <AppIcon name="edit" size="w-4 h-4" /> Редагувати
            </RouterLink>
          </div>

          
          <div v-if="ad.status === 'Active'" class="mt-4 flex flex-wrap gap-3">
            <template v-if="auth.isAuthenticated && ad.sellerId !== auth.userId">
              
              <div v-if="acceptedOffer" class="w-full mb-1 bg-teal-150 border border-teal-150 rounded-xl px-3 py-2">
                <p class="text-xs text-teal-500 font-medium flex items-center gap-1.5"><AppIcon name="check-circle" size="w-4 h-4" /> Продавець прийняв вашу пропозицію</p>
                <div class="flex items-center gap-3 mt-1">
                  <span class="text-sm text-gray-400 line-through">₴{{ ad.price.toLocaleString() }}</span>
                  <span class="text-lg font-bold text-teal-500">₴{{ acceptedOffer.offeredPrice.toLocaleString() }}</span>
                  <span class="text-xs text-teal-500">(-{{ Math.round((1 - acceptedOffer.offeredPrice / ad.price) * 100) }}%)</span>
                </div>
              </div>

              
              <template v-if="(ad as any).isPayoutEnabled">
                <button @click="goToCheckout"
                  class="inline-flex items-center gap-2 px-4 py-2 bg-teal-500 text-white text-sm font-medium rounded-lg hover:bg-teal-600 transition-colors">
                  <AppIcon name="buy" size="w-4 h-4" />
                  Купити · ₴{{ (acceptedOffer?.offeredPrice ?? ad.price).toLocaleString() }}
                </button>
              </template>
              <template v-else>
                <div class="w-full rounded-xl bg-amber-50 border border-amber-200 px-4 py-3 space-y-1.5">
                  <p class="text-sm font-medium text-amber-800 flex items-center gap-2">
                    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                    </svg>
                    Купівля тимчасово недоступна
                  </p>
                  <p class="text-xs text-amber-700">
                    Продавець ще не підключив отримання виплат через Monobank.
                    Напишіть йому у чат, щоб він налаштував профіль.
                  </p>
                  <button @click="startChat" :disabled="chatLoading"
                    class="text-xs text-teal-600 underline hover:text-teal-800 mt-1">
                    Написати продавцю →
                  </button>
                </div>
              </template>

              
              <button v-if="!acceptedOffer" @click="showOfferModal = true" class="btn-secondary gap-2">
                <AppIcon name="handshake" size="w-4 h-4" />
                <span>Запропонувати ціну</span>
              </button>
              <button @click="startChat" :disabled="chatLoading" class="btn-secondary gap-2">
                <AppIcon name="chat" size="w-4 h-4" />
                <span v-if="chatLoading">...</span>
                <span v-else>Написати продавцю</span>
              </button>

              
              <button v-if="(ad as any).latitude" @click="requestViewing" :disabled="viewingLoading" class="btn-secondary gap-2">
                <AppIcon name="eye" size="w-4 h-4" />
                <span v-if="viewingLoading">...</span>
                <span v-else>Домовитись про перегляд</span>
              </button>
            </template>
            <RouterLink v-else-if="!auth.isAuthenticated" to="/login" class="btn-primary">
              Увійти щоб купити
            </RouterLink>
            <span v-else class="text-sm text-gray-400 italic">Це ваше оголошення</span>
          </div>
          <div v-else class="mt-4 inline-block px-3 py-1.5 bg-gray-100 text-gray-500 text-sm rounded-full">{{ statusLabel }}</div>

          
          <div v-if="(ad as any).locationAddress" class="mt-4 flex items-center gap-2 text-sm text-gray-500">
            <AppIcon name="location" size="w-4 h-4 shrink-0" />
            <span>{{ (ad as any).locationAddress }}</span>
          </div>

          <div v-if="viewingSuccess" class="mt-3 bg-teal-150 border border-teal-150 rounded-xl px-4 py-2 text-sm text-teal-500 flex items-center gap-2">
            <AppIcon name="check-circle" size="w-4 h-4" /> Запит на перегляд надіслано продавцю у чат!
          </div>
          <div v-if="payError" class="mt-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ payError }}</div>
          <div v-if="offerSuccess" class="mt-3 text-sm text-teal-500 bg-teal-150 rounded-lg px-3 py-2 flex items-center gap-2">
            <AppIcon name="check-circle" size="w-4 h-4" /> Пропозицію надіслано! Продавець отримає сповіщення.
          </div>
        </div>
      </div>
    </div>

    <div v-else class="text-center py-16 text-gray-400">
      <AppIcon name="search" size="w-12 h-12 mx-auto mb-3 opacity-30" />
      <p>Оголошення не знайдено</p>
    </div>

    <OfferModal
      v-if="showOfferModal && ad"
      :ad-id="ad.id"
      :current-price="ad.price"
      @close="showOfferModal = false"
      @submitted="onOfferSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { catalogApi } from '@/api/catalog'
import { chatApi } from '@/api/chat'
import { paymentApi } from '@/api/payment'
import { offersApi, type OfferDto } from '@/api/offers'
import { usersApi } from '@/api/users'
import { useAuthStore } from '@/stores/auth'
import OfferModal from '@/components/OfferModal.vue'
import AppIcon from '@/components/AppIcon.vue'
import CategoryIcon from '@/components/CategoryIcon.vue'
import type { Ad } from '@/types'
import { CATEGORY_TREE, COLORS, getConditionLabel, getConditionBadge, getColorLabel } from '@/data/categories'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const ad = ref<Ad & { imageUrls?: string[]; sellerId?: string } | null>(null)
const sellerDisplayName = ref<string | null>(null)
const loading = ref(true)
const chatLoading = ref(false)
const viewingLoading = ref(false)
const viewingSuccess = ref(false)
const payError = ref('')
const showOfferModal = ref(false)
const offerSuccess = ref(false)
const currentImage = ref(0)
const acceptedOffer = ref<OfferDto | null>(null)

const lightboxOpen  = ref(false)
const lightboxIndex = ref(0)
const lightboxEl    = ref<HTMLElement | null>(null)

function openLightbox(index: number) {
  lightboxIndex.value = index
  lightboxOpen.value  = true
  nextTick(() => lightboxEl.value?.focus())
}

function onKeydown(e: KeyboardEvent) {
  if (!lightboxOpen.value) return
  const images = ad.value?.imageUrls ?? []
  if (e.key === 'Escape')    { lightboxOpen.value = false }
  if (e.key === 'ArrowLeft'  && lightboxIndex.value > 0) lightboxIndex.value--
  if (e.key === 'ArrowRight' && lightboxIndex.value < images.length - 1) lightboxIndex.value++
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onUnmounted(() => window.removeEventListener('keydown', onKeydown))

const categorySlug = computed(() => ad.value?.category ?? '')
const conditionLabel = computed(() => getConditionLabel((ad.value as any)?.condition))
const conditionBadge = computed(() => getConditionBadge((ad.value as any)?.condition))
const colorLabel = computed(() => getColorLabel((ad.value as any)?.color))
const colorStyle = computed(() => {
  const clr = COLORS.find(c => c.slug === (ad.value as any)?.color)
  if (!clr) return ''
  return clr.slug === 'multicolor'
    ? 'background: conic-gradient(red,orange,yellow,green,blue,violet,red)'
    : `background-color:${clr.hex}`
})
const statusLabel = computed(() => ({ Sold: 'Продано', Reserved: 'Зарезервовано', Removed: 'Знято' }[ad.value?.status ?? ''] ?? ''))

async function startChat() {
  if (!ad.value) return
  chatLoading.value = true
  try {
    const { data } = await chatApi.startChat(ad.value.sellerId!, ad.value.id, ad.value.title)
    router.push(`/chats/${data.chatId}`)
  } finally { chatLoading.value = false }
}

async function requestViewing() {
  if (!ad.value) return
  viewingLoading.value = true
  try {
    const { data } = await chatApi.startChat(ad.value.sellerId!, ad.value.id, ad.value.title)
    const buyerName = auth.displayName || auth.firstName || 'Покупець'
    const msg = `👁️ *Запит на перегляд*\n${buyerName} хоче переглянути ваш товар «${ad.value.title}».\n📍 ${(ad.value as any).locationAddress ?? 'Адреса вказана в оголошенні'}\n\nОберіть зручний час через кнопку «📅 Час зустрічі» внизу чату.`
    await chatApi.sendMessage(data.chatId, msg)
    router.push(`/chats/${data.chatId}?fromViewing=1`)
  } finally { viewingLoading.value = false }
}

function goToCheckout() {
  if (!ad.value) return
  const price = acceptedOffer.value?.offeredPrice ?? ad.value.price
  router.push({ name: 'checkout', params: { adId: ad.value.id }, query: { amount: price } })
}

function onOfferSubmitted(_offerId: string) {
  showOfferModal.value = false
  offerSuccess.value = true
  setTimeout(() => { offerSuccess.value = false }, 5000)
}

onMounted(async () => {
  try {
    const { data } = await catalogApi.getById(route.params.id as string)
    ad.value = data as any

    if ((data as any).sellerId) {
      try {
        const profileRes = await usersApi.getProfile((data as any).sellerId)
        sellerDisplayName.value = profileRes.data.displayName || profileRes.data.username
      } catch {  }
    }

    if (auth.isAuthenticated && data.sellerId !== auth.userId) {
      try {
        const offersRes = await offersApi.getMyOffers()
        acceptedOffer.value = offersRes.data.find(
          o => o.advertisementId === data.id && o.status === 'Accepted'
        ) ?? null
      } catch {  }
    }
  } finally {
    loading.value = false
  }
})
</script>
