<template>
  <div class="mt-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Відгуки</h2>

    
    <div class="card p-5 mb-4">
      <div class="flex flex-col sm:flex-row gap-5">

        
        <div class="flex-1">
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">Як продавець</p>
          <div v-if="stats?.asSeller.count" class="flex items-center gap-3">
            <span class="text-3xl font-bold text-gray-900">{{ stats.asSeller.average.toFixed(1) }}</span>
            <div>
              <StarRating :rating="stats.asSeller.average" />
              <p class="text-xs text-gray-400 mt-0.5">{{ stats.asSeller.count }} відгуків</p>
            </div>
          </div>
          <p v-else class="text-sm text-gray-400 italic">Ще немає відгуків</p>

          <div v-if="stats?.asSeller.count" class="mt-3 space-y-1">
            <div v-for="n in [5,4,3,2,1]" :key="n" class="flex items-center gap-2">
              <span class="text-xs text-gray-500 w-2">{{ n }}</span>
              <svg class="w-3 h-3 text-teal-500 shrink-0" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
              </svg>
              <div class="flex-1 bg-gray-100 rounded-full h-1.5 overflow-hidden">
                <div class="h-full bg-teal-500 rounded-full transition-all duration-500"
                  :style="{ width: barWidth(stats.asSeller, n) }" />
              </div>
              <span class="text-xs text-gray-400 w-4 text-right">{{ stats.asSeller.distribution[n] ?? 0 }}</span>
            </div>
          </div>
        </div>

        <div class="w-px bg-ivory-400 hidden sm:block" />

        
        <div class="flex-1">
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">Як покупець</p>
          <div v-if="stats?.asBuyer.count" class="flex items-center gap-3">
            <span class="text-3xl font-bold text-gray-900">{{ stats.asBuyer.average.toFixed(1) }}</span>
            <div>
              <StarRating :rating="stats.asBuyer.average" />
              <p class="text-xs text-gray-400 mt-0.5">{{ stats.asBuyer.count }} відгуків</p>
            </div>
          </div>
          <p v-else class="text-sm text-gray-400 italic">Ще немає відгуків</p>

          <div v-if="stats?.asBuyer.count" class="mt-3 space-y-1">
            <div v-for="n in [5,4,3,2,1]" :key="n" class="flex items-center gap-2">
              <span class="text-xs text-gray-500 w-2">{{ n }}</span>
              <svg class="w-3 h-3 text-teal-500 shrink-0" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
              </svg>
              <div class="flex-1 bg-gray-100 rounded-full h-1.5 overflow-hidden">
                <div class="h-full bg-teal-500 rounded-full transition-all duration-500"
                  :style="{ width: barWidth(stats.asBuyer, n) }" />
              </div>
              <span class="text-xs text-gray-400 w-4 text-right">{{ stats.asBuyer.distribution[n] ?? 0 }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    
    <div class="flex items-center justify-between mb-4 flex-wrap gap-2">
      <div class="flex border-b border-gray-200">
        <button v-for="tab in reviewTabs" :key="tab.value"
          @click="activeType = tab.value; page = 1; loadReviews()"
          :class="['px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
            activeType === tab.value ? 'border-teal-500 text-teal-600' : 'border-transparent text-gray-500 hover:text-gray-700']">
          {{ tab.label }}
          <span class="ml-1 text-xs text-gray-400">
            ({{ tab.value === 'seller' ? (stats?.asSeller.count ?? 0) : (stats?.asBuyer.count ?? 0) }})
          </span>
        </button>
      </div>

      <AppSelect
        v-model="sort"
        :options="sortOptions"
        @change="page = 1; loadReviews()" />
    </div>

    
    <div v-if="loadingReviews" class="space-y-3">
      <div v-for="i in 3" :key="i" class="card p-4 animate-pulse space-y-2">
        <div class="flex gap-3">
          <div class="w-10 h-10 bg-gray-200 rounded-full shrink-0" />
          <div class="flex-1 space-y-2">
            <div class="h-3 bg-gray-200 rounded w-1/3" />
            <div class="h-3 bg-gray-200 rounded w-1/2" />
          </div>
        </div>
      </div>
    </div>

    <div v-else-if="reviews.length === 0" class="text-center py-10 text-gray-400">
      <div class="flex justify-center mb-2">
        <svg class="w-8 h-8 text-gray-300" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
        </svg>
      </div>
      <p class="text-sm">Відгуків ще немає</p>
    </div>

    <div v-else class="space-y-3">
      <div v-for="r in reviews" :key="r.reviewId" class="card p-4">
        <div class="flex items-start gap-3">
          
          <div class="w-10 h-10 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center text-sm font-bold uppercase shrink-0">
            {{ r.isAnonymous ? '?' : (r.reviewerName?.charAt(0) ?? '?') }}
          </div>

          <div class="flex-1 min-w-0">
            <div class="flex items-center justify-between gap-2 flex-wrap">
              <span class="text-sm font-medium text-gray-900">
                {{ r.isAnonymous ? 'Анонімний користувач' : (r.reviewerName ?? 'Користувач') }}
              </span>
              <span class="text-xs text-gray-400">{{ formatDate(r.publishedAt) }}</span>
            </div>

            <StarRating :rating="r.rating" class="mt-1" />

            <p v-if="r.comment" class="text-sm text-gray-600 mt-2 leading-relaxed">{{ r.comment }}</p>

            
            <div v-if="r.descriptionAccuracy || r.shippingSpeed || r.communication"
              class="mt-2 flex flex-wrap gap-3">
              <span v-if="r.descriptionAccuracy" class="text-xs text-gray-500">
                Опис: <span class="font-medium text-gray-700">{{ r.descriptionAccuracy }}/5</span>
              </span>
              <span v-if="r.shippingSpeed" class="text-xs text-gray-500">
                Відправка: <span class="font-medium text-gray-700">{{ r.shippingSpeed }}/5</span>
              </span>
              <span v-if="r.communication" class="text-xs text-gray-500">
                Комунікація: <span class="font-medium text-gray-700">{{ r.communication }}/5</span>
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    
    <div v-if="totalCount > pageSize" class="flex justify-center gap-2 mt-5">
      <button v-for="p in totalPages" :key="p"
        @click="page = p; loadReviews()"
        :class="['w-8 h-8 rounded-lg text-sm transition-colors',
          page === p ? 'bg-teal-500 text-white' : 'bg-ivory-200 text-gray-600 hover:bg-teal-50']">
        {{ p }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { reviewsApi, type ReviewDto, type UserRatingStatsDto, type RatingStatsDto } from '@/api/reviews'
import AppSelect from './AppSelect.vue'
import StarRating from './StarRating.vue'

const props = defineProps<{ userId: string }>()

const stats = ref<UserRatingStatsDto | null>(null)
const reviews = ref<ReviewDto[]>([])
const totalCount = ref(0)
const loadingReviews = ref(false)
const activeType = ref('seller')
const sort = ref('newest')
const page = ref(1)
const pageSize = 20

const reviewTabs = [
  { value: 'seller', label: 'Як продавець' },
  { value: 'buyer',  label: 'Як покупець'  },
]

const sortOptions = [
  { value: 'newest',  label: 'Новіші'        },
  { value: 'oldest',  label: 'Старіші'       },
  { value: 'highest', label: 'Найвища оцінка' },
  { value: 'lowest',  label: 'Найнижча оцінка'},
]

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize))

function barWidth(s: RatingStatsDto, n: number) {
  if (!s.count) return '0%'
  return `${Math.round(((s.distribution[n] ?? 0) / s.count) * 100)}%`
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' })
}

async function loadReviews() {
  loadingReviews.value = true
  try {
    const { data } = await reviewsApi.getUserReviews(props.userId, {
      type: activeType.value, page: page.value, pageSize, sort: sort.value
    })
    reviews.value = data.items
    totalCount.value = data.totalCount
  } catch {  }
  finally { loadingReviews.value = false }
}

onMounted(async () => {
  try {
    const { data } = await reviewsApi.getRatingStats(props.userId)
    stats.value = data
  } catch {  }
  loadReviews()
})
</script>
