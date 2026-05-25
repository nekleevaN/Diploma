<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4 py-12">
    <div class="w-full max-w-md">
      <div class="card p-8 text-center">
        <div class="text-5xl mb-4">🎉</div>
        <h1 class="text-2xl font-bold text-gray-900 mb-2">
          Ласкаво просимо, {{ auth.firstName || auth.username }}!
        </h1>
        <p class="text-sm text-gray-500 mb-8">Ваш акаунт підтверджено. Починаймо!</p>

        <div class="grid grid-cols-1 gap-3 mb-6">
          <RouterLink to="/profile"
            class="flex items-center gap-3 p-4 bg-teal-50 rounded-xl border border-teal-200 hover:bg-teal-100 transition-colors text-left">
            <span class="text-xl shrink-0">📋</span>
            <div>
              <p class="text-sm font-semibold text-teal-800">Заповніть профіль</p>
              <p class="text-xs text-teal-600">Додайте фото і опис</p>
            </div>
          </RouterLink>

          <RouterLink to="/"
            class="flex items-center gap-3 p-4 bg-ivory-200 rounded-xl border border-ivory-400 hover:bg-ivory-300 transition-colors text-left">
            <span class="text-xl shrink-0">🔍</span>
            <div>
              <p class="text-sm font-semibold text-gray-800">Огляньте каталог</p>
              <p class="text-xs text-gray-500">Тисячі товарів вже чекають</p>
            </div>
          </RouterLink>

          <RouterLink :to="`/users/${auth.userId}`"
            class="flex items-center gap-3 p-4 bg-ivory-200 rounded-xl border border-ivory-400 hover:bg-ivory-300 transition-colors text-left">
            <span class="text-xl shrink-0">🇺🇦</span>
            <div>
              <p class="text-sm font-semibold text-gray-800">Верифікація через Дію</p>
              <p class="text-xs text-gray-500">Підвищте рівень довіри (опціонально)</p>
            </div>
          </RouterLink>
        </div>

        <RouterLink to="/" class="btn-primary block text-center">
          Перейти до каталогу →
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
const auth = useAuthStore()
</script>
