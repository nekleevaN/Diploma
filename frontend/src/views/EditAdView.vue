<template>
  <div class="max-w-xl mx-auto px-4 py-8">
    <RouterLink :to="`/ads/${adId}`" class="text-sm text-teal-600 hover:underline">← Назад до оголошення</RouterLink>

    <div v-if="loading" class="card p-8 mt-4 animate-pulse space-y-4">
      <div class="h-6 bg-gray-200 rounded w-1/2" />
      <div class="h-10 bg-gray-200 rounded" />
      <div class="h-24 bg-gray-200 rounded" />
    </div>

    <div v-else class="card p-8 mt-4">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-xl font-bold text-gray-900">Редагування оголошення</h1>
        <button @click="showDeleteConfirm = true"
          class="text-sm text-red-500 hover:text-red-700 hover:bg-red-50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1">
          <AppIcon name="trash" size="w-4 h-4" /> Видалити
        </button>
      </div>

      <form @submit.prevent="save" class="space-y-5">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Заголовок</label>
          <input v-model="form.title" type="text" class="input" required maxlength="200" />
        </div>

        
        <CategoryPicker
          :initialCategory="form.category"
          :initialCategorySub="form.categorySub"
          :initialCategoryItem="form.categoryItem"
          @selected="onCategorySelected"
        />

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Стан</label>
          <div class="grid grid-cols-1 gap-2">
            <label v-for="cond in CONDITIONS" :key="cond.slug"
              :class="['flex items-center gap-3 p-3 border rounded-xl cursor-pointer transition-colors',
                form.condition === cond.slug
                  ? 'border-teal-400 bg-teal-50'
                  : 'border-gray-200 hover:border-gray-300']">
              <input type="radio" v-model="form.condition" :value="cond.slug" class="text-teal-500" />
              <span class="text-sm font-medium text-gray-800">{{ cond.label }}</span>
            </label>
          </div>
        </div>

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Бренд</label>
          <input v-model="form.brand" type="text" class="input" placeholder="Наприклад: Nike, Apple..." maxlength="100" />
        </div>

        
        <div v-if="availableSizes.length">
          <label class="block text-sm font-medium text-gray-700 mb-2">Розмір</label>
          <div class="flex flex-wrap gap-2">
            <button v-for="sz in availableSizes" :key="sz" type="button"
              @click="form.size = form.size === sz ? '' : sz"
              :class="['px-3 py-1.5 rounded-lg text-sm border transition-colors',
                form.size === sz
                  ? 'bg-teal-500 text-white border-teal-500'
                  : 'border-gray-200 text-gray-600 hover:border-teal-400']">
              {{ sz }}
            </button>
          </div>
        </div>

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Колір</label>
          <div class="flex flex-wrap gap-2">
            <button v-for="clr in COLORS" :key="clr.slug" type="button"
              @click="form.color = form.color === clr.slug ? '' : clr.slug"
              :title="clr.label"
              :class="['w-7 h-7 rounded-full border-2 transition-transform hover:scale-110',
                form.color === clr.slug ? 'border-teal-500 scale-110' : 'border-transparent']"
              :style="clr.slug === 'multicolor'
                ? 'background: conic-gradient(red,orange,yellow,green,blue,violet,red); border-width:2px'
                : `background-color:${clr.hex}`">
            </button>
            <button v-if="form.color" type="button" @click="form.color = ''"
              class="text-xs text-gray-400 hover:text-gray-600 underline self-center ml-1">
              Скинути
            </button>
          </div>
        </div>

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Ціна (₴)</label>
          <input v-model.number="form.price" type="number" class="input" required min="1" />
        </div>

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Опис</label>
          <textarea v-model="form.description" class="input resize-none" rows="6" required maxlength="5000" />
        </div>

        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Фото товару</label>
          <div v-if="existingImages.length" class="flex flex-wrap gap-2 mb-3">
            <div v-for="(url, i) in existingImages" :key="i" class="relative">
              <img :src="url" class="w-20 h-20 object-cover rounded-lg border border-gray-200" />
              <button type="button" @click="removeImage(i)"
                class="absolute -top-1.5 -right-1.5 w-5 h-5 bg-red-500 text-white rounded-full text-xs flex items-center justify-center hover:bg-red-600">
                ×
              </button>
            </div>
          </div>
          <ImageUpload folder="ads" :multiple="true" @uploaded="onNewImages" />
        </div>

        
        <LocationPicker
          :initial-lat="location?.lat"
          :initial-lng="location?.lng"
          @selected="location = $event"
          @cleared="clearLocation = true; location = null"
        />

        <div v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</div>

        <div class="flex gap-3">
          <button type="submit" class="btn-primary flex-1" :disabled="saving">
            {{ saving ? 'Зберігаємо...' : 'Зберегти зміни' }}
          </button>
          <RouterLink :to="`/ads/${adId}`" class="btn-secondary flex-1 text-center">
            Скасувати
          </RouterLink>
        </div>
      </form>
    </div>

    
    <Teleport to="body">
      <div v-if="showDeleteConfirm"
        class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4"
        @click.self="showDeleteConfirm = false">
        <div class="card w-full max-w-sm p-6 text-center">
          <div class="flex justify-center mb-3"><AppIcon name="trash" size="w-10 h-10" class="text-red-500" /></div>
          <h2 class="text-lg font-bold text-gray-900 mb-2">Видалити оголошення?</h2>
          <p class="text-sm text-gray-500 mb-5">Оголошення буде знято з публікації. Цю дію не можна скасувати.</p>
          <div class="flex gap-3">
            <button @click="deleteAd" :disabled="deleting"
              class="flex-1 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50">
              {{ deleting ? '...' : 'Так, видалити' }}
            </button>
            <button @click="showDeleteConfirm = false" class="btn-secondary flex-1">
              Скасувати
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { catalogApi } from '@/api/catalog'
import { useAuthStore } from '@/stores/auth'
import ImageUpload from '@/components/ImageUpload.vue'
import LocationPicker from '@/components/LocationPicker.vue'
import CategoryPicker from '@/components/CategoryPicker.vue'
import AppIcon from '@/components/AppIcon.vue'
import { CONDITIONS, COLORS, getSizesForSub } from '@/data/categories'
import type { Ad } from '@/types'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const adId = route.params.id as string

const loading = ref(true)
const saving = ref(false)
const deleting = ref(false)
const error = ref('')
const showDeleteConfirm = ref(false)
const existingImages = ref<string[]>([])

const form = ref({
  title: '', description: '', price: 0,
  category: '', categorySub: undefined as string | undefined,
  categoryItem: undefined as string | undefined,
  categoryLabel: undefined as string | undefined,
  condition: '', brand: '', size: '', color: ''
})
const location = ref<{ lat: number; lng: number; address: string } | null>(null)
const clearLocation = ref(false)

const availableSizes = computed(() => getSizesForSub(form.value.categorySub))

function onCategorySelected(data: { category: string; categorySub?: string; categoryItem?: string; categoryLabel: string }) {
  form.value.category = data.category
  form.value.categorySub = data.categorySub
  form.value.categoryItem = data.categoryItem
  form.value.categoryLabel = data.categoryLabel
  form.value.size = ''
}

function removeImage(index: number) { existingImages.value.splice(index, 1) }

function onNewImages(urls: string[]) {
  urls.forEach(url => catalogApi.addImage(adId, url))
}

async function save() {
  error.value = ''
  saving.value = true
  try {
    await catalogApi.updateAd(adId, {
      title: form.value.title,
      description: form.value.description,
      price: form.value.price,
      category: form.value.category,
      categorySub: form.value.categorySub,
      categoryItem: form.value.categoryItem,
      categoryLabel: form.value.categoryLabel,
      condition: form.value.condition || undefined,
      brand: form.value.brand || undefined,
      size: form.value.size || undefined,
      color: form.value.color || undefined,
      latitude: location.value?.lat,
      longitude: location.value?.lng,
      locationAddress: location.value?.address,
      clearLocation: clearLocation.value
    })
    router.push(`/ads/${adId}`)
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка збереження'
  } finally {
    saving.value = false
  }
}

async function deleteAd() {
  deleting.value = true
  try {
    await catalogApi.deleteAd(adId)
    router.push(`/users/${auth.userId}`)
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка видалення'
    showDeleteConfirm.value = false
  } finally { deleting.value = false }
}

onMounted(async () => {
  try {
    const { data } = await catalogApi.getById(adId)
    const ad = data as Ad & { imageUrls?: string[] }

    if (ad.sellerId !== auth.userId) {
      router.push(`/ads/${adId}`)
      return
    }

    form.value = {
      title: ad.title,
      description: ad.description,
      price: ad.price,
      category: ad.category,
      categorySub: ad.categorySub ?? undefined,
      categoryItem: ad.categoryItem ?? undefined,
      categoryLabel: ad.categoryLabel ?? undefined,
      condition: ad.condition ?? '',
      brand: ad.brand ?? '',
      size: ad.size ?? '',
      color: ad.color ?? ''
    }
    existingImages.value = ad.imageUrls ?? []
    if (ad.latitude && ad.longitude) {
      location.value = { lat: ad.latitude, lng: ad.longitude, address: ad.locationAddress ?? '' }
    }
  } finally {
    loading.value = false
  }
})
</script>
