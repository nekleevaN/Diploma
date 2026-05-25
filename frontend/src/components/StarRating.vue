<template>
  <div :class="['flex gap-0.5', sizeClass]">
    <svg v-for="i in 5" :key="i"
      :class="starClass(i)"
      fill="currentColor" viewBox="0 0 24 24">
      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
    </svg>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  rating: number
  size?: 'sm' | 'md' | 'lg'
}>(), { size: 'md' })

const sizeClass = { sm: 'gap-0.5', md: 'gap-0.5', lg: 'gap-1' }[props.size]

function starClass(i: number) {
  const base = { sm: 'w-3 h-3', md: 'w-4 h-4', lg: 'w-5 h-5' }[props.size]
  const color = props.rating >= i
    ? 'text-teal-500'
    : props.rating >= i - 0.5
      ? 'text-teal-300'
      : 'text-gray-200'
  return `${base} ${color}`
}
</script>
