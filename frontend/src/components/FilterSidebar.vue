<template>
  <aside class="w-full">
    
    <div class="border-b border-gray-100 pb-4 mb-4">
      <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">Категорія</p>

      
      <div v-if="!activeMain" class="space-y-0.5">
        <button v-for="cat in CATEGORY_TREE" :key="cat.slug"
          @click="setMain(cat)"
          class="w-full flex items-center gap-2.5 px-2 py-2 rounded-lg text-sm text-gray-600 hover:bg-teal-50 hover:text-teal-700 text-left transition-all duration-150 group">
          <svg class="w-4 h-4 text-gray-400 group-hover:text-teal-500 shrink-0 transition-colors" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" :d="cat.icon" />
          </svg>
          <span>{{ cat.label }}</span>
          <svg class="ml-auto w-3.5 h-3.5 text-gray-300 group-hover:text-teal-400 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
      </div>

      
      <div v-else-if="!activeSub">
        <button @click="clearMain" class="flex items-center gap-1.5 text-teal-600 text-xs font-medium mb-3 hover:underline transition-colors">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          {{ activeMain.label }}
        </button>
        <div class="space-y-0.5">
          <button v-for="sub in activeMain.subs" :key="sub.slug"
            @click="setSub(sub)"
            class="w-full flex items-center justify-between px-2 py-2 rounded-lg text-sm text-left transition-all duration-150"
            :class="modelValue.categorySub === sub.slug
              ? 'text-teal-700 font-semibold bg-teal-50'
              : 'text-gray-600 hover:bg-teal-50 hover:text-teal-700'">
            <span>{{ sub.label }}</span>
            <svg class="w-3.5 h-3.5 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </button>
        </div>
      </div>

      
      <div v-else>
        <button @click="clearSub" class="flex items-center gap-1.5 text-teal-600 text-xs font-medium mb-3 hover:underline transition-colors">
          ‹ {{ activeSub.label }}
        </button>
        <div class="space-y-0.5">
          <button
            @click="clearItem"
            class="w-full flex items-center px-2 py-1.5 rounded-lg text-sm text-left transition-colors"
            :class="!modelValue.categoryItem ? 'text-teal-700 font-semibold bg-teal-50' : 'text-gray-700 hover:bg-gray-50'">
            Всі в «{{ activeSub.label }}»
          </button>
          <button v-for="item in activeSub.items" :key="item.slug"
            @click="setItem(item)"
            class="w-full flex items-center px-2 py-1.5 rounded-lg text-sm text-left transition-colors"
            :class="modelValue.categoryItem === item.slug ? 'text-teal-700 font-semibold bg-teal-50' : 'text-gray-700 hover:bg-gray-50'">
            {{ item.label }}
          </button>
        </div>
      </div>
    </div>

    
    <div class="border-b border-gray-100 pb-4 mb-4">
      <button @click="showCondition = !showCondition"
        class="w-full flex items-center justify-between text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
        Стан
        <span class="text-gray-400 font-normal text-base leading-none">{{ showCondition ? '−' : '+' }}</span>
      </button>
      <div v-if="showCondition" class="space-y-2">
        <label v-for="cond in CONDITIONS" :key="cond.slug"
          class="flex items-center gap-2.5 cursor-pointer group">
          <input type="checkbox"
            :value="cond.slug"
            :checked="selectedConditions.includes(cond.slug)"
            @change="toggleCondition(cond.slug)"
            class="w-4 h-4 rounded border-gray-300 text-teal-500 focus:ring-teal-400" />
          <span class="text-sm text-gray-700 group-hover:text-gray-900">{{ cond.label }}</span>
        </label>
      </div>
    </div>

    
    <div class="border-b border-gray-100 pb-4 mb-4">
      <button @click="showPrice = !showPrice"
        class="w-full flex items-center justify-between text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
        Ціна (₴)
        <span class="text-gray-400 font-normal text-base leading-none">{{ showPrice ? '−' : '+' }}</span>
      </button>
      <div v-if="showPrice" class="flex items-center gap-2">
        <input v-model.number="localPriceMin" type="number" placeholder="від" min="0"
          class="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:border-teal-400"
          @change="emitPriceRange" />
        <span class="text-gray-400 text-sm shrink-0">—</span>
        <input v-model.number="localPriceMax" type="number" placeholder="до" min="0"
          class="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:border-teal-400"
          @change="emitPriceRange" />
      </div>
    </div>

    
    <div class="border-b border-gray-100 pb-4 mb-4">
      <button @click="showBrand = !showBrand"
        class="w-full flex items-center justify-between text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
        Бренд
        <span class="text-gray-400 font-normal text-base leading-none">{{ showBrand ? '−' : '+' }}</span>
      </button>
      <div v-if="showBrand">
        <input v-model="localBrand" type="text" placeholder="Наприклад: Nike, Zara..."
          class="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:border-teal-400"
          @keydown.enter="emit('update:modelValue', { ...modelValue, brand: localBrand || undefined })" />
        <button v-if="localBrand" @click="applyBrand"
          class="mt-2 text-xs text-teal-600 hover:underline">Застосувати</button>
      </div>
    </div>

    
    <div v-if="availableSizes.length" class="border-b border-gray-100 pb-4 mb-4">
      <button @click="showSize = !showSize"
        class="w-full flex items-center justify-between text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
        Розмір
        <span class="text-gray-400 font-normal text-base leading-none">{{ showSize ? '−' : '+' }}</span>
      </button>
      <div v-if="showSize" class="flex flex-wrap gap-1.5">
        <button v-for="sz in availableSizes" :key="sz"
          @click="toggleSize(sz)"
          :class="['px-2.5 py-1 rounded-lg text-xs border transition-colors',
            selectedSizes.includes(sz)
              ? 'bg-teal-500 text-white border-teal-500'
              : 'border-gray-200 text-gray-600 hover:border-teal-400']">
          {{ sz }}
        </button>
      </div>
    </div>

    
    <div class="border-b border-gray-100 pb-4 mb-4">
      <button @click="showColor = !showColor"
        class="w-full flex items-center justify-between text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
        Колір
        <span class="text-gray-400 font-normal text-base leading-none">{{ showColor ? '−' : '+' }}</span>
      </button>
      <div v-if="showColor" class="flex flex-wrap gap-2">
        <button
          v-for="clr in COLORS" :key="clr.slug"
          @click="toggleColor(clr.slug)"
          :title="clr.label"
          :class="['relative w-7 h-7 rounded-full border-2 transition-all hover:scale-110 focus:outline-none',
            selectedColors.includes(clr.slug) ? 'border-teal-500 scale-110' : 'border-transparent']"
          :style="clr.slug === 'multicolor'
            ? 'background: conic-gradient(red,orange,yellow,green,blue,violet,red)'
            : `background-color:${clr.hex}`">
          <span v-if="selectedColors.includes(clr.slug)"
            class="absolute inset-0 flex items-center justify-center text-white text-[10px] font-bold drop-shadow">✓</span>
        </button>
      </div>
      <p v-if="selectedColors.length" class="text-xs text-teal-600 mt-2">
        Обрано: {{ selectedColors.map(s => COLORS.find(c=>c.slug===s)?.label).join(', ') }}
      </p>
    </div>

    
    <button v-if="hasAnyFilter" @click="resetAll"
      class="w-full py-2 text-sm text-red-500 hover:text-red-700 border border-red-200 rounded-lg hover:bg-red-50 transition-colors">
      Скинути всі фільтри
    </button>
  </aside>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { CATEGORY_TREE, CONDITIONS, COLORS, getSizesForSub, type CategoryMain, type CategorySub as Sub, type CategoryItem } from '@/data/categories'

export interface FilterState {
  category?: string
  categorySub?: string
  categoryItem?: string
  conditions?: string[]
  brand?: string
  priceMin?: number
  priceMax?: number
  sizes?: string[]
  colors?: string[]
}

const props = defineProps<{ modelValue: FilterState }>()
const emit = defineEmits<{ 'update:modelValue': [FilterState] }>()

const activeMain = ref<CategoryMain | null>(
  CATEGORY_TREE.find(c => c.slug === props.modelValue.category) ?? null
)
const activeSub = ref<Sub | null>(
  activeMain.value?.subs.find(s => s.slug === props.modelValue.categorySub) ?? null
)

const showCondition = ref(true)
const showPrice = ref(true)
const showBrand = ref(true)
const showSize = ref(true)
const showColor = ref(true)

const selectedConditions = ref<string[]>(props.modelValue.conditions ?? [])
const selectedSizes = ref<string[]>(props.modelValue.sizes ?? [])
const selectedColors = ref<string[]>(props.modelValue.colors ?? [])
const localPriceMin = ref<number | undefined>(props.modelValue.priceMin)
const localPriceMax = ref<number | undefined>(props.modelValue.priceMax)
const localBrand = ref(props.modelValue.brand ?? '')

const availableSizes = computed(() => getSizesForSub(props.modelValue.categorySub))

const hasAnyFilter = computed(() =>
  !!(props.modelValue.category || props.modelValue.conditions?.length ||
     props.modelValue.brand || props.modelValue.priceMin || props.modelValue.priceMax ||
     props.modelValue.sizes?.length || props.modelValue.colors?.length))

watch(() => props.modelValue, val => {
  activeMain.value = CATEGORY_TREE.find(c => c.slug === val.category) ?? null
  activeSub.value = activeMain.value?.subs.find(s => s.slug === val.categorySub) ?? null
  selectedConditions.value = val.conditions ?? []
  selectedSizes.value = val.sizes ?? []
  selectedColors.value = val.colors ?? []
  localPriceMin.value = val.priceMin
  localPriceMax.value = val.priceMax
  localBrand.value = val.brand ?? ''
}, { deep: true })

function setMain(cat: CategoryMain) {
  activeMain.value = cat
  activeSub.value = null
  emit('update:modelValue', { ...props.modelValue, category: cat.slug, categorySub: undefined, categoryItem: undefined })
}

function setSub(sub: Sub) {
  activeSub.value = sub
  emit('update:modelValue', { ...props.modelValue, categorySub: sub.slug, categoryItem: undefined, sizes: [] })
  selectedSizes.value = []
}

function setItem(item: CategoryItem) {
  emit('update:modelValue', { ...props.modelValue, categoryItem: item.slug })
}

function clearItem() {
  emit('update:modelValue', { ...props.modelValue, categoryItem: undefined })
}

function clearSub() {
  activeSub.value = null
  emit('update:modelValue', { ...props.modelValue, categorySub: undefined, categoryItem: undefined, sizes: [] })
  selectedSizes.value = []
}

function clearMain() {
  activeMain.value = null
  activeSub.value = null
  emit('update:modelValue', { ...props.modelValue, category: undefined, categorySub: undefined, categoryItem: undefined, sizes: [] })
  selectedSizes.value = []
}

function toggleCondition(slug: string) {
  const idx = selectedConditions.value.indexOf(slug)
  if (idx >= 0) selectedConditions.value.splice(idx, 1)
  else selectedConditions.value.push(slug)
  emit('update:modelValue', { ...props.modelValue, conditions: [...selectedConditions.value] })
}

function toggleSize(sz: string) {
  const idx = selectedSizes.value.indexOf(sz)
  if (idx >= 0) selectedSizes.value.splice(idx, 1)
  else selectedSizes.value.push(sz)
  emit('update:modelValue', { ...props.modelValue, sizes: [...selectedSizes.value] })
}

function emitPriceRange() {
  emit('update:modelValue', {
    ...props.modelValue,
    priceMin: localPriceMin.value || undefined,
    priceMax: localPriceMax.value || undefined
  })
}

function applyBrand() {
  emit('update:modelValue', { ...props.modelValue, brand: localBrand.value || undefined })
}

function toggleColor(slug: string) {
  const idx = selectedColors.value.indexOf(slug)
  if (idx >= 0) selectedColors.value.splice(idx, 1)
  else selectedColors.value.push(slug)
  emit('update:modelValue', { ...props.modelValue, colors: [...selectedColors.value] })
}

function resetAll() {
  activeMain.value = null
  activeSub.value = null
  selectedConditions.value = []
  selectedSizes.value = []
  selectedColors.value = []
  localPriceMin.value = undefined
  localPriceMax.value = undefined
  localBrand.value = ''
  emit('update:modelValue', {})
}
</script>
