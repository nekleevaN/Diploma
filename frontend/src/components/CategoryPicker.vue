<template>
  <div>
    <label class="block text-sm font-medium text-gray-700 mb-2">Категорія</label>

    
    <div class="flex items-center gap-2 text-xs text-gray-400 mb-3">
      <span :class="step >= 1 ? 'text-teal-600 font-medium' : ''">Розділ</span>
      <span>›</span>
      <span :class="step >= 2 ? 'text-teal-600 font-medium' : ''">Категорія</span>
      <span>›</span>
      <span :class="step >= 3 ? 'text-teal-600 font-medium' : ''">Вид</span>
    </div>

    
    <div v-if="selectedMain" class="flex items-center gap-2 mb-3 flex-wrap">
      <button @click="reset" class="flex items-center gap-1.5 text-xs bg-teal-50 text-teal-700 px-2.5 py-1 rounded-full border border-teal-200 hover:bg-teal-100">
        <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" :d="selectedMain.icon" />
        </svg>
        {{ selectedMain.label }}
        <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
      </button>
      <span v-if="selectedSub" class="text-gray-400">›</span>
      <button v-if="selectedSub" @click="resetSub" class="flex items-center gap-1 text-xs bg-teal-50 text-teal-700 px-2.5 py-1 rounded-full border border-teal-200 hover:bg-teal-100">
        {{ selectedSub.label }} ✕
      </button>
      <span v-if="selectedItem" class="text-gray-400">›</span>
      <span v-if="selectedItem" class="text-xs bg-teal-500 text-white px-2.5 py-1 rounded-full">
        {{ selectedItem.label }}
      </span>
    </div>

    
    <div v-if="step === 1" class="grid grid-cols-2 sm:grid-cols-4 gap-2">
      <button
        v-for="cat in CATEGORY_TREE" :key="cat.slug"
        @click="selectMain(cat)"
        class="flex flex-col items-center gap-2 p-3 border border-ivory-400 rounded-xl hover:border-teal-400 hover:bg-teal-50 transition-all duration-150 text-center group">
        <svg class="w-6 h-6 text-gray-400 group-hover:text-teal-600 transition-colors" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" :d="cat.icon" />
        </svg>
        <span class="text-xs font-medium text-gray-700 leading-tight">{{ cat.label }}</span>
      </button>
    </div>

    
    <div v-if="step === 2 && selectedMain" class="space-y-1">
      <button
        v-for="sub in selectedMain.subs" :key="sub.slug"
        @click="selectSub(sub)"
        class="w-full flex items-center justify-between px-4 py-2.5 border border-gray-200 rounded-xl hover:border-teal-400 hover:bg-teal-50 transition-colors text-left">
        <span class="text-sm text-gray-800">{{ sub.label }}</span>
        <span class="text-gray-400 text-xs">{{ sub.items.length }} →</span>
      </button>
    </div>

    
    <div v-if="step === 3 && selectedSub" class="space-y-1">
      
      <button @click="skipItem"
        class="w-full flex items-center px-4 py-2.5 border border-dashed border-gray-200 rounded-xl hover:border-teal-400 hover:bg-teal-50 transition-colors text-left">
        <span class="text-sm text-gray-500 italic">Загальна — {{ selectedSub.label }}</span>
      </button>
      <button
        v-for="item in selectedSub.items" :key="item.slug"
        @click="selectItem(item)"
        class="w-full flex items-center px-4 py-2.5 border border-gray-200 rounded-xl hover:border-teal-400 hover:bg-teal-50 transition-colors text-left">
        <span class="text-sm text-gray-800">{{ item.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { CATEGORY_TREE, type CategoryMain, type CategorySub as Sub, type CategoryItem } from '@/data/categories'

const emit = defineEmits<{
  selected: [{ category: string; categorySub?: string; categoryItem?: string; categoryLabel: string }]
}>()

const props = withDefaults(defineProps<{
  initialCategory?: string
  initialCategorySub?: string
  initialCategoryItem?: string
}>(), {})

const selectedMain = ref<CategoryMain | null>(
  CATEGORY_TREE.find(c => c.slug === props.initialCategory) ?? null
)
const selectedSub = ref<Sub | null>(
  selectedMain.value?.subs.find(s => s.slug === props.initialCategorySub) ?? null
)
const selectedItem = ref<CategoryItem | null>(
  selectedSub.value?.items.find(i => i.slug === props.initialCategoryItem) ?? null
)

const step = computed(() => {
  if (!selectedMain.value) return 1
  if (!selectedSub.value) return 2
  if (!selectedItem.value) return 3
  return 3
})

function selectMain(cat: CategoryMain) {
  selectedMain.value = cat
  selectedSub.value = null
  selectedItem.value = null
}

function selectSub(sub: Sub) {
  selectedSub.value = sub
  selectedItem.value = null
}

function selectItem(item: CategoryItem) {
  selectedItem.value = item
  const label = `${selectedMain.value!.label} / ${selectedSub.value!.label} / ${item.label}`
  emit('selected', {
    category: selectedMain.value!.slug,
    categorySub: selectedSub.value!.slug,
    categoryItem: item.slug,
    categoryLabel: label
  })
}

function skipItem() {
  selectedItem.value = null
  const label = `${selectedMain.value!.label} / ${selectedSub.value!.label}`
  emit('selected', {
    category: selectedMain.value!.slug,
    categorySub: selectedSub.value!.slug,
    categoryLabel: label
  })
}

function reset() {
  selectedMain.value = null
  selectedSub.value = null
  selectedItem.value = null
}

function resetSub() {
  selectedSub.value = null
  selectedItem.value = null
}
</script>
