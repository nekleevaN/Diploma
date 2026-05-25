import api from './axios'

export const viewingApi = {
  propose: (data: {
    chatId: string
    advertisementId: string
    responderId: string
    adTitle: string
    locationAddress?: string | null
    proposedDateTime: string
    proposerTrustedTelegramId?: number | null
    proposerTrustedEmail?: string | null
  }) => api.post<{ viewingId: string }>('/viewings', data),

  respond: (viewingId: string, data: {
    action: 'accept' | 'decline' | 'reschedule'
    newDateTime?: string
    responderTrustedTelegramId?: number | null
    responderTrustedEmail?: string | null
    proposerName?: string
  }) => api.put(`/viewings/${viewingId}/respond`, data),

  followUp: (viewingId: string, action: 'buy' | 'buy_delivery' | 'cancelled') =>
    api.put(`/viewings/${viewingId}/followup`, { action })
}
