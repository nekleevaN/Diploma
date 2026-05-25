import api from './axios'

export interface NPCity {
  ref: string
  description: string
  area: string
  region: string
}

export interface NPWarehouse {
  ref: string
  description: string
  number: string
  shortAddress: string
}

export interface DeliveryDto {
  deliveryId: string
  orderId: string
  status: string
  recipientCityName?: string | null
  recipientWarehouseAddress?: string | null
  recipientName?: string | null
  recipientPhone?: string | null
  senderCityName?: string | null
  senderWarehouseAddress?: string | null
  senderName?: string | null
  ttn?: string | null
  trackingStatus?: string | null
  trackingStatusDescription?: string | null
  estimatedDeliveryDate?: string | null
}

export const deliveryApi = {
  searchCities: (q: string) =>
    api.get<NPCity[]>('/delivery/cities', { params: { q } }),

  getWarehouses: (cityRef: string, page = 1, q?: string) =>
    api.get<NPWarehouse[]>('/delivery/warehouses', { params: { cityRef, page, q } }),

  setRecipientAddress: (orderId: string, data: {
    cityRef: string; cityName: string
    warehouseRef: string; warehouseAddress: string
    recipientName: string; recipientPhone: string
  }) => api.post(`/delivery/${orderId}/recipient`, data),

  setSenderAddress: (orderId: string, data: {
    cityRef: string; cityName: string
    warehouseRef: string; warehouseAddress: string
    senderName: string; senderPhone: string
  }) => api.post(`/delivery/${orderId}/sender`, data),

  generateTTN: (orderId: string) =>
    api.post<{ ttn: string }>(`/delivery/${orderId}/generate-ttn`),

  getDelivery: (orderId: string) =>
    api.get<DeliveryDto>(`/delivery/${orderId}`),

  track: (orderId: string) =>
    api.get<DeliveryDto>(`/delivery/${orderId}/track`)
}
