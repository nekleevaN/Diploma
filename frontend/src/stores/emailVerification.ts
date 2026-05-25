import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useEmailVerificationStore = defineStore('emailVerification', () => {
  const showModal = ref(false)

  function triggerModal() { showModal.value = true }
  function closeModal()   { showModal.value = false }

  return { showModal, triggerModal, closeModal }
})
