<template>
  <div :class="[
    'rounded-full flex items-center justify-center font-bold uppercase overflow-hidden bg-teal-50 text-teal-600 shrink-0',
    sizeClass,
    verified ? ringClass : ''
  ]">
    <img v-if="avatarUrl" :src="(avatarUrl as string)" :alt="(username as string)" class="w-full h-full object-cover" />
    <span v-else>{{ username?.charAt(0) ?? '?' }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  username?: string | null
  avatarUrl?: string | null
  size?: 'sm' | 'md' | 'lg' | 'xl'
  verified?: boolean
}>()

const sizeClass = computed(() => ({
  sm: 'w-7 h-7 text-xs',
  md: 'w-10 h-10 text-sm',
  lg: 'w-16 h-16 text-lg',
  xl: 'w-24 h-24 text-2xl'
}[props.size ?? 'md']))

const ringClass = computed(() => ({
  sm: 'ring-2 ring-teal-500 ring-offset-1',
  md: 'ring-2 ring-teal-500 ring-offset-1',
  lg: 'ring-3 ring-teal-500 ring-offset-2',
  xl: 'ring-4 ring-teal-500 ring-offset-2'
}[props.size ?? 'md']))
</script>
