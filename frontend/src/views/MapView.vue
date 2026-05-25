<template>
  <div class="h-[calc(100vh-56px)] flex flex-col">
    
    <div class="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3 shrink-0">
      <h1 class="text-base font-bold text-gray-900 flex items-center gap-1.5"><AppIcon name="map" size="w-4 h-4" /> Карта оголошень</h1>
      <span class="text-xs text-gray-400">{{ mapAds.length }} оголошень з локацією</span>
    </div>

    
    <div ref="mapEl" class="flex-1" />

    
    <div v-if="loading" class="absolute inset-0 flex items-center justify-center bg-white/80 z-10">
      <span class="text-gray-400 text-sm">Завантаження карти...</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { catalogApi, type MapAdDto } from '@/api/catalog'
import AppIcon from '@/components/AppIcon.vue'
import type { Map } from 'leaflet'

const mapEl = ref<HTMLElement | null>(null)
const mapAds = ref<MapAdDto[]>([])
const loading = ref(true)
let map: Map | null = null

const CATEGORY_EMOJIS: Record<string, string> = {
  'Електроніка': '💻', 'Телефони': '📱', 'Одяг': '👕', 'Взуття': '👟',
  'Меблі': '🛋️', 'Авто': '🚗', 'Книги': '📚', 'Спорт': '⚽'
}

onMounted(async () => {
  const L = (await import('leaflet')).default
  await import('leaflet/dist/leaflet.css')

  if (!mapEl.value) return

  map = L.map(mapEl.value).setView([49.0, 31.0], 6)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map)

  try {
    const { data } = await catalogApi.getMapAds()
    mapAds.value = data

    for (const ad of data) {
      const emoji = CATEGORY_EMOJIS[ad.category] ?? '🏷️'
      const icon = L.divIcon({
        className: '',
        html: `<div style="font-size:28px;cursor:pointer;filter:drop-shadow(0 2px 4px rgba(0,0,0,.25));transition:transform .15s"
               onmouseover="this.style.transform='scale(1.2)'"
               onmouseout="this.style.transform='scale(1)'">${emoji}</div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -32]
      })

      const firstImg = ad.imageUrls?.[0]
      const imgHtml = firstImg
        ? `<img src="${firstImg}" style="width:100%;height:80px;object-fit:cover;border-radius:6px;margin-bottom:6px">`
        : ''

      const popup = L.popup({ maxWidth: 220, className: 'ad-popup' }).setContent(`
        <div style="font-family:sans-serif;min-width:180px">
          ${imgHtml}
          <p style="font-size:13px;font-weight:600;margin:0 0 2px;line-height:1.3">${ad.title}</p>
          <p style="font-size:15px;font-weight:700;color:#708238;margin:0 0 4px">₴${ad.price.toLocaleString()}</p>
          ${ad.locationAddress ? `<p style="font-size:11px;color:#6b7280;margin:0 0 6px">📍 ${ad.locationAddress}</p>` : ''}
          <a href="/ads/${ad.id}"
             onclick="window.location.href='/ads/${ad.id}';return false"
             style="display:inline-block;padding:4px 10px;background:#708238;color:white;border-radius:6px;font-size:12px;text-decoration:none">
            Переглянути →
          </a>
        </div>
      `)

      L.marker([ad.latitude, ad.longitude], { icon })
        .addTo(map)
        .bindPopup(popup)
    }

    if (data.length > 0) {
      const bounds = L.latLngBounds(data.map(a => [a.latitude, a.longitude] as [number, number]))
      map.fitBounds(bounds, { padding: [50, 50], maxZoom: 13 })
    }
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  map?.remove()
})
</script>
