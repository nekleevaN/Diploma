<template>
  <div class="max-w-3xl mx-auto px-4 py-8">
    <RouterLink to="/" class="text-sm text-teal-600 hover:underline">← Назад</RouterLink>

    <div v-if="loading" class="card p-8 mt-4 animate-pulse space-y-4">
      <div class="flex gap-4">
        <div class="w-20 h-20 bg-gray-200 rounded-full shrink-0" />
        <div class="flex-1 space-y-2">
          <div class="h-5 bg-gray-200 rounded w-1/3" />
          <div class="h-4 bg-gray-200 rounded w-1/4" />
        </div>
      </div>
    </div>

    <template v-else-if="profile">
      
      <div class="card p-6 mt-4">
        <div class="flex items-start gap-5 flex-wrap">

          
          <div class="relative shrink-0">
            <div :class="[
              'w-24 h-24 rounded-full overflow-hidden bg-teal-50 text-teal-600 flex items-center justify-center text-3xl font-bold uppercase',
              isDiiaVerified ? 'ring-4 ring-teal-500 ring-offset-2' : ''
            ]">
              <img v-if="profile.avatarUrl" :src="profile.avatarUrl" class="w-full h-full object-cover" :alt="profile.displayName || profile.username" />
              <span v-else>{{ (profile.firstName || profile.username)?.charAt(0) }}</span>
            </div>
            
            <label v-if="isOwnProfile"
              class="absolute inset-0 rounded-full bg-black/40 flex items-center justify-center cursor-pointer opacity-0 hover:opacity-100 transition-opacity"
              title="Змінити фото">
              <AppIcon name="camera" size="w-4 h-4" class="text-white" />
              <input type="file" class="hidden" accept="image/*" @change="handleAvatarUpload" :disabled="avatarUploading" />
            </label>
            <div v-if="avatarUploading" class="absolute inset-0 rounded-full bg-black/50 flex items-center justify-center">
              <span class="text-white text-xs">...</span>
            </div>
          </div>

          
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-2 flex-wrap">
              <div>
                <h1 class="text-xl font-bold text-gray-900">
                  {{ profile.displayName || profile.username }}
                </h1>
                <p class="text-xs text-gray-400 mt-0.5">@{{ profile.username }}</p>
                <div class="flex items-center gap-1.5 mt-1">
                  <AppIcon name="star" size="w-3.5 h-3.5" class="text-yellow-400 fill-yellow-400" :stroke-width="0" />
                  <span class="text-sm font-semibold">{{ profile.rating.toFixed(1) }}</span>
                  <span class="text-gray-300">·</span>
                  <span class="text-xs text-gray-400">На платформі з {{ formatDate(profile.joinedAt) }}</span>
                </div>
              </div>
              <button v-if="isOwnProfile && !editingBio" @click="startEditBio"
                class="btn-secondary text-xs py-1 px-3 flex items-center gap-1">
                <AppIcon name="edit" size="w-3.5 h-3.5" /> Редагувати
              </button>
            </div>

            
            <div class="mt-3">
              <template v-if="editingBio">
                <textarea v-model="bioInput" class="input resize-none text-sm" rows="3"
                  placeholder="Розкажіть про себе (до 500 символів)..." maxlength="500" />
                
                <div class="mt-2 p-3 bg-teal-50 rounded-xl space-y-3">
                  <p class="text-xs font-medium text-teal-700 flex items-center gap-1">
                    <AppIcon name="shield" size="w-3.5 h-3.5" /> Довірена особа (для безпеки переглядів)
                  </p>
                  <p class="text-xs text-teal-600 leading-relaxed">
                    Вкажи контакт довіреної особи — вона отримає сповіщення коли ти йдеш на перегляд.
                    Можна вказати <strong>email</strong> або <strong>Telegram ID</strong> (або обидва).
                  </p>

                  
                  <div>
                    <label class="block text-xs font-medium text-gray-600 mb-1">📧 Email довіреної особи</label>
                    <input v-model="trustedEmailInput" type="email"
                      class="input text-sm" placeholder="trusted@example.com" />
                    <p v-if="trustedEmailInput" class="mt-1 text-xs text-teal-500 flex items-center gap-1">
                      <AppIcon name="check-circle" size="w-3.5 h-3.5" /> Отримає email-сповіщення перед переглядами
                    </p>
                  </div>

                  
                  <div>
                    <label class="block text-xs font-medium text-gray-600 mb-1">✈️ Telegram ID довіреної особи</label>
                    <div class="bg-white rounded-lg p-2 space-y-1 mb-1.5">
                      <p class="text-xs text-gray-600">1. Вона пише боту <strong>/start</strong></p>
                      <p class="text-xs text-gray-600">2. Бот відповідає її ID — вона надсилає тобі</p>
                      <a :href="`https://t.me/${botUsername}`" target="_blank"
                        class="inline-flex items-center gap-1 text-xs text-teal-600 hover:underline font-medium">
                        <AppIcon name="arrow-right" size="w-3.5 h-3.5" /> Відкрити бота →
                      </a>
                    </div>
                    <input v-model.number="trustedTelegramInput" type="number"
                      class="input text-sm" placeholder="485086927" />
                    <p v-if="trustedTelegramInput" class="mt-1 text-xs text-teal-500 flex items-center gap-1">
                      <AppIcon name="check-circle" size="w-3.5 h-3.5" /> Отримає Telegram-сповіщення
                    </p>
                  </div>
                </div>

                
                <div class="mt-2 p-3 bg-gray-50 rounded-xl space-y-2">
                  <p class="text-xs font-medium text-gray-700 flex items-center gap-1">
                    💳 Monobank SubMerchant ID
                  </p>
                  <p class="text-xs text-gray-500">
                    Необхідний для отримання виплат від покупців. Отримайте в
                    <a href="https://business.monobank.ua" target="_blank" class="text-teal-600 underline">Monobank Business</a>.
                  </p>
                  <input v-model="subMerchantInput" type="text" class="input text-sm font-mono"
                    placeholder="sub_merchant_abc123" />
                  <p v-if="subMerchantInput" class="text-xs text-teal-500 flex items-center gap-1">
                    <AppIcon name="check-circle" size="w-3.5 h-3.5" />
                    {{ auth.isPayoutEnabled ? 'Буде оновлено' : 'Виплати будуть підключені' }}
                  </p>
                  <button v-if="auth.isPayoutEnabled && !subMerchantInput"
                    @click="subMerchantInput = '__remove__'"
                    class="text-xs text-red-400 hover:text-red-600 underline">
                    Відключити виплати
                  </button>
                </div>

                <div class="flex gap-2 mt-2">
                  <button @click="saveBio" :disabled="savingBio" class="btn-primary text-xs py-1.5 px-3">
                    {{ savingBio ? '...' : 'Зберегти' }}
                  </button>
                  <button @click="cancelEditBio" class="btn-secondary text-xs py-1.5 px-3">Скасувати</button>
                </div>
              </template>
              <p v-else-if="profile.bio" class="text-sm text-gray-600 leading-relaxed">{{ profile.bio }}</p>
              <p v-else-if="isOwnProfile" class="text-sm text-gray-400 italic">Додайте опис про себе...</p>
            </div>

            
            <div class="flex flex-wrap gap-1.5 mt-3">
              <span v-for="badge in profile.badges" :key="badge"
                class="text-xs px-2 py-0.5 bg-teal-150 text-teal-700 border border-teal-300 rounded-full">
                {{ badgeLabel(badge) }}
              </span>
            </div>

          </div>
        </div>

        
        <div v-if="isOwnProfile" class="mt-5 border-t pt-5">
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide mb-3">Верифікація особи</p>
          <DiiaVerification @verified="onDiiaVerified" />
        </div>
      </div>

      
      <div class="mt-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-gray-900">
            {{ isOwnProfile ? 'Мої оголошення' : `Оголошення продавця` }}
          </h2>
          <template v-if="isOwnProfile">
            <button v-if="auth.isPayoutEnabled"
              @click="$router.push('/ads/create')"
              class="btn-primary text-xs py-1.5 px-3">
              + Додати
            </button>
            <button v-else @click="showNoPayoutAlert = true"
              class="btn-primary text-xs py-1.5 px-3 opacity-80">
              + Додати
            </button>
          </template>

          
          <Teleport to="body">
            <Transition name="fade">
              <div v-if="showNoPayoutAlert"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                @click.self="showNoPayoutAlert = false">
                <div class="bg-white rounded-2xl p-6 max-w-sm w-full shadow-xl text-center">
                  <div class="text-4xl mb-3">💳</div>
                  <h2 class="text-base font-bold text-gray-900 mb-2">Спочатку підключіть виплати</h2>
                  <p class="text-sm text-gray-500 mb-5">
                    Щоб публікувати оголошення, потрібно підключити отримання коштів через
                    <strong>Monobank SubMerchant</strong>.
                  </p>
                  <div class="flex gap-3">
                    <button @click="showNoPayoutAlert = false" class="btn-secondary flex-1">Скасувати</button>
                    <RouterLink to="/profile" @click="showNoPayoutAlert = false"
                      class="btn-primary flex-1 block text-center">
                      Налаштувати →
                    </RouterLink>
                  </div>
                </div>
              </div>
            </Transition>
          </Teleport>
        </div>
        <div v-if="loadingAds" class="grid grid-cols-2 sm:grid-cols-3 gap-4">
          <div v-for="i in 3" :key="i" class="card animate-pulse">
            <div class="h-36 bg-gray-200 rounded-t-xl" />
            <div class="p-3 space-y-2">
              <div class="h-3 bg-gray-200 rounded w-3/4" />
              <div class="h-4 bg-gray-200 rounded w-1/2" />
            </div>
          </div>
        </div>
        <div v-else-if="ads.length === 0" class="text-center py-10 text-gray-400">
          <div class="flex justify-center mb-2"><AppIcon name="tag" size="w-8 h-8" /></div>
          <p class="text-sm">{{ isOwnProfile ? 'У вас ще немає оголошень' : 'Немає активних оголошень' }}</p>
        </div>
        <div v-else class="grid grid-cols-2 sm:grid-cols-3 gap-4">
          <AdCard v-for="ad in ads" :key="ad.id" :ad="ad" />
        </div>
      </div>

      
      <UserReviews :user-id="(route.params.id as string)" />

    </template>

    <div v-else class="text-center py-16 text-gray-400">
      <div class="flex justify-center mb-3"><AppIcon name="user" size="w-10 h-10" /></div>
      <p>Користувача не знайдено</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { usersApi, type UserProfile } from '@/api/users'
import { catalogApi } from '@/api/catalog'
import { useAuthStore } from '@/stores/auth'
import AdCard from '@/components/AdCard.vue'
import DiiaVerification from '@/components/DiiaVerification.vue'
import UserReviews from '@/components/UserReviews.vue'
import AppIcon from '@/components/AppIcon.vue'
import type { AdListItem } from '@/types'

const route = useRoute()
const auth = useAuthStore()

const profile = ref<UserProfile | null>(null)
const ads = ref<AdListItem[]>([])
const loading = ref(true)
const loadingAds = ref(false)

const editingBio = ref(false)
const bioInput = ref('')
const trustedTelegramInput = ref<number | null>(null)
const trustedEmailInput = ref('')
const subMerchantInput = ref('')
const savingBio = ref(false)
const avatarUploading = ref(false)
const showNoPayoutAlert = ref(false)

const isOwnProfile = computed(() => auth.userId === route.params.id)
const isDiiaVerified = computed(() => profile.value?.badges?.includes('DiiaVerified') ?? false)
const botUsername = 'Trust_Market_Test_Bot'

function onDiiaVerified() {
  if (profile.value) {
    profile.value = { ...profile.value, badges: [...profile.value.badges, 'DiiaVerified'] }
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' })
}

function badgeLabel(badge: string) {
  return { EmailVerified: 'Email', PhoneVerified: 'Телефон', DiiaVerified: '🇺🇦 Дія', TrustedSeller: 'Надійний' }[badge] ?? badge
}

function startEditBio() {
  bioInput.value = profile.value?.bio ?? ''
  trustedTelegramInput.value = auth.trustedContactTelegramId
  trustedEmailInput.value = auth.trustedContactEmail ?? ''
  subMerchantInput.value = profile.value?.monobankSubMerchantId ?? ''
  editingBio.value = true
}

function cancelEditBio() {
  editingBio.value = false
}

async function saveBio() {
  savingBio.value = true
  try {
    await usersApi.updateProfile(
      bioInput.value,
      trustedTelegramInput.value ?? undefined,
      trustedEmailInput.value || undefined)
    if (profile.value) profile.value = { ...profile.value, bio: bioInput.value }
    if (trustedTelegramInput.value) auth.setTrustedTelegram(trustedTelegramInput.value)
    auth.setTrustedEmail(trustedEmailInput.value || null)

    const currentSubMerchant = profile.value?.monobankSubMerchantId ?? ''
    const newSubMerchant = subMerchantInput.value === '__remove__' ? null : subMerchantInput.value.trim() || null
    if ((newSubMerchant ?? '') !== currentSubMerchant) {
      const { data } = await usersApi.setPayoutMethod(newSubMerchant)
      auth.setPayoutEnabled(data.payoutEnabled)
      if (profile.value) profile.value = { ...profile.value, monobankSubMerchantId: data.monobankSubMerchantId }
    }

    editingBio.value = false
  } finally {
    savingBio.value = false
  }
}

async function handleAvatarUpload(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  avatarUploading.value = true
  try {
    const { data } = await usersApi.uploadAvatar(file)
    if (profile.value) profile.value = { ...profile.value, avatarUrl: data.avatarUrl }
    auth.updateAvatar(data.avatarUrl)
  } finally {
    avatarUploading.value = false
  }
}

onMounted(async () => {
  const userId = route.params.id as string
  try {
    const profileRes = await usersApi.getProfile(userId)
    profile.value = profileRes.data
  } catch {
    loading.value = false
    return
  } finally {
    loading.value = false
  }

  loadingAds.value = true
  try {
    const adsRes = await catalogApi.getAll({ page: 1, pageSize: 100 })
    ads.value = adsRes.data.items.filter((a: AdListItem) => a.sellerId === userId)
  } finally {
    loadingAds.value = false
  }
})
</script>
