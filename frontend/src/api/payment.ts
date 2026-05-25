import api from './axios'

export interface OrderDto {
  orderId: string
  advertisementId: string
  buyerId: string
  sellerId: string
  adTitle: string
  amount: number
  status: 'Pending' | 'Hold' | 'Completed' | 'Cancelled' | 'Refunded' | 'Failed' | 'Expired' | 'AwaitingConfirmation'
  hasDelivery: boolean
  createdAt: string
  updatedAt?: string | null
}

export interface CheckoutOrderRequest {
  advertisementId: string
  amount: number
  recipientCityRef: string
  recipientCityName: string
  recipientWarehouseRef: string
  recipientWarehouseAddress: string
  recipientFirstName: string
  recipientLastName: string
  recipientPhone: string
}

export const paymentApi = {
  createCheckoutOrder: (data: CheckoutOrderRequest) =>
    api.post<{ orderId: string; monoPageUrl: string }>('/orders', data),

  createOrder: (advertisementId: string, sellerId: string, adTitle: string, amount: number, hasDelivery = true) =>
    api.post<{ orderId: string; pageUrl: string }>('/payment/create', {
      advertisementId, sellerId, adTitle, amount, hasDelivery
    }),

  finalizeOrder: (orderId: string) =>
    api.post(`/payment/${orderId}/finalize`),

  cancelOrder: (orderId: string) =>
    api.post(`/payment/${orderId}/cancel`),

  getMyBuyerOrders: () =>
    api.get<OrderDto[]>('/payment/my/buyer'),

  getMySellerOrders: () =>
    api.get<OrderDto[]>('/payment/my/seller'),

  getOrder: (orderId: string) =>
    api.get<OrderDto>(`/payment/${orderId}`),

  syncStatus: (orderId: string) =>
    api.post<{ status: string; message: string }>(`/payment/${orderId}/sync-status`),

  confirmReceipt: (orderId: string) =>
    api.post<{ message: string }>(`/payment/${orderId}/confirm-receipt`),

  refundOrder: (orderId: string) =>
    api.post<{ message: string }>(`/payment/${orderId}/refund`)
}
