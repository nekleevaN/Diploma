import api from './axios'

export interface UserProfile {
  userId: string
  username: string
  displayName: string
  firstName: string
  lastName: string
  avatarUrl?: string | null
  bio?: string | null
  rating: number
  sellerRating: number
  sellerReviewCount: number
  buyerRating: number
  buyerReviewCount: number
  badges: string[]
  joinedAt: string
  monobankSubMerchantId?: string | null
  isPayoutEnabled?: boolean
}


export const usersApi = {
  getProfile: (userId: string) =>
    api.get<UserProfile>(`/users/${userId}`),

  uploadAvatar: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post<{ avatarUrl: string }>('/users/avatar', form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },

  updateProfile: (bio: string, trustedContactTelegramId?: number, trustedContactEmail?: string) =>
    api.put<{ bio: string; trustedContactTelegramId?: number; trustedContactEmail?: string }>(
      '/users/me/profile', { bio, trustedContactTelegramId, trustedContactEmail }),

  setPayoutMethod: (monobankSubMerchantId: string | null) =>
    api.put<{ monobankSubMerchantId: string | null; payoutEnabled: boolean; message: string }>(
      '/users/me/payout', { monobankSubMerchantId }),

  upload: (file: File, folder = 'ads') => {
    const form = new FormData()
    form.append('file', file)
    return api.post<{ url: string }>(`/upload?folder=${folder}`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  }
}
