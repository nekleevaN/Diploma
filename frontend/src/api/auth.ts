import api from './axios'
import type { AuthResponse, RegisterResponse, VerifyEmailResponse, ResetPasswordResponse } from '@/types'

export const authApi = {
  register: (data: {
    firstName: string; lastName: string; email: string
    password: string; passwordConfirm: string
    agreeToTerms: boolean; wantsNewsletter: boolean
    website?: string; formOpenedAt?: number
  }) => api.post<RegisterResponse>('/auth/register', data),

  login: (email: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { email, password }),

  verifyEmail: (token: string) =>
    api.get<VerifyEmailResponse>(`/auth/verify-email?token=${encodeURIComponent(token)}`),

  resendVerification: () =>
    api.post<{ message: string }>('/auth/resend-verification'),

  forgotPassword: (email: string) =>
    api.post<{ message: string }>('/auth/forgot-password', { email }),

  resetPassword: (token: string, newPassword: string, confirmPassword: string) =>
    api.post<ResetPasswordResponse>('/auth/reset-password',
      { token, newPassword, confirmPassword }),

  checkEmail: (email: string) =>
    api.get<{ available: boolean }>(`/auth/check-email?email=${encodeURIComponent(email)}`),

  googleAuth: (idToken: string) =>
    api.post<{ userId: string; token: string; isNewUser: boolean }>(
      '/auth/google', { idToken }),

  startDiia: () =>
    api.post<{ sessionId: string }>('/users/verify/diia/start'),

  confirmDiia: (sessionId: string) =>
    api.post('/users/verify/diia/confirm', { sessionId }),
}
