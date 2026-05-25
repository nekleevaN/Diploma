import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from './auth'

export const useChatStore = defineStore('chat', () => {
  const connection = ref<signalR.HubConnection | null>(null)

  async function connect() {
    const auth = useAuthStore()
    if (!auth.token || connection.value?.state === signalR.HubConnectionState.Connected) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => auth.token! })
      .withAutomaticReconnect()
      .build()

    await connection.value.start()
  }

  async function disconnect() {
    await connection.value?.stop()
    connection.value = null
  }

  async function joinChat(chatId: string) {
    await connect()
    await connection.value?.invoke('JoinChat', chatId)
  }

  function onReceiveMessage(callback: (msg: unknown) => void) {
    connection.value?.on('ReceiveMessage', callback)
  }

  function onMessageBlocked(callback: (info: unknown) => void) {
    connection.value?.on('MessageBlocked', callback)
  }

  function offAll() {
    connection.value?.off('ReceiveMessage')
    connection.value?.off('MessageBlocked')
  }

  async function sendMessage(chatId: string, content: string) {
    await connect()
    await connection.value?.invoke('SendMessage', chatId, content)
  }

  return { connection, connect, disconnect, joinChat, sendMessage, onReceiveMessage, onMessageBlocked, offAll }
})
