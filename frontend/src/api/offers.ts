import api from './axios'

export interface OfferDto {
  offerId: string
  advertisementId: string
  buyerId: string
  buyerName: string
  offeredPrice: number
  status: 'Pending' | 'Accepted' | 'Rejected' | 'CounterOffered'
  counterPrice?: number | null
  sellerNote?: string | null
  createdAt: string
}

export const offersApi = {
  makeOffer: (adId: string, offeredPrice: number) =>
    api.post<{ offerId: string }>(`/ads/${adId}/offers`, { offeredPrice }),

  getAdOffers: (adId: string) =>
    api.get<OfferDto[]>(`/ads/${adId}/offers`),

  getMyOffers: () =>
    api.get<OfferDto[]>('/offers/my'),

  getPendingCount: () =>
    api.get<{ count: number }>('/offers/pending-count'),

  respond: (offerId: string, action: 'accept' | 'reject' | 'counter', counterPrice?: number, note?: string) =>
    api.put(`/offers/${offerId}/respond`, { action, counterPrice, note }),

  acceptCounter: (offerId: string) =>
    api.post<{ agreedPrice: number }>(`/offers/${offerId}/accept-counter`)
}
