<template>
  <div class="max-w-5xl mx-auto px-4 py-8">
    
    <Transition name="fade">
      <div v-if="toast"
        class="fixed top-4 left-1/2 -translate-x-1/2 z-50 flex items-center gap-3 bg-red-600 text-white text-sm px-5 py-3 rounded-xl shadow-lg max-w-md w-full mx-4">
        <AppIcon name="warning" size="w-5 h-5 shrink-0" />
        <span class="flex-1">{{ toast }}</span>
        <button @click="toast = ''" class="text-white/70 hover:text-white">
          <AppIcon name="x" size="w-4 h-4" />
        </button>
      </div>
    </Transition>

    
    <Transition name="fade">
      <div v-if="showSellerNoPayoutModal"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
        <div class="bg-white rounded-2xl p-6 max-w-sm w-full shadow-xl text-center">
          <div class="text-4xl mb-3">💳</div>
          <h2 class="text-base font-bold text-gray-900 mb-2">Купівля тимчасово недоступна</h2>
          <p class="text-sm text-gray-500 mb-3">
            Продавець ще не підключив отримання виплат через Monobank.
          </p>
          <p class="text-xs text-gray-400 mb-5">
            Зв'яжіться з продавцем у чаті та попросіть налаштувати профіль виплат у розділі
            «Профіль → Налаштування виплат».
          </p>
          <div class="flex gap-3">
            <RouterLink :to="`/ads/${adId}`" class="btn-secondary flex-1 text-center">
              Назад до оголошення
            </RouterLink>
            <button @click="showSellerNoPayoutModal = false; $router.push('/')"
              class="btn-primary flex-1">
              До каталогу
            </button>
          </div>
        </div>
      </div>
    </Transition>

    
    <Transition name="fade">
      <div v-if="showConflict"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
        <div class="bg-white rounded-2xl p-6 max-w-sm w-full shadow-xl text-center">
          <div class="text-4xl mb-3">😔</div>
          <h2 class="text-base font-bold text-gray-900 mb-2">Товар уже придбано</h2>
          <p class="text-sm text-gray-500 mb-5">На жаль, цей товар щойно придбав інший покупець.</p>
          <RouterLink to="/" class="btn-primary w-full block text-center">
            Назад до каталогу
          </RouterLink>
        </div>
      </div>
    </Transition>

    <h1 class="text-xl font-bold text-gray-900 mb-6">Оформлення замовлення</h1>

    <div class="flex flex-col lg:flex-row gap-6">
      
      <div class="flex-1 space-y-4">

        
        <section class="card p-5">
          <h2 class="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <span class="w-6 h-6 rounded-full bg-teal-500 text-white flex items-center justify-center text-xs font-bold shrink-0">1</span>
            Доставка (Нова Пошта)
          </h2>

          
          <div v-if="npError"
            class="mb-4 flex items-center gap-3 bg-orange-50 border border-orange-200 rounded-xl px-4 py-3 text-sm text-orange-700">
            <AppIcon name="warning" size="w-4 h-4 shrink-0" />
            <span class="flex-1">Не вдалося завантажити дані Нової Пошти</span>
            <button @click="npError = false" class="text-xs underline">Спробувати ще</button>
          </div>

          
          <div class="flex gap-3 mb-4">
            <label v-for="t in warehouseTypes" :key="t.value"
              :class="['flex items-center gap-2 px-3 py-2 rounded-lg border cursor-pointer text-sm transition-colors',
                warehouseType === t.value
                  ? 'border-teal-500 bg-teal-50 text-teal-700'
                  : 'border-gray-200 text-gray-600 hover:border-teal-300']">
              <input type="radio" v-model="warehouseType" :value="t.value" class="hidden" />
              {{ t.label }}
            </label>
          </div>

          
          <div class="mb-4 relative">
            <label class="block text-xs font-medium text-gray-600 mb-1">Місто *</label>
            <input
              v-model="cityQuery"
              @input="onCityInput"
              @blur="onCityBlur"
              @focus="showCitySuggestions = cityResults.length > 0"
              type="text"
              placeholder="Введіть назву міста..."
              :class="inputClass(errors.city)"
              autocomplete="off" />
            <p v-if="touched.city && errors.city" class="mt-1 text-xs text-red-600">{{ errors.city }}</p>

            
            <ul v-if="showCitySuggestions && cityResults.length"
              class="absolute z-20 left-0 right-0 mt-1 bg-white border border-ivory-400 rounded-xl shadow-lg max-h-52 overflow-y-auto">
              <li v-for="city in cityResults" :key="city.ref"
                @mousedown.prevent="selectCity(city)"
                class="px-4 py-2.5 text-sm text-gray-700 hover:bg-teal-50 hover:text-teal-700 cursor-pointer">
                {{ city.description }}
                <span v-if="city.area" class="text-xs text-gray-400 ml-1">{{ city.area }}</span>
              </li>
            </ul>
          </div>

          
          <div class="relative">
            <label class="block text-xs font-medium text-gray-600 mb-1">
              {{ warehouseType === 'postomat' ? 'Поштомат' : 'Відділення' }} *
            </label>
            <input
              v-model="warehouseQuery"
              @input="onWarehouseInput"
              @blur="onWarehouseBlur"
              @focus="showWarehouseSuggestions = warehouseResults.length > 0"
              type="text"
              :placeholder="selectedCity ? 'Пошук відділення...' : 'Спочатку оберіть місто'"
              :disabled="!selectedCity"
              :class="inputClass(errors.warehouse)"
              autocomplete="off" />
            <p v-if="touched.warehouse && errors.warehouse" class="mt-1 text-xs text-red-600">{{ errors.warehouse }}</p>

            <ul v-if="showWarehouseSuggestions && warehouseResults.length"
              class="absolute z-20 left-0 right-0 mt-1 bg-white border border-ivory-400 rounded-xl shadow-lg max-h-52 overflow-y-auto">
              <li v-for="w in warehouseResults" :key="w.ref"
                @mousedown.prevent="selectWarehouse(w)"
                class="px-4 py-2.5 text-sm text-gray-700 hover:bg-teal-50 hover:text-teal-700 cursor-pointer">
                {{ w.shortAddress || w.description }}
              </li>
            </ul>
          </div>
        </section>

        
        <section class="card p-5">
          <h2 class="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <span class="w-6 h-6 rounded-full bg-teal-500 text-white flex items-center justify-center text-xs font-bold shrink-0">2</span>
            Контактні дані
          </h2>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Ім'я *</label>
              <input v-model="form.firstName" @blur="touch('firstName')" type="text"
                placeholder="Іванна" :class="inputClass(errors.firstName)" />
              <p v-if="touched.firstName && errors.firstName" class="mt-1 text-xs text-red-600">{{ errors.firstName }}</p>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Прізвище *</label>
              <input v-model="form.lastName" @blur="touch('lastName')" type="text"
                placeholder="Коваленко" :class="inputClass(errors.lastName)" />
              <p v-if="touched.lastName && errors.lastName" class="mt-1 text-xs text-red-600">{{ errors.lastName }}</p>
            </div>
          </div>

          <div class="mt-4">
            <label class="block text-xs font-medium text-gray-600 mb-1">Телефон *</label>
            <input v-model="form.phone" @blur="touch('phone')" type="tel"
              placeholder="+380XXXXXXXXX" :class="inputClass(errors.phone)" />
            <p v-if="touched.phone && errors.phone" class="mt-1 text-xs text-red-600">{{ errors.phone }}</p>
          </div>
        </section>

        
        <section class="card p-5">
          <h2 class="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <span class="w-6 h-6 rounded-full bg-teal-500 text-white flex items-center justify-center text-xs font-bold shrink-0">3</span>
            Оплата
          </h2>

          <div class="flex flex-wrap items-center gap-3 mb-4">
            <span class="text-xs text-gray-500">Безпечна оплата через</span>
            <span class="font-bold text-gray-800 text-sm">Monobank</span>
            <span class="text-xs text-gray-400">·</span>
            <span class="text-xs text-gray-500">Visa, Mastercard, Apple Pay, Google Pay</span>
          </div>

          <label class="flex items-start gap-3 cursor-pointer group">
            <input type="checkbox" v-model="form.agreed" @change="touch('agreed')"
              class="mt-0.5 rounded border-gray-300 shrink-0" />
            <span :class="['text-xs leading-relaxed', touched.agreed && errors.agreed ? 'text-red-600' : 'text-gray-600']">
              Я погоджуюсь з
              <a href="#" class="text-teal-600 underline hover:text-teal-700">умовами користування</a>
              платформою trustee
            </span>
          </label>
          <p v-if="touched.agreed && errors.agreed" class="mt-1 text-xs text-red-600">{{ errors.agreed }}</p>
        </section>
      </div>

      
      <div class="lg:w-72 lg:shrink-0">
        <div class="sticky top-6 space-y-3">

          
          <div class="card p-4" v-if="ad">
            <div class="flex gap-3">
              <img v-if="ad.imageUrls?.[0]" :src="ad.imageUrls[0]" :alt="ad.title"
                class="w-16 h-16 rounded-lg object-cover shrink-0 bg-ivory-300" />
              <div v-else class="w-16 h-16 rounded-lg bg-ivory-300 shrink-0 flex items-center justify-center">
                <AppIcon name="photo" size="w-6 h-6 text-gray-300" />
              </div>
              <div class="min-w-0">
                <p class="text-sm font-medium text-gray-900 line-clamp-2 leading-tight">{{ ad.title }}</p>
                <p class="text-xs text-gray-400 mt-0.5">{{ ad.sellerName }}</p>
              </div>
            </div>
          </div>

          
          <div class="card p-4 space-y-2">
            <div class="flex justify-between text-sm">
              <span class="text-gray-500">Товар</span>
              <span class="font-medium">₴{{ amount.toLocaleString() }}</span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-gray-500">Доставка</span>
              <span class="text-gray-400 text-xs">За тарифами НП</span>
            </div>
            <div class="border-t border-ivory-400 pt-2 flex justify-between">
              <span class="text-sm font-semibold text-gray-900">Разом</span>
              <span class="text-sm font-bold text-teal-600">₴{{ amount.toLocaleString() }}</span>
            </div>
          </div>

          
          <button
            @click="submit"
            :disabled="!isValid || submitting"
            class="checkout-submit-btn w-full py-3 rounded-xl text-sm font-semibold text-white transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2">
            <AppIcon v-if="submitting" name="refresh" size="w-4 h-4 animate-spin" />
            <AppIcon v-else name="buy" size="w-4 h-4" />
            {{ submitting ? 'Створення замовлення...' : 'Підтвердити та оплатити' }}
          </button>

          
          <div class="flex items-start gap-2 px-1">
            <div class="relative group shrink-0 mt-0.5">
              <AppIcon name="shield" size="w-4 h-4 text-teal-500 cursor-pointer" />
              <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-52 bg-gray-900 text-white text-xs rounded-lg px-3 py-2 hidden group-hover:block shadow-lg z-10 leading-relaxed">
                Гроші блокуються на рахунку Monobank і надходять продавцю тільки після підтвердження отримання товару.
              </div>
            </div>
            <p class="text-xs text-gray-400 leading-relaxed">
              Гроші тримаються в безпеці до отримання товару
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { catalogApi } from '@/api/catalog'
import { deliveryApi, type NPCity, type NPWarehouse } from '@/api/delivery'
import { paymentApi } from '@/api/payment'
import { useAuthStore } from '@/stores/auth'
import AppIcon from '@/components/AppIcon.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const adId = route.params.adId as string
const amount = ref(Number(route.query.amount) || 0)

const ad = ref<any>(null)
const submitting = ref(false)
const toast = ref('')
const showConflict = ref(false)
const showSellerNoPayoutModal = ref(false)
const npError = ref(false)

const form = reactive({
  firstName: '',
  lastName: '',
  phone: '',
  agreed: false,
})

const warehouseTypes = [
  { value: 'warehouse', label: 'Відділення' },
  { value: 'postomat', label: 'Поштомат' },
]
const warehouseType = ref<'warehouse' | 'postomat'>('warehouse')

const cityQuery = ref('')
const cityResults = ref<NPCity[]>([])
const selectedCity = ref<NPCity | null>(null)
const showCitySuggestions = ref(false)
let cityDebounce: ReturnType<typeof setTimeout> | null = null

const warehouseQuery = ref('')
const warehouseResults = ref<NPWarehouse[]>([])
const selectedWarehouse = ref<NPWarehouse | null>(null)
const showWarehouseSuggestions = ref(false)
let warehouseDebounce: ReturnType<typeof setTimeout> | null = null

const touched = reactive<Record<string, boolean>>({})
const nameRe = /^[А-ЯҐЄІЇа-яґєії'a-zA-Z\- ]{2,50}$/u

const errors = computed(() => ({
  city:      !selectedCity.value ? 'Оберіть місто зі списку' : '',
  warehouse: !selectedWarehouse.value ? 'Оберіть відділення зі списку' : '',
  firstName: !nameRe.test(form.firstName.trim()) ? 'Від 2 до 50 літер' : '',
  lastName:  !nameRe.test(form.lastName.trim())  ? 'Від 2 до 50 літер' : '',
  phone:     !/^\+380\d{9}$/.test(form.phone)    ? 'Формат: +380XXXXXXXXX' : '',
  agreed:    !form.agreed                         ? 'Підтвердіть згоду' : '',
}))

const isValid = computed(() => Object.values(errors.value).every(e => !e))

function touch(field: string) {
  touched[field] = true
}

function touchAll() {
  ['city', 'warehouse', 'firstName', 'lastName', 'phone', 'agreed'].forEach(f => touch(f))
}

function inputClass(err: string) {
  return err
    ? 'input border-red-400 focus:border-red-500 focus:ring-red-400/20'
    : 'input'
}

function onCityInput() {
  selectedCity.value = null
  selectedWarehouse.value = null
  warehouseQuery.value = ''
  if (cityDebounce) clearTimeout(cityDebounce)
  if (cityQuery.value.length < 2) { cityResults.value = []; showCitySuggestions.value = false; return }
  cityDebounce = setTimeout(async () => {
    try {
      const { data } = await deliveryApi.searchCities(cityQuery.value)
      cityResults.value = data
      showCitySuggestions.value = data.length > 0
    } catch { npError.value = true }
  }, 300)
}

function onCityBlur() {
  touch('city')
  setTimeout(() => { showCitySuggestions.value = false }, 150)
}

function selectCity(city: NPCity) {
  selectedCity.value = city
  cityQuery.value = city.description
  showCitySuggestions.value = false
  cityResults.value = []
  touched['city'] = true
  loadWarehouses()
}

async function loadWarehouses(q = '') {
  if (!selectedCity.value) return
  try {
    const typeFilter = warehouseType.value === 'postomat' ? 'Поштомат' : undefined
    const { data } = await deliveryApi.getWarehouses(selectedCity.value.ref, 1, q || typeFilter)
    warehouseResults.value = data
    showWarehouseSuggestions.value = data.length > 0
  } catch { npError.value = true }
}

function onWarehouseInput() {
  selectedWarehouse.value = null
  if (warehouseDebounce) clearTimeout(warehouseDebounce)
  warehouseDebounce = setTimeout(() => loadWarehouses(warehouseQuery.value), 300)
}

function onWarehouseBlur() {
  touch('warehouse')
  setTimeout(() => { showWarehouseSuggestions.value = false }, 150)
}

function selectWarehouse(w: NPWarehouse) {
  selectedWarehouse.value = w
  warehouseQuery.value = w.shortAddress || w.description
  showWarehouseSuggestions.value = false
  touched['warehouse'] = true
}

watch(warehouseType, () => {
  selectedWarehouse.value = null
  warehouseQuery.value = ''
  if (selectedCity.value) loadWarehouses()
})

const abortController = ref<AbortController | null>(null)

async function submit() {
  touchAll()
  if (!isValid.value || submitting.value) return

  submitting.value = true
  toast.value = ''
  abortController.value = new AbortController()

  try {
    const { data } = await paymentApi.createCheckoutOrder({
      advertisementId: adId,
      amount: amount.value,
      recipientCityRef: selectedCity.value!.ref,
      recipientCityName: selectedCity.value!.description,
      recipientWarehouseRef: selectedWarehouse.value!.ref,
      recipientWarehouseAddress: selectedWarehouse.value!.shortAddress || selectedWarehouse.value!.description,
      recipientFirstName: form.firstName.trim(),
      recipientLastName: form.lastName.trim(),
      recipientPhone: form.phone,
    })

    sessionStorage.setItem(`checkout_${adId}`, JSON.stringify({
      firstName: form.firstName,
      lastName: form.lastName,
      phone: form.phone,
      cityDescription: cityQuery.value,
      warehouseDescription: warehouseQuery.value,
    }))

    window.location.href = data.monoPageUrl
  } catch (e: any) {
    console.error('Checkout error', e?.response?.status, e?.response?.data, e?.message)
    const errMsg: string = e?.response?.data?.error || ''

    if (e?.response?.status === 409) {
      showConflict.value = true
    } else if (errMsg.startsWith('SELLER_NO_PAYOUT:')) {
      showSellerNoPayoutModal.value = true
    } else {
      const d = e?.response?.data
      toast.value = d?.error || d?.title || d?.detail || e?.message || 'Щось пішло не так. Спробуйте ще раз.'
      submitting.value = false
    }
  }
}

onMounted(async () => {
  try {
    const { data } = await catalogApi.getById(adId)
    ad.value = data
    if (!amount.value) amount.value = (data as any).price
  } catch {
    router.push('/')
    return
  }

  form.firstName = auth.firstName ?? ''
  form.lastName  = auth.lastName  ?? ''

  const saved = sessionStorage.getItem(`checkout_${adId}`)
  if (saved) {
    try {
      const s = JSON.parse(saved)
      form.firstName = s.firstName || form.firstName
      form.lastName  = s.lastName  || form.lastName
      form.phone     = s.phone     || ''
      if (s.cityDescription) cityQuery.value = s.cityDescription
    } catch {  }
  }
})
</script>

<style scoped>
.checkout-submit-btn {
  background-color: #708238;
}
.checkout-submit-btn:hover:not(:disabled) {
  background-color: #5d6c2e;
}
</style>
