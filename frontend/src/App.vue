<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    <EmailVerifiedBanner />
    <main class="flex-1">
      <RouterView />
    </main>
    <EmailNotVerifiedModal />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import NavBar from '@/components/NavBar.vue'
import EmailVerifiedBanner from '@/components/EmailVerifiedBanner.vue'
import EmailNotVerifiedModal from '@/components/EmailNotVerifiedModal.vue'
import { useAuthStore } from '@/stores/auth'
import { usersApi } from '@/api/users'

const auth = useAuthStore()

onMounted(async () => {
  if (auth.isAuthenticated && auth.userId) {
    try {
      const { data } = await usersApi.getProfile(auth.userId)
      if (data.avatarUrl) auth.updateAvatar(data.avatarUrl)
      auth.setPayoutEnabled(data.isPayoutEnabled ?? false)
    } catch {  }
  }
})
</script>
