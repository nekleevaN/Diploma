<template>
  <div class="max-w-xl mx-auto px-4 py-8">
    <h1 class="text-xl font-bold text-gray-900 mb-6">Мій профіль</h1>

    
    <div class="card p-6 mb-4">
      <div class="flex items-center gap-4">
        <div class="w-16 h-16 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center text-2xl font-bold uppercase">
          {{ (auth.firstName || auth.username)?.charAt(0) }}
        </div>
        <div>
          <h2 class="text-lg font-semibold text-gray-900">
            {{ auth.firstName && auth.lastName ? `${auth.firstName} ${auth.lastName}` : auth.username }}
          </h2>
          <p class="text-xs text-gray-400">@{{ auth.username }}</p>
          <p class="text-sm text-gray-500">{{ auth.email }}</p>
        </div>
      </div>

      
      <div class="mt-5">
        <p class="text-xs font-medium text-gray-500 uppercase tracking-wide mb-3">Верифікація</p>
        <div class="flex flex-wrap gap-2">
          <span
            v-for="badge in badges"
            :key="badge.key"
            :class="[
              'inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium',
              badge.active
                ? 'bg-teal-150 text-teal-700 border border-teal-300'
                : 'bg-gray-100 text-gray-400'
            ]"
          >
            <AppIcon v-if="badge.active" name="check-circle" size="w-3.5 h-3.5" class="text-teal-500" /><span v-else>○</span>
            {{ badge.label }}
          </span>
        </div>
      </div>
    </div>

    
    <div class="card p-6 mb-4">
      <div class="flex items-start gap-4">
        <div class="text-2xl shrink-0">💳</div>
        <div class="flex-1">
          <h3 class="font-semibold text-gray-900">Отримання виплат</h3>
          <p class="text-sm text-gray-500 mt-1">
            Підключіть <strong>Monobank SubMerchant</strong> — гроші від покупців
            надходитимуть автоматично після підтвердження доставки.
          </p>

          <div v-if="auth.isPayoutEnabled" class="mt-3 space-y-2">
            <div class="flex items-center gap-2 text-teal-600">
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span class="text-sm font-medium">Виплати підключено ✓</span>
            </div>
            <button @click="showPayoutEdit = !showPayoutEdit"
              class="text-xs text-gray-400 hover:text-gray-600 underline">
              Змінити SubMerchant ID
            </button>
          </div>

          <div v-else class="mt-3">
            <div class="bg-amber-50 border border-amber-200 rounded-xl px-3 py-2 mb-3 text-xs text-amber-800">
              ⚠️ Без цього ви <strong>не зможете публікувати оголошення</strong>.
            </div>
            <button @click="showPayoutEdit = true" class="btn-primary text-sm">
              Підключити виплати
            </button>
          </div>

          <div v-if="showPayoutEdit" class="mt-4 space-y-3 border-t border-ivory-400 pt-4">
            <div class="bg-teal-50 border border-teal-200 rounded-xl p-3 text-xs text-teal-700 space-y-1">
              <p class="font-medium">Як отримати SubMerchant ID:</p>
              <p>1. Зареєструйтесь у <a href="https://business.monobank.ua" target="_blank" class="underline">Monobank Business</a></p>
              <p>2. Пройдіть KYC верифікацію (фіз. особа або ФОП)</p>
              <p>3. Скопіюйте виданий SubMerchant ID і вставте нижче</p>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Monobank SubMerchant ID</label>
              <input v-model="subMerchantInput" type="text" class="input text-sm font-mono"
                placeholder="sub_merchant_abc123" />
            </div>
            <p v-if="payoutError" class="text-xs text-red-600">{{ payoutError }}</p>
            <div class="flex gap-2 items-center">
              <button @click="savePayout" :disabled="payoutSaving || !subMerchantInput.trim()"
                class="btn-primary text-xs py-1.5 px-3 flex items-center gap-1.5">
                <svg v-if="payoutSaving" class="w-3.5 h-3.5 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                </svg>
                {{ payoutSaving ? 'Зберігаємо...' : 'Зберегти' }}
              </button>
              <button @click="showPayoutEdit = false" class="btn-secondary text-xs py-1.5 px-3">Скасувати</button>
              <button v-if="auth.isPayoutEnabled" @click="removePayout"
                class="text-xs text-red-400 hover:text-red-600 ml-auto underline">
                Відключити
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>


    <div class="card p-6">
      <DiiaVerification />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { usersApi } from '@/api/users'
import AppIcon from '@/components/AppIcon.vue'
import DiiaVerification from '@/components/DiiaVerification.vue'

const auth = useAuthStore()

const showPayoutEdit = ref(false)
const subMerchantInput = ref('')
const payoutSaving = ref(false)
const payoutError = ref('')

async function savePayout() {
  if (!subMerchantInput.value.trim()) return
  payoutSaving.value = true
  payoutError.value = ''
  try {
    const { data } = await usersApi.setPayoutMethod(subMerchantInput.value.trim())
    auth.setPayoutEnabled(data.payoutEnabled)
    showPayoutEdit.value = false
    subMerchantInput.value = ''
  } catch (e: any) {
    payoutError.value = e?.response?.data?.error ?? 'Помилка збереження'
  } finally {
    payoutSaving.value = false
  }
}

async function removePayout() {
  payoutSaving.value = true
  try {
    await usersApi.setPayoutMethod(null)
    auth.setPayoutEnabled(false)
    showPayoutEdit.value = false
  } catch {  }
  finally { payoutSaving.value = false }
}

const badges = computed(() => [
  { key: 'email', label: 'Email підтверджено', active: true },
  { key: 'diia', label: 'Дія верифікація', active: auth.hasDiia }
])
</script>
