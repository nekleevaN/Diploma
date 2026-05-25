import api from './axios'

export interface ReviewDto {
  reviewId: string
  reviewType: string
  rating: number
  comment?: string | null
  isAnonymous: boolean
  reviewerName?: string | null
  reviewerId: string
  descriptionAccuracy?: number | null
  shippingSpeed?: number | null
  communication?: number | null
  publishedAt: string
}

export interface ReviewsPageDto {
  items: ReviewDto[]
  totalCount: number
  page: number
  pageSize: number
}

export interface RatingStatsDto {
  average: number
  count: number
  distribution: Record<number, number>
}

export interface UserRatingStatsDto {
  asSeller: RatingStatsDto
  asBuyer: RatingStatsDto
  pendingCount: number
}

export interface PendingReviewDto {
  reviewId: string
  orderId: string
  revieweeId: string
  reviewType: string
  expiresAt: string
}

export const reviewsApi = {
  submit: (reviewId: string, data: {
    rating: number
    comment?: string
    isAnonymous: boolean
    descriptionAccuracy?: number
    shippingSpeed?: number
    communication?: number
  }) => api.post(`/reviews/${reviewId}/submit`, data),

  update: (reviewId: string, data: {
    rating: number
    comment?: string
    isAnonymous: boolean
    descriptionAccuracy?: number
    shippingSpeed?: number
    communication?: number
  }) => api.put(`/reviews/${reviewId}`, data),

  getUserReviews: (userId: string, params?: {
    type?: string
    page?: number
    pageSize?: number
    sort?: string
  }) => api.get<ReviewsPageDto>(`/reviews/users/${userId}`, { params }),

  getRatingStats: (userId: string) =>
    api.get<UserRatingStatsDto>(`/reviews/users/${userId}/stats`),

  getMyPending: () =>
    api.get<PendingReviewDto[]>('/reviews/my/pending'),

  initOrderReviews: (orderId: string, data: {
    buyerId: string
    sellerId: string
    buyerName: string
    sellerName: string
  }) => api.post<{ reviewId: string }>(`/reviews/orders/${orderId}/init`, data),

  getMySubmitted: () =>
    api.get<string[]>('/reviews/my/submitted'),
}
