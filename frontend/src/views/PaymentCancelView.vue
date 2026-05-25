<template>
  <div class="min-h-[calc(100vh-56px)] flex items-center justify-center px-4">
    <div class="text-center max-w-sm">
      <div class="flex justify-center mb-4"><AppIcon name="x-circle" size="w-14 h-14" class="text-red-500" /></div>
      <h1 class="text-2xl font-bold text-gray-900 mb-2">Оплату скасовано</h1>
      <p class="text-gray-500 text-sm mb-6">Нічого не списано. Можеш спробувати ще раз.</p>
      <RouterLink to="/" class="btn-secondary">Назад до оголошень</RouterLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '@/components/AppIcon.vue'
</script>
