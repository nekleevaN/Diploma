<template>
  <nav class="bg-ivory-100 border-b border-ivory-400 sticky top-0 z-50 shadow-sm">
    <div class="max-w-7xl mx-auto px-4 h-16 flex items-center gap-4">

      
      <RouterLink to="/" class="flex items-center gap-2 shrink-0">
        <div class="w-8 h-8 bg-teal-500 rounded-lg flex items-center justify-center shadow-sm">
          <span class="text-white font-bold text-base leading-none">T</span>
        </div>
        <span class="text-xl font-bold text-gray-900 tracking-tight">trustee</span>
      </RouterLink>

      
      <div class="flex-1 max-w-xl hidden sm:block">
        <form @submit.prevent="submitSearch" class="relative">
          <svg class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none"
            fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            autocomplete="off"
            placeholder="Пошук оголошень..."
            class="w-full pl-10 pr-10 py-2.5 bg-ivory-200 border border-ivory-400 rounded-full text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-teal-400 focus:bg-white transition-all duration-200 shadow-inner"
            @keydown.enter.prevent="submitSearch"
          />
          <button v-if="searchQuery" type="button" @click="clearSearch"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </form>
      </div>

      
      <div class="flex items-center gap-1 ml-auto">
        <RouterLink to="/map"
          class="hidden md:flex items-center gap-1.5 px-3 py-2 text-sm text-gray-500 hover:text-teal-600 hover:bg-teal-50 rounded-lg transition-all duration-200">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7"/>
          </svg>
          Карта
        </RouterLink>

        <template v-if="auth.isAuthenticated">
          <RouterLink to="/chats"
            class="flex items-center gap-1.5 px-3 py-2 text-sm text-gray-500 hover:text-teal-600 hover:bg-teal-50 rounded-lg transition-all duration-200">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 10h.01M12 10h.01M16 10h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"/>
            </svg>
            <span class="hidden md:inline">Чати</span>
          </RouterLink>

          <RouterLink to="/orders"
            class="flex items-center gap-1.5 px-3 py-2 text-sm text-gray-500 hover:text-teal-600 hover:bg-teal-50 rounded-lg transition-all duration-200">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10"/>
            </svg>
            <span class="hidden md:inline">Замовлення</span>
          </RouterLink>

          <RouterLink to="/offers"
            class="relative flex items-center gap-1.5 px-3 py-2 text-sm text-gray-500 hover:text-teal-600 hover:bg-teal-50 rounded-lg transition-all duration-200">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"/>
            </svg>
            <span class="hidden md:inline">Торги</span>
            <span v-if="pendingOffersCount > 0"
              class="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-[10px] rounded-full flex items-center justify-center leading-none font-bold">
              {{ pendingOffersCount }}
            </span>
          </RouterLink>

          
          <Teleport to="body">
            <Transition name="fade">
              <div v-if="showNoPayoutModal"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                @click.self="showNoPayoutModal = false">
                <div class="bg-white rounded-2xl p-6 max-w-sm w-full shadow-xl text-center">
                  <div class="text-4xl mb-3">💳</div>
                  <h2 class="text-base font-bold text-gray-900 mb-2">Спочатку підключіть виплати</h2>
                  <p class="text-sm text-gray-500 mb-5">
                    Щоб публікувати оголошення, потрібно підключити отримання коштів
                    через <strong>Monobank SubMerchant</strong>.
                  </p>
                  <div class="flex gap-3">
                    <button @click="showNoPayoutModal = false"
                      class="btn-secondary flex-1">Скасувати</button>
                    <RouterLink to="/profile" @click="showNoPayoutModal = false"
                      class="btn-primary flex-1 block text-center">
                      Налаштувати →
                    </RouterLink>
                  </div>
                </div>
              </div>
            </Transition>
          </Teleport>

          <button @click="onSellClick"
            class="ml-1 btn-primary text-sm px-4 py-2 shadow-sm">
            + Продати
          </button>

          <RouterLink :to="`/users/${auth.userId}`"
            class="ml-1 flex items-center gap-2 rounded-full hover:bg-ivory-300 p-1 transition-all duration-200">
            <span class="relative shrink-0">
              <span :class="[
                'w-8 h-8 rounded-full bg-teal-100 text-teal-700 flex items-center justify-center font-semibold text-sm uppercase overflow-hidden shadow-sm',
                auth.hasDiia ? 'ring-2 ring-teal-500 ring-offset-1' : ''
              ]">
                <img v-if="auth.avatarUrl && !avatarError"
                  :src="auth.avatarUrl" class="w-full h-full object-cover"
                  @error="avatarError = true" />
                <span v-else>{{ (auth.firstName || auth.username)?.charAt(0) }}</span>
              </span>
              <span v-if="pendingReviewsCount > 0"
                class="absolute -top-1 -right-1 min-w-[16px] h-4 bg-teal-500 text-white text-[10px] rounded-full flex items-center justify-center font-bold px-1 leading-none">
                {{ pendingReviewsCount }}
              </span>
            </span>
            <span class="hidden lg:block text-sm font-medium text-gray-700 pr-1">
              {{ auth.displayName || auth.firstName || auth.username }}
            </span>
          </RouterLink>

          <button @click="handleLogout"
            class="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-all duration-200"
            title="Вийти">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
          </button>
        </template>

        <template v-else>
          <RouterLink to="/login" class="btn-secondary text-sm px-4 py-2">Увійти</RouterLink>
          <RouterLink to="/register" class="btn-primary text-sm px-4 py-2 shadow-sm">Реєстрація</RouterLink>
        </template>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { offersApi } from '@/api/offers'
import { reviewsApi } from '@/api/reviews'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const searchQuery = ref('')

watch(() => route.query.q, (q) => {
  searchQuery.value = (q as string) ?? ''
}, { immediate: true })

function submitSearch() {
  const q = normalize(searchQuery.value)
  router.push({ path: '/', query: q ? { q } : {} })
}

function clearSearch() {
  searchQuery.value = ''
  if (route.path === '/') router.push({ path: '/', query: {} })
}

function normalize(s: string): string {
  return s.trim().replace(/\s+/g, ' ')
}
const pendingOffersCount = ref(0)
const pendingReviewsCount = ref(0)
const avatarError = ref(false)
const showNoPayoutModal = ref(false)

function onSellClick() {
  if (!auth.isPayoutEnabled) {
    showNoPayoutModal.value = true
  } else {
    router.push('/ads/create')
  }
}
watch(() => auth.avatarUrl, () => { avatarError.value = false })

let pollInterval: ReturnType<typeof setInterval> | null = null

async function fetchPendingCount() {
  if (!auth.isAuthenticated) return
  try {
    const { data } = await offersApi.getPendingCount()
    pendingOffersCount.value = data.count
  } catch {  }
  try {
    const { data } = await reviewsApi.getMyPending()
    pendingReviewsCount.value = data.length
  } catch {  }
}

onMounted(() => { fetchPendingCount(); pollInterval = setInterval(fetchPendingCount, 60000) })
onUnmounted(() => { if (pollInterval) clearInterval(pollInterval) })

function handleLogout() {
  auth.logout()
  router.push('/')
}
</script>
