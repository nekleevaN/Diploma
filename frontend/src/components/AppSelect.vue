<template>
  <div class="relative" ref="rootEl">
    
    <button
      type="button"
      @click="toggle"
      @keydown="onKeydown"
      :aria-expanded="open"
      aria-haspopup="listbox"
      :class="[
        'flex items-center gap-1.5 border rounded-lg px-2.5 py-1.5 text-sm bg-ivory-100 cursor-pointer transition-all duration-200 select-none',
        open
          ? 'border-teal-500 ring-2 ring-teal-400/20'
          : 'border-ivory-400 hover:border-teal-400'
      ]"
    >
      <span class="text-gray-700">{{ selectedLabel }}</span>
      <AppIcon
        name="chevron-down"
        size="w-3.5 h-3.5"
        :class="['text-gray-400 transition-transform duration-200 shrink-0', open ? 'rotate-180' : '']"
        :stroke-width="2.5"
      />
    </button>

    
    <Transition
      enter-active-class="transition duration-100 ease-out"
      enter-from-class="opacity-0 scale-95"
      enter-to-class="opacity-100 scale-100"
      leave-active-class="transition duration-75 ease-in"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
    >
      <ul
        v-if="open"
        role="listbox"
        class="absolute right-0 mt-1 min-w-full bg-white border border-ivory-400 rounded-lg shadow-lg z-50 py-1 origin-top-right"
      >
        <li
          v-for="(opt, i) in options"
          :key="opt.value"
          role="option"
          :aria-selected="opt.value === modelValue"
          @click="select(opt.value)"
          @mouseenter="highlighted = i"
          :class="[
            'px-3 py-2 text-sm cursor-pointer transition-colors duration-100',
            opt.value === modelValue
              ? 'bg-teal-50 text-teal-700 font-medium'
              : highlighted === i
                ? 'bg-teal-50 text-teal-700'
                : 'text-gray-700'
          ]"
        >
          {{ opt.label }}
        </li>
      </ul>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import AppIcon from './AppIcon.vue'

const props = defineProps<{
  modelValue: string
  options: { value: string; label: string }[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'change': [value: string]
}>()

const open = ref(false)
const highlighted = ref(-1)
const rootEl = ref<HTMLElement | null>(null)

const selectedLabel = computed(
  () => props.options.find(o => o.value === props.modelValue)?.label ?? ''
)

function toggle() {
  open.value = !open.value
  if (open.value) highlighted.value = props.options.findIndex(o => o.value === props.modelValue)
}

function select(value: string) {
  emit('update:modelValue', value)
  emit('change', value)
  open.value = false
}

function onKeydown(e: KeyboardEvent) {
  if (!open.value) {
    if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
      e.preventDefault()
      open.value = true
      highlighted.value = props.options.findIndex(o => o.value === props.modelValue)
    }
    return
  }
  if (e.key === 'Escape') { open.value = false; return }
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    highlighted.value = Math.min(highlighted.value + 1, props.options.length - 1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    highlighted.value = Math.max(highlighted.value - 1, 0)
  } else if (e.key === 'Enter' && highlighted.value >= 0) {
    e.preventDefault()
    select(props.options[highlighted.value].value)
  }
}

function onClickOutside(e: MouseEvent) {
  if (rootEl.value && !rootEl.value.contains(e.target as Node)) {
    open.value = false
  }
}

onMounted(() => document.addEventListener('mousedown', onClickOutside))
onUnmounted(() => document.removeEventListener('mousedown', onClickOutside))
</script>
