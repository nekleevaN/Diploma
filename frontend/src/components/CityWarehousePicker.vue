<template>
  <div class="space-y-3">
    
    <div class="relative">
      <label class="block text-sm font-medium text-gray-700 mb-1">Місто</label>
      <input
        v-model="cityQuery"
        @input="debouncedSearch"
        @focus="showCitySuggestions = true"
        type="text"
        class="input"
        :placeholder="selectedCity ? selectedCity.description : 'Введіть назву міста...'"
      />
      <div v-if="showCitySuggestions && cities.length"
        class="absolute z-20 w-full mt-1 bg-white border border-gray-200 rounded-xl shadow-lg max-h-48 overflow-y-auto">
        <button v-for="city in cities" :key="city.ref"
          @click="selectCity(city)"
          class="w-full text-left px-4 py-2.5 text-sm hover:bg-teal-50 transition-colors border-b border-gray-50 last:border-0">
          <span class="font-medium text-gray-900">{{ city.description }}</span>
          <span class="text-gray-400 ml-2 text-xs">{{ city.area }}</span>
        </button>
      </div>
      <div v-if="loadingCities" class="absolute right-3 top-9 text-gray-400 text-xs">Пошук...</div>
    </div>

    
    <div v-if="selectedCity">
      <label class="block text-sm font-medium text-gray-700 mb-1">Відділення</label>
      
      <input
        v-model="warehouseSearch"
        @input="debouncedWarehouseSearch"
        type="text"
        class="input mb-2 text-sm"
        placeholder="Пошук по номеру або адресі (напр. 1, Хрещатик)..."
      />
      <div v-if="loadingWarehouses" class="text-sm text-gray-400 py-2">Пошук відділень...</div>
      <div v-else-if="warehouses.length" class="max-h-52 overflow-y-auto border border-gray-200 rounded-xl">
        <button v-for="w in warehouses" :key="w.ref"
          @click="selectWarehouse(w)"
          :class="['w-full text-left px-4 py-2.5 text-sm border-b border-gray-50 last:border-0 transition-colors',
            selectedWarehouse?.ref === w.ref ? 'bg-teal-50 text-teal-700' : 'hover:bg-gray-50 text-gray-800']">
          <span class="font-medium">{{ w.number ? `№${w.number} · ` : '' }}{{ w.shortAddress || w.description }}</span>
        </button>
        <button v-if="hasMoreWarehouses && !warehouseSearch" @click="loadMoreWarehouses"
          class="w-full py-2 text-xs text-teal-600 hover:bg-teal-50">
          Завантажити ще...
        </button>
      </div>
      <p v-else-if="warehouseSearch.length >= 2" class="text-sm text-gray-400">Відділень не знайдено</p>
      <p v-else class="text-xs text-gray-400">Введіть номер або адресу відділення для пошуку</p>
    </div>

    
    <div v-if="selectedWarehouse" class="bg-teal-150 border border-teal-150 rounded-xl px-4 py-3">
      <p class="text-xs font-medium text-teal-500 flex items-center gap-1"><AppIcon name="check-circle" size="w-3.5 h-3.5" /> Вибрано:</p>
      <p class="text-sm text-teal-600 mt-0.5">{{ selectedCity?.description }}, {{ selectedWarehouse.shortAddress || selectedWarehouse.description }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { deliveryApi, type NPCity, type NPWarehouse } from '@/api/delivery'
import AppIcon from '@/components/AppIcon.vue'

const emit = defineEmits<{
  selected: [{ cityRef: string; cityName: string; warehouseRef: string; warehouseAddress: string }]
}>()

const cityQuery = ref('')
const cities = ref<NPCity[]>([])
const selectedCity = ref<NPCity | null>(null)
const showCitySuggestions = ref(false)
const loadingCities = ref(false)

const warehouses = ref<NPWarehouse[]>([])
const selectedWarehouse = ref<NPWarehouse | null>(null)
const loadingWarehouses = ref(false)
const warehousePage = ref(1)
const hasMoreWarehouses = ref(false)
const warehouseSearch = ref('')

let searchTimer: ReturnType<typeof setTimeout>
let warehouseSearchTimer: ReturnType<typeof setTimeout>

function debouncedWarehouseSearch() {
  clearTimeout(warehouseSearchTimer)
  warehouseSearchTimer = setTimeout(async () => {
    if (!selectedCity.value) return
    if (warehouseSearch.value.length > 0 && warehouseSearch.value.length < 2) return
    warehousePage.value = 1
    await loadWarehouses()
  }, 350)
}

function debouncedSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(searchCities, 350)
}

async function searchCities() {
  if (cityQuery.value.length < 2) { cities.value = []; return }
  loadingCities.value = true
  try {
    const { data } = await deliveryApi.searchCities(cityQuery.value)
    cities.value = data
    showCitySuggestions.value = true
  } finally {
    loadingCities.value = false
  }
}

async function selectCity(city: NPCity) {
  selectedCity.value = city
  cityQuery.value = city.description
  showCitySuggestions.value = false
  selectedWarehouse.value = null
  warehousePage.value = 1
  warehouseSearch.value = ''
  warehouses.value = []
}

async function loadWarehouses() {
  if (!selectedCity.value) return
  if (warehouseSearch.value.length === 1) return
  loadingWarehouses.value = true
  try {
    const searchParam = warehouseSearch.value.length >= 2 ? warehouseSearch.value : undefined
    const { data } = await deliveryApi.getWarehouses(selectedCity.value.ref, warehousePage.value, searchParam)
    warehouses.value = warehousePage.value === 1 ? data : [...warehouses.value, ...data]
    hasMoreWarehouses.value = data.length >= 20
  } finally {
    loadingWarehouses.value = false
  }
}

async function loadMoreWarehouses() {
  warehousePage.value++
  await loadWarehouses()
}

function selectWarehouse(w: NPWarehouse) {
  selectedWarehouse.value = w
  emit('selected', {
    cityRef: selectedCity.value!.ref,
    cityName: selectedCity.value!.description,
    warehouseRef: w.ref,
    warehouseAddress: w.shortAddress || w.description
  })
}

function onClickOutside() { showCitySuggestions.value = false }
</script>
