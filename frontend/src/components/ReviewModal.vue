<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4 py-8 overflow-y-auto"
      @click.self="$emit('close')">
      <div class="bg-white rounded-2xl shadow-xl w-full max-w-md my-auto">

        
        <div class="flex items-center justify-between px-5 pt-5 pb-3 border-b border-ivory-400">
          <div>
            <h2 class="text-base font-bold text-gray-900">
              {{ reviewType === 'BuyerToSeller' ? 'Відгук про продавця' : 'Відгук про покупця' }}
            </h2>
            <p class="text-xs text-gray-400 mt-0.5 truncate max-w-xs">{{ adTitle }}</p>
          </div>
          <button @click="$emit('close')" class="text-gray-400 hover:text-gray-600 shrink-0">
            <AppIcon name="x" size="w-5 h-5" />
          </button>
        </div>

        <div class="px-5 py-4 space-y-5">

          
          <div>
            <p class="text-sm font-medium text-gray-700 mb-2">Загальна оцінка *</p>
            <div class="flex gap-1">
              <button v-for="i in 5" :key="i"
                @click="rating = i"
                @mouseenter="hoverRating = i"
                @mouseleave="hoverRating = 0"
                type="button"
                class="transition-transform hover:scale-110">
                <svg :class="['w-9 h-9 transition-colors', (hoverRating || rating) >= i ? 'text-teal-500' : 'text-gray-200']"
                  fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                </svg>
              </button>
            </div>
            <p v-if="submitted && !rating" class="text-xs text-red-600 mt-1">Оберіть оцінку</p>
          </div>

          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1.5">
              Коментар
              <span v-if="rating > 0 && rating < 3" class="text-red-600"> *</span>
              <span v-else class="text-gray-400 text-xs font-normal"> (необов'язково)</span>
            </label>
            <textarea
              v-model="comment"
              rows="3"
              maxlength="500"
              :placeholder="rating < 3 && rating > 0 ? 'Розкажіть що саме пішло не так (мінімум 20 символів)...' : 'Поділіться враженнями...'"
              :class="['input resize-none text-sm', submitted && commentError ? 'border-red-400 focus:border-red-500' : '']" />
            <div class="flex justify-between mt-1">
              <p v-if="submitted && commentError" class="text-xs text-red-600">{{ commentError }}</p>
              <span v-else />
              <span class="text-xs text-gray-400 ml-auto">{{ comment.length }}/500</span>
            </div>
          </div>

          
          <div>
            <button @click="showCriteria = !showCriteria" type="button"
              class="flex items-center gap-2 text-sm text-teal-600 hover:text-teal-700">
              <AppIcon :name="showCriteria ? 'chevron-down' : 'chevron-right'" size="w-4 h-4" />
              Детальні критерії (опціонально)
            </button>

            <div v-if="showCriteria" class="mt-3 space-y-3">
              <div v-for="c in criteria" :key="c.key">
                <div class="flex items-center justify-between mb-1">
                  <p class="text-xs font-medium text-gray-600">{{ c.label }}</p>
                  <span v-if="c.model.value" class="text-xs text-teal-600 font-medium">
                    {{ c.model.value }}/5
                  </span>
                </div>
                <div class="flex gap-1">
                  <button v-for="i in 5" :key="i"
                    @click="c.model.value = c.model.value === i ? null : i"
                    type="button"
                    class="transition-transform hover:scale-110">
                    <svg :class="['w-6 h-6 transition-colors', (c.model.value ?? 0) >= i ? 'text-teal-500' : 'text-gray-200']"
                      fill="currentColor" viewBox="0 0 24 24">
                      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          
          <label class="flex items-center gap-3 cursor-pointer">
            <input type="checkbox" v-model="isAnonymous" class="rounded border-gray-300" />
            <span class="text-sm text-gray-600">
              Залишити анонімно
              <span class="text-xs text-gray-400 block">Ваше ім'я не буде відображатись у відгуку, рейтинг враховується</span>
            </span>
          </label>

          
          <p v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</p>
        </div>

        
        <div class="px-5 pb-5 flex gap-3">
          <button @click="$emit('close')" class="btn-secondary flex-1">Скасувати</button>
          <button @click="submit" :disabled="!rating || submitting"
            class="flex-1 py-2.5 rounded-xl text-sm font-semibold text-white transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 review-submit-btn">
            <AppIcon v-if="submitting" name="refresh" size="w-4 h-4 animate-spin" />
            {{ submitting ? 'Публікуємо...' : 'Опублікувати' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { reviewsApi } from '@/api/reviews'
import AppIcon from './AppIcon.vue'

const props = defineProps<{
  reviewId: string
  reviewType: string
  adTitle: string
}>()

const emit = defineEmits<{ close: []; submitted: [] }>()

const rating = ref(0)
const hoverRating = ref(0)
const comment = ref('')
const isAnonymous = ref(false)
const showCriteria = ref(false)
const submitted = ref(false)
const submitting = ref(false)
const error = ref('')

const descriptionAccuracy = ref<number | null>(null)
const shippingSpeed = ref<number | null>(null)
const communication = ref<number | null>(null)

const criteria = [
  { key: 'desc',  label: 'Відповідність опису',  model: descriptionAccuracy },
  { key: 'ship',  label: 'Швидкість відправки',   model: shippingSpeed },
  { key: 'comm',  label: 'Комунікація',            model: communication },
]

const commentError = computed(() => {
  if (rating.value > 0 && rating.value < 3) {
    if (!comment.value.trim()) return 'Обов\'язковий для оцінки нижче 3 зірок'
    if (comment.value.trim().length < 20) return 'Мінімум 20 символів'
  }
  if (comment.value.trim().length > 0 && comment.value.trim().length < 10)
    return 'Мінімум 10 символів'
  return ''
})

async function submit() {
  submitted.value = true
  if (!rating.value || commentError.value) return

  submitting.value = true
  error.value = ''
  try {
    await reviewsApi.submit(props.reviewId, {
      rating: rating.value,
      comment: comment.value.trim() || undefined,
      isAnonymous: isAnonymous.value,
      descriptionAccuracy: descriptionAccuracy.value ?? undefined,
      shippingSpeed: shippingSpeed.value ?? undefined,
      communication: communication.value ?? undefined,
    })
    emit('submitted')
  } catch (e: any) {
    error.value = e?.response?.data?.error ?? 'Помилка публікації. Спробуйте ще раз.'
    submitting.value = false
  }
}
</script>

<style scoped>
.review-submit-btn { background-color: #708238; }
.review-submit-btn:hover:not(:disabled) { background-color: #5d6c2e; }
</style>
