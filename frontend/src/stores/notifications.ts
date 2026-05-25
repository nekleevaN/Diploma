import { defineStore } from 'pinia'
import { ref } from 'vue'
import { offersApi } from '@/api/offers'
import { reviewsApi } from '@/api/reviews'

export const useNotificationsStore = defineStore('notifications', () => {
  const pendingOffersCount = ref(0)
  const pendingReviewsCount = ref(0)

  async function refresh() {
    try {
      const { data } = await offersApi.getPendingCount()
      pendingOffersCount.value = data.count
    } catch { }
    try {
      const { data } = await reviewsApi.getMyPending()
      pendingReviewsCount.value = data.length
    } catch { }
  }

  return { pendingOffersCount, pendingReviewsCount, refresh }
})
