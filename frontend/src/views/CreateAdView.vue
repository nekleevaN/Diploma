<template>
  <div class="max-w-xl mx-auto px-4 py-8">
    <RouterLink to="/" class="text-sm text-teal-600 hover:underline">← Назад</RouterLink>
    <div class="card p-8 mt-4">
      <h1 class="text-xl font-bold text-gray-900 mb-6">Нове оголошення</h1>

      <form @submit.prevent="submit" class="space-y-5">
        
        <CategoryPicker @selected="onCategorySelected" />

        <div v-if="categoryData">
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Стан <span class="text-red-500">*</span>
            </label>
            <div class="grid grid-cols-1 gap-2">
              <label v-for="cond in CONDITIONS" :key="cond.slug"
                :class="['flex items-center gap-3 p-3 border rounded-xl cursor-pointer transition-colors',
                  form.condition === cond.slug
                    ? 'border-teal-400 bg-teal-50'
                    : 'border-gray-200 hover:border-gray-300']">
                <input type="radio" v-model="form.condition" :value="cond.slug" class="text-teal-500" required />
                <span class="text-sm font-medium text-gray-800">{{ cond.label }}</span>
              </label>
            </div>
          </div>

          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">Заголовок <span class="text-red-500">*</span></label>
            <input v-model="form.title" type="text" class="input" placeholder="Наприклад: iPhone 13 Pro 128GB" required maxlength="200" />
          </div>

          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">Бренд</label>
            <input v-model="form.brand" type="text" class="input" placeholder="Наприклад: Nike, Apple, H&M..." maxlength="100" />
          </div>

          
          <div v-if="availableSizes.length" class="mt-4">
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

          
          <div class="mt-4">
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

          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">Ціна (₴) <span class="text-red-500">*</span></label>
            <input v-model.number="form.price" type="number" class="input" placeholder="0" required min="1" />
          </div>

          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">Опис <span class="text-red-500">*</span></label>
            <textarea v-model="form.description" class="input resize-none" rows="5"
              placeholder="Детально опишіть товар: стан, характеристики, умови продажу..." required maxlength="5000" />
          </div>

          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">Фото товару</label>
            <ImageUpload folder="ads" :multiple="true" @uploaded="pendingImages = $event" />
            <p v-if="pendingImages.length > 0" class="text-xs text-teal-500 mt-1.5 flex items-center gap-1">
              <AppIcon name="check-circle" size="w-3.5 h-3.5" class="inline" /> {{ pendingImages.length }} фото буде додано після публікації
            </p>
          </div>

          
          <div class="mt-4">
            <LocationPicker @selected="onLocationSelected" @cleared="location = null" />
          </div>

          <div v-if="error" class="mt-4 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</div>

          <button type="submit" class="btn-primary w-full mt-5" :disabled="loading">
            <span v-if="loading">Публікуємо...</span>
            <span v-else>Опублікувати оголошення</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { catalogApi } from '@/api/catalog'
import ImageUpload from '@/components/ImageUpload.vue'
import LocationPicker from '@/components/LocationPicker.vue'
import CategoryPicker from '@/components/CategoryPicker.vue'
import AppIcon from '@/components/AppIcon.vue'
import { CONDITIONS, COLORS, getSizesForSub } from '@/data/categories'

const router = useRouter()

const form = ref({
  title: '', price: 0, description: '',
  condition: '', brand: '', size: '', color: ''
})
const categoryData = ref<{ category: string; categorySub?: string; categoryItem?: string; categoryLabel: string } | null>(null)
const loading = ref(false)
const error = ref('')
const pendingImages = ref<string[]>([])
const location = ref<{ lat: number; lng: number; address: string } | null>(null)

const availableSizes = computed(() => getSizesForSub(categoryData.value?.categorySub))

function onCategorySelected(data: typeof categoryData.value) {
  categoryData.value = data
  form.value.size = ''
}

function onLocationSelected(loc: { lat: number; lng: number; address: string }) {
  location.value = loc
}

async function submit() {
  if (!categoryData.value) return
  error.value = ''
  loading.value = true
  try {
    const { data } = await catalogApi.create({
      title: form.value.title,
      description: form.value.description,
      price: form.value.price,
      category: categoryData.value.category,
      categorySub: categoryData.value.categorySub,
      categoryItem: categoryData.value.categoryItem,
      categoryLabel: categoryData.value.categoryLabel,
      condition: form.value.condition || undefined,
      brand: form.value.brand || undefined,
      size: form.value.size || undefined,
      color: form.value.color || undefined,
      latitude: location.value?.lat,
      longitude: location.value?.lng,
      locationAddress: location.value?.address
    })
    const adId = data.advertisementId
    for (const url of pendingImages.value) {
      await catalogApi.addImage(adId, url)
    }
    router.push(`/ads/${adId}`)
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Помилка публікації'
  } finally {
    loading.value = false
  }
}
</script>
