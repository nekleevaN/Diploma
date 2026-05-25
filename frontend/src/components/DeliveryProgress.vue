<template>
  <div class="py-2">
    <div class="flex items-center gap-1 mb-2">
      <span class="text-xs font-medium text-gray-600">Статус доставки:</span>
      <span v-if="description" class="text-xs text-gray-700">{{ description }}</span>
    </div>
    <div class="flex items-center gap-1">
      <template v-for="(step, i) in steps" :key="step.id">
        
        <div class="flex flex-col items-center">
          <div :class="[
            'w-7 h-7 rounded-full flex items-center justify-center text-sm transition-all',
            getStepClass(step.id)
          ]">
            <span v-if="isCompleted(step.id)">✓</span>
            <span v-else-if="isCurrent(step.id)" class="animate-pulse">●</span>
            <span v-else class="text-gray-300">○</span>
          </div>
          <p :class="['text-xs mt-1 text-center w-14 leading-tight', isCurrent(step.id) ? 'text-teal-600 font-medium' : isCompleted(step.id) ? 'text-teal-600' : 'text-gray-400']">
            {{ step.label }}
          </p>
        </div>
        
        <div v-if="i < steps.length - 1"
          :class="['flex-1 h-0.5 mb-5 transition-colors', isLineCompleted(step.id) ? 'bg-teal-400' : 'bg-gray-200']" />
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ status: string; description?: string | null }>()

const steps = [
  { id: 'TTNCreated',  label: 'ТТН' },
  { id: 'AtWarehouse', label: 'Здано НП' },
  { id: 'InTransit',   label: 'В дорозі' },
  { id: 'Arrived',     label: 'Прибуло' },
  { id: 'Received',    label: 'Отримано' },
]

const statusOrder = ['TTNCreated', 'AtWarehouse', 'InTransit', 'Arrived', 'Received']

const currentIndex = computed(() => {
  const idx = statusOrder.indexOf(props.status)
  return idx >= 0 ? idx : 0
})

const isFinalStatus = computed(() => props.status === 'Received')

function isCompleted(stepId: string) {
  if (isFinalStatus.value) return true
  return statusOrder.indexOf(stepId) < currentIndex.value
}

function isCurrent(stepId: string) {
  if (isFinalStatus.value) return false
  return statusOrder.indexOf(stepId) === currentIndex.value
}

function isLineCompleted(stepId: string) {
  if (isFinalStatus.value) return true
  return statusOrder.indexOf(stepId) < currentIndex.value
}

function getStepClass(stepId: string) {
  if (isCompleted(stepId)) return 'bg-teal-500 text-white'
  if (isCurrent(stepId)) return 'bg-teal-500 text-white'
  return 'bg-gray-100 text-gray-400'
}
</script>
