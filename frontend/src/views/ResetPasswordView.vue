<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-sm">
      <div class="card p-8">
        <h1 class="text-xl font-bold text-gray-900 mb-1">Новий пароль</h1>
        <p class="text-sm text-gray-500 mb-6">Введіть та підтвердіть новий пароль</p>

        <form @submit.prevent="submit" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Новий пароль</label>
            <input v-model="form.password" type="password"
              class="input" placeholder="Мін. 8 символів" required minlength="8" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Підтвердження</label>
            <input v-model="form.confirm" type="password"
              class="input" placeholder="Повторіть пароль" required />
            <p v-if="form.confirm && form.password !== form.confirm"
              class="mt-1 text-xs text-red-600">Паролі не співпадають</p>
          </div>
          <p v-if="error" class="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{{ error }}</p>
          <button type="submit"
            :disabled="loading || !form.password || form.password !== form.confirm"
            class="btn-primary w-full">
            {{ loading ? 'Зберігаємо...' : 'Зберегти новий пароль' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/auth'

const route  = useRoute()
const router = useRouter()
const auth   = useAuthStore()
const token  = ref((route.query.token as string) ?? '')

const form    = ref({ password: '', confirm: '' })
const loading = ref(false)
const error   = ref('')

async function submit() {
  if (form.value.password !== form.value.confirm) return
  loading.value = true; error.value = ''
  try {
    const { data } = await authApi.resetPassword(
      token.value, form.value.password, form.value.confirm)
    auth.setAuth(data.token, data.userId)
    router.push('/')
  } catch (e: any) {
    const d = e?.response?.data
    error.value = d?.error ?? (d?.code === 'TOKEN_EXPIRED'
      ? 'Посилання застаріло. Запросіть нове.' : 'Помилка. Спробуйте ще раз.')
  } finally { loading.value = false }
}
</script>
