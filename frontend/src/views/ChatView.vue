<template>
  <div class="max-w-2xl mx-auto px-4 py-6 flex flex-col" style="height: calc(100vh - 56px)">
    <div class="flex items-center gap-3 mb-4">
      <RouterLink to="/chats" class="text-gray-400 hover:text-gray-600">←</RouterLink>
      <h1 class="font-semibold text-gray-900 truncate">{{ chatTitle || 'Переписка' }}</h1>
      <span class="text-xs text-gray-400 ml-auto flex items-center gap-1"><AppIcon name="shield" size="w-3.5 h-3.5" /> Антифрод захист увімкнено</span>
    </div>

    <div ref="messagesEl" class="flex-1 overflow-y-auto space-y-3 pr-1">
      <div v-if="loading" class="flex justify-center py-8">
        <span class="text-gray-400 text-sm">Завантаження...</span>
      </div>

      <template v-else>
        <div
          v-for="msg in allMessages"
          :key="msg.id"
          :class="['flex', msg.senderId === auth.userId ? 'justify-end' : 'justify-start']"
        >
          <div class="max-w-xs lg:max-w-md">
            
            <ViewingMessageCard
              v-if="isViewingMessage(msg.content)"
              :raw-content="msg.content"
              :sender-id="msg.senderId"
              :current-user-id="auth.userId!"
              :responder-id="chatResponderId"
              :ad-id="chatAdId"
              :partner-id="chatResponderId"
              @responded="reloadMessages"
            />
            
            <template v-else>
              <div :class="[
                'rounded-2xl px-4 py-2.5 text-sm',
                msg.isBlocked
                  ? 'bg-gray-100 border border-red-200 text-gray-700'
                  : msg.isFlagged
                    ? 'bg-yellow-50 border border-amber-200 text-gray-800'
                    : msg.senderId === auth.userId
                      ? 'bg-teal-500 text-white'
                      : 'bg-white border border-gray-100 text-gray-800'
              ]">
                <p>{{ msg.content }}</p>
                <p :class="['text-xs mt-1 opacity-60',
                  msg.senderId === auth.userId && !msg.isBlocked && !msg.isFlagged ? 'text-blue-200' : 'text-gray-400']">
                  {{ msg.time }}
                </p>
              </div>
              <FraudWarning
                v-if="msg.isFlagged || msg.isBlocked"
                :blocked="msg.isBlocked"
                :flagged="msg.isFlagged"
                :reason="msg.fraudWarning"
              />
            </template>
          </div>
        </div>
      </template>
    </div>

    
    <div v-if="showViewingModal" class="mb-2 bg-white border border-gray-200 rounded-xl p-4 space-y-2">
      <p class="text-sm font-semibold text-gray-800 flex items-center gap-1"><AppIcon name="calendar" size="w-4 h-4" /> Запропонувати дату перегляду</p>
      <input v-model="viewingDate" type="date" :min="todayDate" class="input text-sm" />
      <input v-model="viewingTime" type="time" class="input text-sm" />
      <div class="flex gap-2">
        <button @click="sendViewingProposal" :disabled="!viewingDate || !viewingTime || viewingSending"
          class="btn-primary flex-1 text-sm">
          <template v-if="viewingSending">...</template>
          <template v-else><AppIcon name="bell" size="w-3.5 h-3.5" class="inline mr-1" />Надіслати</template>
        </button>
        <button @click="showViewingModal = false" class="btn-secondary text-sm">Скасувати</button>
      </div>
    </div>

    <div class="flex gap-2 mt-2">
      
      <button v-if="hasViewingFeature"
        @click="showViewingModal = !showViewingModal"
        :class="['flex items-center gap-1.5 px-2.5 py-1.5 text-xs font-medium rounded-lg border transition-colors shrink-0',
          showViewingModal
            ? 'bg-teal-500 text-white border-teal-500'
            : 'text-teal-600 border-teal-200 hover:bg-teal-50 bg-white']"
        title="Запропонувати час перегляду">
        <AppIcon name="calendar" size="w-4 h-4" /> <span class="hidden sm:inline">Час зустрічі</span>
      </button>
      <input
        v-model="newMessage"
        @keydown.enter.prevent="send"
        type="text"
        class="input flex-1"
        placeholder="Написати повідомлення..."
        :disabled="sending"
      />
      <button @click="send" :disabled="!newMessage.trim() || sending" class="btn-primary px-4">
        <span v-if="sending">...</span>
        <span v-else>↑</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { chatApi } from '@/api/chat'
import { catalogApi } from '@/api/catalog'
import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'
import FraudWarning from '@/components/FraudWarning.vue'
import ViewingMessageCard from '@/components/ViewingMessageCard.vue'
import AppIcon from '@/components/AppIcon.vue'
import { viewingApi } from '@/api/viewing'
import type { Message } from '@/types'

interface LocalMessage {
  id: string
  senderId: string
  content: string
  time: string
  isBlocked: boolean
  isFlagged: boolean
  fraudWarning?: string | null
}

const route = useRoute()
const chatStore = useChatStore()
const auth = useAuthStore()
const chatId = route.params.id as string
const chatTitle = ref('')
const chatResponderId = ref('')

const historicMessages = ref<LocalMessage[]>([])
const liveMessages = ref<LocalMessage[]>([])
const loading = ref(true)
const newMessage = ref('')
const sending = ref(false)
const messagesEl = ref<HTMLElement | null>(null)
const lastBlocked = ref<string | null>(null)

const showViewingModal = ref(false)
const viewingDate = ref('')
const viewingTime = ref('')
const viewingSending = ref(false)
const todayDate = new Date().toISOString().split('T')[0]
const chatAdId = ref('')
const chatAdTitle = ref('')
const chatAdLocation = ref<string | null>(null)

const hasViewingFeature = computed(() => {
  if (route.query.fromViewing === '1') return true
  return allMessages.value.some(m => m.content.startsWith('{"type":"viewing_'))
})

function isViewingMessage(content: string): boolean {
  return content.startsWith('{"type":"viewing_')
}

async function sendViewingProposal() {
  if (!viewingDate.value || !viewingTime.value) return
  viewingSending.value = true
  try {
    const proposedDateTime = new Date(`${viewingDate.value}T${viewingTime.value}`).toISOString()
    await viewingApi.propose({
      chatId,
      advertisementId: chatAdId.value,
      responderId: chatResponderId.value,
      adTitle: chatAdTitle.value,
      locationAddress: chatAdLocation.value,
      proposedDateTime,
      proposerTrustedTelegramId: auth.trustedContactTelegramId ?? undefined,
      proposerTrustedEmail: auth.trustedContactEmail || undefined
    })
    showViewingModal.value = false
    viewingDate.value = ''
    viewingTime.value = ''
    await reloadMessages()
  } finally {
    viewingSending.value = false
  }
}

async function reloadMessages() {
  const { data } = await chatApi.getById(chatId)
  historicMessages.value = data.messages.map(toLocal)
  await scrollToBottom()
}

const allMessages = computed(() => [...historicMessages.value, ...liveMessages.value])

function toLocal(msg: Message): LocalMessage {
  return {
    id: msg.messageId,
    senderId: msg.senderId,
    content: msg.content,
    time: new Date(msg.sentAt).toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }),
    isBlocked: msg.isBlocked,
    isFlagged: msg.isFlagged,
    fraudWarning: msg.fraudWarning
  }
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

async function send() {
  const content = newMessage.value.trim()
  if (!content || sending.value) return

  sending.value = true
  newMessage.value = ''

  try {
    const { data } = await chatApi.sendMessage(chatId, content)

    liveMessages.value.push({
      id: data.messageId,
      senderId: auth.userId!,
      content,
      time: new Date().toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }),
      isBlocked: data.isBlocked,
      isFlagged: data.isFlagged,
      fraudWarning: (data.isBlocked || data.isFlagged) ? data.fraudReason : null
    })
    await scrollToBottom()
  } catch {
    newMessage.value = content
  } finally {
    sending.value = false
  }
}

onMounted(async () => {
  try {
    const { data } = await chatApi.getById(chatId)
    historicMessages.value = data.messages.map(toLocal)
    chatTitle.value = (data as any).adTitle || ''
    chatAdId.value = (data as any).advertisementId || ''
    chatAdTitle.value = (data as any).adTitle || ''
    const d = data as any
    chatResponderId.value = d.buyerId === auth.userId ? d.sellerId : d.buyerId
    await scrollToBottom()
    if (chatAdId.value) {
      try {
        const { data: ad } = await catalogApi.getById(chatAdId.value)
        chatAdLocation.value = (ad as any).locationAddress ?? null
      } catch { }
    }
  } finally {
    loading.value = false
  }

  await chatStore.joinChat(chatId)

  chatStore.onReceiveMessage((payload) => {
    const msg = payload as { messageId: string; senderId: string; content: string; isFlagged?: boolean; fraudWarning?: string }
    if (msg.senderId === auth.userId) return
    liveMessages.value.push({
      id: msg.messageId ?? crypto.randomUUID(),
      senderId: msg.senderId,
      content: msg.content,
      time: new Date().toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }),
      isBlocked: false,
      isFlagged: !!msg.isFlagged,
      fraudWarning: msg.fraudWarning
    })
    scrollToBottom()
  })
})

onUnmounted(() => {
  chatStore.offAll()
})
</script>
