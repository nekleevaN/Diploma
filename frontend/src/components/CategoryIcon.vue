<template>
  <svg
    :class="cls"
    fill="none"
    stroke="currentColor"
    :stroke-width="strokeWidth"
    viewBox="0 0 24 24"
    xmlns="http://www.w3.org/2000/svg"
    aria-hidden="true"
  >
    <path stroke-linecap="round" stroke-linejoin="round" :d="iconPath" />
  </svg>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { CATEGORY_ICONS } from '@/data/categories'

const props = withDefaults(defineProps<{
  slug: string
  size?: string
  strokeWidth?: number
}>(), {
  size: 'w-5 h-5',
  strokeWidth: 1.5
})

const cls = computed(() => props.size)
const iconPath = computed(() => CATEGORY_ICONS[props.slug] ?? CATEGORY_ICONS['default'])
</script>
