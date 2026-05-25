import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: () => import('@/views/HomeView.vue') },
    { path: '/map', name: 'map', component: () => import('@/views/MapView.vue') },
    { path: '/ads/create', name: 'create-ad', component: () => import('@/views/CreateAdView.vue'), meta: { requiresAuth: true } },
    { path: '/ads/:id/edit', name: 'edit-ad', component: () => import('@/views/EditAdView.vue'), meta: { requiresAuth: true } },
    { path: '/ads/:id', name: 'ad-detail', component: () => import('@/views/AdDetailView.vue') },
    { path: '/login', name: 'login', component: () => import('@/views/LoginView.vue') },
    { path: '/register', name: 'register', component: () => import('@/views/RegisterView.vue') },
    { path: '/verify-email-sent', name: 'verify-email-sent', component: () => import('@/views/VerifyEmailSentView.vue') },
    { path: '/verify-email', name: 'verify-email', component: () => import('@/views/VerifyEmailView.vue') },
    { path: '/welcome', name: 'welcome', component: () => import('@/views/WelcomeView.vue'), meta: { requiresAuth: true } },
    { path: '/forgot-password', name: 'forgot-password', component: () => import('@/views/ForgotPasswordView.vue') },
    { path: '/reset-password', name: 'reset-password', component: () => import('@/views/ResetPasswordView.vue') },
    { path: '/chats', name: 'chats', component: () => import('@/views/ChatsView.vue'), meta: { requiresAuth: true } },
    { path: '/chats/:id', name: 'chat', component: () => import('@/views/ChatView.vue'), meta: { requiresAuth: true } },
    { path: '/profile', name: 'profile', component: () => import('@/views/ProfileView.vue'), meta: { requiresAuth: true } },
    { path: '/users/:id', name: 'public-profile', component: () => import('@/views/PublicProfileView.vue') },
    { path: '/offers', name: 'offers', component: () => import('@/views/MyOffersView.vue'), meta: { requiresAuth: true } },
    { path: '/orders', name: 'orders', component: () => import('@/views/OrdersView.vue'), meta: { requiresAuth: true } },
    { path: '/delivery/:orderId', name: 'delivery-setup', component: () => import('@/views/DeliverySetupView.vue'), meta: { requiresAuth: true } },
    { path: '/payment/success', name: 'payment-success', component: () => import('@/views/PaymentSuccessView.vue') },
    { path: '/payment/cancel', name: 'payment-cancel', component: () => import('@/views/PaymentCancelView.vue') },
    { path: '/checkout/:adId', name: 'checkout', component: () => import('@/views/CheckoutView.vue'), meta: { requiresAuth: true } },
    { path: '/orders/:orderId/success', name: 'order-success', component: () => import('@/views/OrderSuccessView.vue'), meta: { requiresAuth: true } }
  ]
})

router.beforeEach(to => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
})

export default router
