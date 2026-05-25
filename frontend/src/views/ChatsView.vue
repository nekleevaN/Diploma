<template>
  <div class="max-w-xl mx-auto px-4 py-8">
    <h1 class="text-xl font-bold text-gray-900 mb-6">Мої переписки</h1>

    <div v-if="loading" class="space-y-3">
      <div v-for="i in 4" :key="i" class="card p-4 animate-pulse flex gap-3">
        <div class="w-10 h-10 bg-gray-200 rounded-full shrink-0" />
        <div class="flex-1 space-y-2">
          <div class="h-4 bg-gray-200 rounded w-3/4" />
          <div class="h-3 bg-gray-200 rounded w-1/2" />
        </div>
      </div>
    </div>

    <div v-else-if="chats.length === 0" class="text-center py-16 text-gray-400">
      <div class="flex justify-center mb-3"><AppIcon name="chat" size="w-10 h-10" /></div>
      <p class="text-sm">Переписок ще немає</p>
      <RouterLink to="/" class="text-sm text-teal-600 hover:underline mt-2 inline-block">Переглянути оголошення</RouterLink>
    </div>

    <div v-else class="space-y-2">
      <RouterLink
        v-for="chat in chats"
        :key="chat.chatId"
        :to="`/chats/${chat.chatId}`"
        class="card p-4 flex items-center gap-3 hover:bg-gray-50 transition-colors"
      >
        <div class="w-10 h-10 rounded-full bg-teal-50 text-teal-600 flex items-center justify-center font-medium shrink-0">
          <AppIcon name="chat" size="w-5 h-5" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 truncate">{{ chat.adTitle || 'Оголошення' }}</p>
          <p class="text-xs text-gray-400">{{ chat.messageCount }} повідомлень · {{ formatDate(chat.createdAt) }}</p>
        </div>
        <span class="text-gray-400 text-sm">→</span>
      </RouterLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { chatApi } from '@/api/chat'
import type { ChatSummary } from '@/types'
import AppIcon from '@/components/AppIcon.vue'

const chats = ref<ChatSummary[]>([])
const loading = ref(true)

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short' })
}

onMounted(async () => {
  try {
    const { data } = await chatApi.getMyChats()
    chats.value = data
  } finally {
    loading.value = false
  }
})
</script>
