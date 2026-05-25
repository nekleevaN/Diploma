export interface AuthResponse {
  userId: string
  token: string
}

export interface RegisterResponse {
  userId: string
  token: string
  message: string
}

export interface VerifyEmailResponse {
  userId: string
  token: string
  redirectTo: string
}

export interface ResetPasswordResponse {
  userId: string
  token: string
}

export interface Ad {
  id: string
  title: string
  description: string
  price: number
  category: string
  categorySub?: string | null
  categoryItem?: string | null
  categoryLabel?: string | null
  condition?: string | null
  brand?: string | null
  size?: string | null
  color?: string | null
  sellerId: string
  sellerName: string
  sellerRating: number
  status: string
  createdAt: string
  imageUrls: string[]
  latitude?: number | null
  longitude?: number | null
  locationAddress?: string | null
  isPayoutEnabled?: boolean
}

export interface AdListItem {
  id: string
  title: string
  price: number
  category: string
  categorySub?: string | null
  categoryItem?: string | null
  categoryLabel?: string | null
  condition?: string | null
  brand?: string | null
  size?: string | null
  color?: string | null
  sellerId: string
  sellerName: string
  sellerRating: number
  status: string
  imageUrls: string[]
  latitude?: number | null
  longitude?: number | null
  locationAddress?: string | null
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface Message {
  messageId: string
  senderId: string
  content: string
  sentAt: string
  isBlocked: boolean
  isFlagged: boolean
  fraudWarning?: string | null
}

export interface ChatSummary {
  chatId: string
  buyerId: string
  sellerId: string
  advertisementId: string
  adTitle: string
  messageCount: number
  createdAt: string
}

export interface ChatDetail {
  chatId: string
  buyerId: string
  sellerId: string
  advertisementId: string
  messages: Message[]
}

export interface SendMessageResult {
  messageId: string
  fraudScore: number
  fraudReason?: string | null
  isBlocked: boolean
  isFlagged: boolean
}
