<template>
  <RouterLink :to="`/ads/${ad.id}`" class="group block">
    
    <div class="aspect-square bg-ivory-300 rounded-xl overflow-hidden relative mb-2.5 shadow-sm group-hover:shadow-md transition-shadow duration-200">
      <img v-if="firstImage" :src="firstImage" :alt="ad.title"
        class="w-full h-full object-cover group-hover:scale-[1.03] transition-transform duration-300 ease-out" />
      <div v-else class="w-full h-full flex flex-col items-center justify-center text-gray-300">
        <svg class="w-10 h-10 mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
        <span class="text-xs">Фото відсутнє</span>
      </div>

      
      <span class="absolute top-2 left-2 bg-white/80 backdrop-blur-sm rounded-lg w-7 h-7 flex items-center justify-center shadow-sm">
        <svg class="w-3.5 h-3.5 text-gray-600" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" :d="categoryIcon" />
        </svg>
      </span>

      
      <span v-if="ad.condition"
        class="absolute bottom-2 left-2 text-[10px] font-semibold px-2 py-0.5 rounded-full bg-teal-600 text-white shadow">
        {{ conditionShort }}
      </span>

      
      <span v-if="ad.color && colorHex"
        class="absolute bottom-2 right-2 w-3.5 h-3.5 rounded-full border-2 border-white shadow"
        :style="ad.color === 'multicolor'
          ? 'background: conic-gradient(red,orange,yellow,green,blue,violet,red)'
          : `background-color:${colorHex}`">
      </span>
    </div>

    
    <div class="px-0.5 space-y-0.5">
      
      <div class="flex items-center gap-2">
        <p class="text-sm font-bold text-gray-900">₴{{ ad.price.toLocaleString() }}</p>
        <span v-if="ad.size"
          class="text-[11px] font-medium px-1.5 py-0.5 border border-gray-300 rounded text-gray-500 leading-tight">
          {{ ad.size }}
        </span>
      </div>

      
      <p v-if="ad.brand" class="text-[11px] font-semibold text-teal-700 uppercase tracking-wide">{{ ad.brand }}</p>

      
      <p class="text-xs text-gray-500 line-clamp-2 leading-tight">{{ ad.title }}</p>

      
      <p class="text-[11px] text-gray-400">{{ ad.sellerName }}</p>
    </div>
  </RouterLink>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { AdListItem } from '@/types'
import { CATEGORY_ICONS, COLORS, getConditionBadge } from '@/data/categories'

const props = defineProps<{ ad: AdListItem }>()

const categoryIcon = computed(() => CATEGORY_ICONS[props.ad.category] ?? CATEGORY_ICONS['default'])
const firstImage = computed(() => props.ad.imageUrls?.[0] ?? null)

const conditionBadge = computed(() => getConditionBadge(props.ad.condition))
const conditionShort = computed(() => ({
  new_with_tags:    'Нове з ярл.',
  new_without_tags: 'Нове',
  very_good:        'Дуже гарний',
  good:             'Гарний стан',
  satisfactory:     'Задовільний',
}[props.ad.condition ?? ''] ?? ''))

const colorHex = computed(() => COLORS.find(c => c.slug === props.ad.color)?.hex ?? null)
</script>
