<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-sm">
      <div class="card p-8">
        <RouterLink to="/login" class="text-sm text-teal-600 hover:underline mb-5 block">← Назад до входу</RouterLink>
        <h1 class="text-xl font-bold text-gray-900 mb-1">Відновлення паролю</h1>
        <p class="text-sm text-gray-500 mb-6">Введіть email — надішлемо посилання для скидання</p>

        <template v-if="!sent">
          <form @submit.prevent="submit" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input v-model="email" type="email" class="input" placeholder="anna@example.com" required />
            </div>
            <p v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</p>
            <button type="submit" :disabled="loading" class="btn-primary w-full">
              {{ loading ? 'Надсилаємо...' : 'Надіслати посилання' }}
            </button>
          </form>
        </template>

        <template v-else>
          <div class="text-center py-4">
            <div class="text-4xl mb-3">📧</div>
            <p class="text-sm text-gray-600 mb-1">Якщо цей email зареєстровано, ви отримаєте лист.</p>
            <p class="text-xs text-gray-400">Перевірте папку «Спам».</p>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { authApi } from '@/api/auth'

const email   = ref('')
const loading = ref(false)
const error   = ref('')
const sent    = ref(false)

async function submit() {
  loading.value = true; error.value = ''
  try {
    await authApi.forgotPassword(email.value)
    sent.value = true
  } catch (e: any) {
    error.value = e?.response?.data?.error ?? 'Помилка. Спробуйте ще раз.'
  } finally { loading.value = false }
}
</script>
