import api from './axios'
import type { ChatDetail, ChatSummary, SendMessageResult } from '@/types'

export const chatApi = {
  startChat: (sellerId: string, advertisementId: string, adTitle?: string) =>
    api.post<{ chatId: string; isNew: boolean }>('/chats', { sellerId, advertisementId, adTitle }),

  getMyChats: () =>
    api.get<ChatSummary[]>('/chats'),

  getById: (chatId: string) =>
    api.get<ChatDetail>(`/chats/${chatId}`),

  sendMessage: (chatId: string, content: string) =>
    api.post<SendMessageResult>(`/chats/${chatId}/messages`, { content })
}
