<template>
  
  <div v-if="blocked" class="mt-2 rounded-xl overflow-hidden border border-red-200">
    <div class="bg-red-50 px-3 py-2 flex items-start gap-2">
      <AppIcon name="shield" size="w-4 h-4" class="mt-0.5 text-red-600 shrink-0" />
      <div class="flex-1">
        <p class="text-xs font-semibold text-red-700">Антифрод-система Trustee</p>
        <p class="text-xs text-red-600 mt-0.5 leading-relaxed">
          Це повідомлення містить ознаки шахрайства. Відповідно до <strong>Правил платформи</strong>:
        </p>
        <ul class="text-xs text-red-600 mt-1 space-y-0.5 list-none">
          <li>Переведення спілкування у Viber / Telegram / WhatsApp</li>
          <li>Передача номерів карток або телефонів</li>
          <li>Оплата поза платформою Trustee</li>
        </ul>
        <p class="text-xs text-red-500 mt-1.5">
          Усі угоди захищені через безпечну оплату Trustee з HOLD-захистом коштів.
          <span v-if="reason" class="italic"> ({{ reason }})</span>
        </p>
      </div>
    </div>
  </div>

  
  <div v-else-if="flagged" class="mt-2 bg-amber-50 border border-amber-200 rounded-xl px-3 py-2 flex items-start gap-2">
    <AppIcon name="warning" size="w-4 h-4" class="mt-0.5 text-amber-600 shrink-0" />
    <div>
      <p class="text-xs font-semibold text-amber-700">Увага від антифрод-системи</p>
      <p class="text-xs text-amber-600 mt-0.5 leading-relaxed">
        Повідомлення може порушувати правила платформи.
        Не переходьте у сторонні месенджери та не передавайте платіжні дані.
        Використовуйте захищену оплату Trustee.
        <span v-if="reason" class="italic"> ({{ reason }})</span>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '@/components/AppIcon.vue'

defineProps<{
  blocked?: boolean
  flagged?: boolean
  reason?: string | null
}>()
</script>
