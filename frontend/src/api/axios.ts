import axios from 'axios'
import { useEmailVerificationStore } from '@/stores/emailVerification'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  res => res,
  err => {
    const status = err.response?.status
    const code   = err.response?.data?.code

    if (status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('userId')
      window.location.href = '/login'
      return Promise.reject(err)
    }

    if (status === 403 && code === 'EMAIL_NOT_VERIFIED') {
      try {
        const { triggerModal } = useEmailVerificationStore()
        triggerModal()
      } catch {  }
    }

    return Promise.reject(err)
  }
)

export default api
