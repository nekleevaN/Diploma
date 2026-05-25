<template>
  <div>
    <label
      class="flex flex-col items-center justify-center w-full h-28 border-2 border-dashed border-gray-300 rounded-xl cursor-pointer hover:bg-gray-50 transition-colors"
      :class="{ 'opacity-50': uploading }"
    >
      <span v-if="uploading" class="text-sm text-gray-400">Завантаження...</span>
      <template v-else>
        <AppIcon name="camera" size="w-8 h-8" class="text-gray-400" />
        <span class="text-sm text-gray-500 mt-1">Натисни або перетягни фото</span>
        <span class="text-xs text-gray-400">JPG, PNG до 10 МБ</span>
      </template>
      <input type="file" class="hidden" accept="image/*" :multiple="multiple" @change="handleFiles" :disabled="uploading" />
    </label>

    <div v-if="uploadedUrls.length" class="flex flex-wrap gap-2 mt-3">
      <div v-for="(url, i) in uploadedUrls" :key="i" class="relative">
        <img :src="url" class="w-20 h-20 object-cover rounded-lg border border-gray-200" />
        <button @click="remove(i)"
          class="absolute -top-1.5 -right-1.5 w-5 h-5 bg-red-500 text-white rounded-full text-xs flex items-center justify-center">
          ×
        </button>
      </div>
    </div>

    <p v-if="error" class="text-xs text-red-500 mt-1">{{ error }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { usersApi } from '@/api/users'
import AppIcon from '@/components/AppIcon.vue'

const props = defineProps<{ folder?: string; multiple?: boolean }>()
const emit = defineEmits<{ uploaded: [urls: string[]] }>()

const uploading = ref(false)
const uploadedUrls = ref<string[]>([])
const error = ref('')

async function handleFiles(e: Event) {
  const files = (e.target as HTMLInputElement).files
  if (!files?.length) return
  uploading.value = true
  error.value = ''
  try {
    for (const file of Array.from(files)) {
      const { data } = await usersApi.upload(file, props.folder ?? 'ads')
      uploadedUrls.value.push(data.url)
    }
    emit('uploaded', [...uploadedUrls.value])
  } catch {
    error.value = 'Помилка завантаження'
  } finally {
    uploading.value = false
  }
}

function remove(i: number) {
  uploadedUrls.value.splice(i, 1)
  emit('uploaded', [...uploadedUrls.value])
}
</script>
