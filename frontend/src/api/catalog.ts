import api from './axios'
import type { Ad, AdListItem, PagedResult } from '@/types'

export const catalogApi = {
  getAll: (params: {
    category?: string; categorySub?: string; categoryItem?: string
    search?: string; condition?: string; brand?: string
    priceMin?: number; priceMax?: number; sortBy?: string
    size?: string; color?: string
    page?: number; pageSize?: number
  }) => api.get<PagedResult<AdListItem>>('/ads', { params }),

  getById: (id: string) =>
    api.get<Ad>(`/ads/${id}`),

  create: (data: {
    title: string; description: string; price: number; category: string
    categorySub?: string; categoryItem?: string; categoryLabel?: string
    condition?: string; brand?: string; size?: string; color?: string
    latitude?: number; longitude?: number; locationAddress?: string
  }) => api.post<{ advertisementId: string }>('/ads', data),

  addImage: (adId: string, url: string) =>
    api.post(`/ads/${adId}/images`, { url }),

  updateAd: (id: string, data: {
    title: string; description: string; price: number; category: string
    categorySub?: string; categoryItem?: string; categoryLabel?: string
    condition?: string; brand?: string; size?: string; color?: string
    latitude?: number; longitude?: number; locationAddress?: string
    clearLocation?: boolean
  }) => api.put(`/ads/${id}`, data),

  deleteAd: (id: string) =>
    api.delete(`/ads/${id}`),

  getMapAds: () =>
    api.get<MapAdDto[]>('/ads/map'),

  getBySeller: (sellerId: string) =>
    api.get<any[]>(`/ads/by-seller/${sellerId}`)
}

export interface MapAdDto {
  id: string
  title: string
  price: number
  category: string
  sellerId: string
  sellerName: string
  imageUrls: string[]
  latitude: number
  longitude: number
  locationAddress?: string | null
}
