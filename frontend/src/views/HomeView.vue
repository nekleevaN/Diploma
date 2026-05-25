<template>
  <div class="min-h-screen">
    
    <div class="bg-ivory-100 border-b border-ivory-400">
      <div class="max-w-7xl mx-auto px-4 py-2 flex items-center gap-3">
        
        <button @click="drawerOpen = true"
          class="md:hidden flex items-center gap-2 px-3.5 py-2 border border-ivory-400 rounded-xl text-sm text-gray-600 bg-ivory-100 hover:bg-ivory-200 relative transition-all duration-200">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4h18M7 12h10M11 20h2"/>
          </svg>
          Фільтри
          <span v-if="activeFilterCount > 0"
            class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] bg-teal-500 text-white text-[10px] rounded-full flex items-center justify-center font-bold px-1">
            {{ activeFilterCount }}
          </span>
        </button>
        
        <span v-if="search" class="inline-flex items-center gap-1.5 pl-3 pr-2 py-1 bg-teal-500 text-white rounded-full text-xs font-medium shadow-sm">
          «{{ search }}»
          <button @click="clearSearch" class="w-4 h-4 flex items-center justify-center rounded-full hover:bg-white/20 transition-colors">✕</button>
        </span>
      </div>

      
      <div v-if="activeChips.length" class="max-w-7xl mx-auto px-4 pb-3 flex flex-wrap gap-2">
        <span v-for="chip in activeChips" :key="chip.key"
          class="inline-flex items-center gap-1.5 pl-3 pr-2 py-1 bg-teal-500 text-white rounded-full text-xs font-medium shadow-sm">
          {{ chip.label }}
          <button @click="removeChip(chip.key)"
            class="w-4 h-4 flex items-center justify-center rounded-full hover:bg-white/20 transition-colors leading-none">✕</button>
        </span>
        <button v-if="activeChips.length > 1" @click="resetFilters"
          class="text-xs text-gray-400 hover:text-teal-600 underline self-center transition-colors">
          Скинути всі
        </button>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-4 py-6 flex gap-6">
      
      <aside class="hidden md:block w-64 shrink-0">
        <div class="card p-4 sticky top-20">
          <FilterSidebar v-model="filters" />
        </div>
      </aside>

      
      <main class="flex-1 min-w-0">
        
        <div class="flex items-center justify-between mb-4">
          <p class="text-sm text-gray-500">
            <span v-if="!loading">
              <span class="font-semibold text-gray-800">{{ totalItems }}</span> {{ pluralAds(totalItems) }}
            </span>
            <span v-else class="animate-pulse">Завантаження...</span>
          </p>
          <div class="flex items-center gap-2">
            <label class="text-xs text-gray-400 hidden sm:block">Сортування:</label>
            <AppSelect
              v-model="sortBy"
              :options="[
                { value: 'newest',    label: 'Новіші' },
                { value: 'price_asc', label: 'Ціна: зростання' },
                { value: 'price_desc',label: 'Ціна: спадання' },
              ]"
              @change="loadAds(1)"
            />
          </div>
        </div>

        
        <div class="relative">
          
          <Transition name="fade">
            <div v-if="loading && ads.length === 0"
              class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
              <div v-for="i in 12" :key="i" class="animate-pulse">
                <div class="aspect-square bg-ivory-300 rounded-xl mb-2" />
                <div class="h-4 bg-ivory-300 rounded w-2/3 mb-1.5" />
                <div class="h-3 bg-ivory-300 rounded w-1/2 mb-1" />
                <div class="h-3 bg-ivory-300 rounded w-3/4" />
              </div>
            </div>
          </Transition>

          
          <div v-if="loading && ads.length > 0"
            class="absolute inset-0 bg-ivory-200/60 backdrop-blur-[1px] rounded-xl z-10 flex items-center justify-center">
            <div class="w-8 h-8 border-2 border-teal-500 border-t-transparent rounded-full animate-spin"></div>
          </div>

          
          <Transition name="fade">
            <div v-if="!loading && ads.length === 0"
              class="text-center py-20 card">
              <svg class="w-14 h-14 text-gray-200 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
              </svg>
              <p class="text-gray-500 font-medium">Нічого не знайдено</p>
              <p class="text-gray-400 text-sm mt-1">Спробуйте змінити запит або фільтри</p>
              <button v-if="activeFilterCount > 0" @click="resetFilters"
                class="mt-4 text-sm text-teal-600 hover:underline font-medium">
                Скинути фільтри
              </button>
            </div>
          </Transition>

          
          <Transition name="fade">
            <div v-if="!loading && ads.length > 0"
              class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
              <AdCard v-for="ad in ads" :key="ad.id" :ad="ad" />
            </div>
          </Transition>
        </div>

        
        <div v-if="totalPages > 1" class="flex justify-center items-center gap-2 mt-8">
          <button @click="loadAds(page - 1)" :disabled="page === 1"
            class="btn-secondary px-4 py-2 text-sm disabled:opacity-40">← Назад</button>
          <span class="text-sm text-gray-500 px-2">{{ page }} / {{ totalPages }}</span>
          <button @click="loadAds(page + 1)" :disabled="page === totalPages"
            class="btn-secondary px-4 py-2 text-sm disabled:opacity-40">Далі →</button>
        </div>
      </main>
    </div>

    
    <Teleport to="body">
      <div v-if="drawerOpen" class="fixed inset-0 z-50 flex">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="drawerOpen = false" />
        <div class="relative ml-auto w-80 max-w-full h-full bg-ivory-100 overflow-y-auto flex flex-col shadow-2xl">
          <div class="flex items-center justify-between px-4 py-4 border-b border-ivory-400">
            <h2 class="font-semibold text-gray-900">Фільтри</h2>
            <button @click="drawerOpen = false" class="text-gray-400 hover:text-gray-600 text-xl leading-none">✕</button>
          </div>
          <div class="p-4 flex-1 overflow-y-auto">
            <FilterSidebar v-model="filters" />
          </div>
          <div class="p-4 border-t border-gray-100">
            <button @click="drawerOpen = false" class="btn-primary w-full">
              Показати {{ totalItems }} {{ pluralAds(totalItems) }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { catalogApi } from '@/api/catalog'
import AdCard from '@/components/AdCard.vue'
import FilterSidebar, { type FilterState } from '@/components/FilterSidebar.vue'
import AppSelect from '@/components/AppSelect.vue'
import { getConditionLabel, getCategoryLabel, COLORS, getColorLabel } from '@/data/categories'
import type { AdListItem } from '@/types'

const route = useRoute()
const router = useRouter()

const ads = ref<AdListItem[]>([])
const loading = ref(true)
const search = ref('')
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const sortBy = ref('newest')
const drawerOpen = ref(false)

const filters = ref<FilterState>({})

watch(() => route.query.q, (q) => {
  search.value = normalize((q as string) ?? '')
  loadAds(1)
}, { immediate: true })

watch(filters, () => loadAds(1), { deep: true })
watch(sortBy, () => loadAds(1))

function normalize(s: string): string {
  return s.trim().replace(/\s+/g, ' ').toLowerCase()
}

function clearSearch() {
  search.value = ''
  router.push({ path: '/', query: { ...route.query, q: undefined } })
  loadAds(1)
}

const activeFilterCount = computed(() => {
  let n = 0
  if (filters.value.category) n++
  if (filters.value.conditions?.length) n++
  if (filters.value.brand) n++
  if (filters.value.priceMin || filters.value.priceMax) n++
  if (filters.value.sizes?.length) n++
  if (filters.value.colors?.length) n++
  return n
})

const activeChips = computed(() => {
  const chips: { key: string; label: string }[] = []
  const { category, categorySub, categoryItem } = filters.value
  if (category) {
    const label = getCategoryLabel(category, categorySub, categoryItem)
    const key = categoryItem ? 'categoryItem' : categorySub ? 'categorySub' : 'category'
    chips.push({ key, label })
  }
  filters.value.conditions?.forEach(c =>
    chips.push({ key: `cond:${c}`, label: getConditionLabel(c) }))
  if (filters.value.brand)
    chips.push({ key: 'brand', label: `Бренд: ${filters.value.brand}` })
  if (filters.value.priceMin || filters.value.priceMax) {
    const from = filters.value.priceMin ? `від ₴${filters.value.priceMin}` : ''
    const to = filters.value.priceMax ? `до ₴${filters.value.priceMax}` : ''
    chips.push({ key: 'price', label: [from, to].filter(Boolean).join(' ') })
  }
  filters.value.sizes?.forEach(s =>
    chips.push({ key: `size:${s}`, label: `Розмір ${s}` }))
  filters.value.colors?.forEach(c =>
    chips.push({ key: `color:${c}`, label: getColorLabel(c) }))
  return chips
})

function removeChip(key: string) {
  if (key === 'category')     { filters.value = { ...filters.value, category: undefined, categorySub: undefined, categoryItem: undefined } }
  else if (key === 'categorySub') { filters.value = { ...filters.value, categorySub: undefined, categoryItem: undefined } }
  else if (key === 'categoryItem') { filters.value = { ...filters.value, categoryItem: undefined } }
  else if (key === 'brand')   { filters.value = { ...filters.value, brand: undefined } }
  else if (key === 'price')   { filters.value = { ...filters.value, priceMin: undefined, priceMax: undefined } }
  else if (key.startsWith('cond:')) {
    const slug = key.slice(5)
    filters.value = { ...filters.value, conditions: filters.value.conditions?.filter(c => c !== slug) }
  } else if (key.startsWith('size:')) {
    const sz = key.slice(5)
    filters.value = { ...filters.value, sizes: filters.value.sizes?.filter(s => s !== sz) }
  } else if (key.startsWith('color:')) {
    const c = key.slice(6)
    filters.value = { ...filters.value, colors: filters.value.colors?.filter(x => x !== c) }
  }
}

function resetFilters() {
  filters.value = {}
  clearSearch()
}

async function loadAds(newPage = 1) {
  loading.value = true
  page.value = newPage
  try {
    const f = filters.value
    const { data } = await catalogApi.getAll({
      search: search.value || undefined,
      category: f.category,
      categorySub: f.categorySub,
      categoryItem: f.categoryItem,
      condition: f.conditions?.length ? f.conditions.join(',') : undefined,
      brand: f.brand,
      priceMin: f.priceMin,
      priceMax: f.priceMax,
      size: f.sizes?.length ? f.sizes.join(',') : undefined,
      color: f.colors?.length ? f.colors.join(',') : undefined,
      sortBy: sortBy.value !== 'newest' ? sortBy.value : undefined,
      page: newPage,
      pageSize: 20
    })
    ads.value = data.items
    totalItems.value = data.total
    totalPages.value = Math.ceil(data.total / data.pageSize) || 1
  } finally {
    loading.value = false
  }
}

function pluralAds(n: number): string {
  const mod10 = n % 10
  const mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return 'оголошення'
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return 'оголошення'
  return 'оголошень'
}

</script>
