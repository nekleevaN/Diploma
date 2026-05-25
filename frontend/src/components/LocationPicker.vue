<template>
  <div>
    <div class="flex items-center justify-between mb-2">
      <label class="text-sm font-medium text-gray-700">Місцезнаходження товару (необов'язково)</label>
      <button v-if="selected" type="button" @click="clear"
        class="text-xs text-red-500 hover:text-red-700">Видалити</button>
    </div>

    <div v-if="selected" class="mb-2 text-xs text-teal-700 bg-teal-150 rounded-lg px-3 py-2 flex items-center gap-1">
      <AppIcon name="location" size="w-3.5 h-3.5" class="shrink-0" /> {{ selected.address || `${selected.lat.toFixed(5)}, ${selected.lng.toFixed(5)}` }}
    </div>

    <div ref="mapEl" class="w-full h-48 rounded-xl border border-gray-200 overflow-hidden" />

    <div class="flex gap-2 mt-2">
      <button type="button" @click="useMyLocation" :disabled="locating"
        class="text-xs text-teal-600 hover:text-blue-800 flex items-center gap-1">
        <AppIcon name="location" size="w-3.5 h-3.5" /> {{ locating ? 'Визначаємо...' : 'Моє місцезнаходження' }}
      </button>
      <span class="text-xs text-gray-300">·</span>
      <span class="text-xs text-gray-400">або клікніть на карті</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import AppIcon from '@/components/AppIcon.vue'
import type { Map, Marker } from 'leaflet'

const emit = defineEmits<{
  selected: [{ lat: number; lng: number; address: string }]
  cleared: []
}>()

const props = defineProps<{ initialLat?: number | null; initialLng?: number | null }>()

const mapEl = ref<HTMLElement | null>(null)
const locating = ref(false)
const selected = ref<{ lat: number; lng: number; address: string } | null>(null)

let map: Map | null = null
let marker: Marker | null = null

onMounted(async () => {
  const L = (await import('leaflet')).default
  await import('leaflet/dist/leaflet.css')

  if (!mapEl.value) return

  const center: [number, number] = props.initialLat && props.initialLng
    ? [props.initialLat, props.initialLng]
    : [48.3794, 31.1656]

  map = L.map(mapEl.value, { zoomControl: true }).setView(center, props.initialLat ? 14 : 6)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map)

  if (props.initialLat && props.initialLng) {
    placeMarker(L, props.initialLat, props.initialLng, '')
  }

  map.on('click', async (e) => {
    const { lat, lng } = e.latlng
    const address = await reverseGeocode(lat, lng)
    placeMarker(L, lat, lng, address)
    selected.value = { lat, lng, address }
    emit('selected', { lat, lng, address })
  })
})

onUnmounted(() => {
  map?.remove()
})

function placeMarker(L: any, lat: number, lng: number, address: string) {
  if (marker) marker.remove()
  marker = L.marker([lat, lng], {
    icon: L.divIcon({
      className: '',
      html: '<div style="font-size:28px;filter:drop-shadow(0 2px 4px rgba(0,0,0,.3))">📍</div>',
      iconSize: [32, 32],
      iconAnchor: [16, 32]
    })
  }).addTo(map!)
}

async function reverseGeocode(lat: number, lng: number): Promise<string> {
  try {
    const r = await fetch(
      `https:
    )
    const data = await r.json()
    const a = data.address
    return [a.road, a.city || a.town || a.village, a.country].filter(Boolean).join(', ')
  } catch {
    return `${lat.toFixed(4)}, ${lng.toFixed(4)}`
  }
}

async function useMyLocation() {
  if (!navigator.geolocation) return
  locating.value = true
  navigator.geolocation.getCurrentPosition(
    async (pos) => {
      const { latitude: lat, longitude: lng } = pos.coords
      map?.setView([lat, lng], 15)
      const address = await reverseGeocode(lat, lng)
      const L = (await import('leaflet')).default
      placeMarker(L, lat, lng, address)
      selected.value = { lat, lng, address }
      emit('selected', { lat, lng, address })
      locating.value = false
    },
    () => { locating.value = false }
  )
}

function clear() {
  if (marker) { marker.remove(); marker = null }
  selected.value = null
  emit('cleared')
}
</script>
