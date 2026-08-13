<template>
  <!-- Dialog Overlay -->
  <Transition
    enter-active-class="transition-opacity duration-200"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition-opacity duration-150"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div v-if="showDialog" @click.self="closeDialog"
         class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">

      <Transition
        enter-active-class="transition-all duration-300"
        enter-from-class="opacity-0 scale-90 translate-y-4"
        enter-to-class="opacity-100 scale-100 translate-y-0"
        leave-active-class="transition-all duration-200"
        leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-90"
      >
        <div v-if="showDialog" class="bg-white rounded-3xl shadow-2xl w-full max-w-sm p-8">
          <!-- Icon & Title -->
          <div class="flex flex-col items-center text-center mb-8">
            <div class="p-4 bg-emerald-100 rounded-2xl mb-4">
              <span class="text-4xl">📍</span>
            </div>
            <h2 class="text-2xl font-black text-gray-900">İstasyon Önerisi</h2>
            <p class="text-sm text-gray-500 mt-2 leading-relaxed">
              Yapay zeka, coğrafi olay yoğunluğunu ve mesafe analizini kullanarak en optimal istasyon koordinatlarını hesaplar.
            </p>
          </div>

          <!-- Count Selection -->
          <p class="text-xs font-bold text-gray-400 uppercase tracking-wider text-center mb-4">Kaç istasyon önerilsin?</p>
          <div class="grid grid-cols-4 gap-3 mb-8">
            <button
              v-for="n in [1, 2, 3, 5]"
              :key="n"
              @click="selectedCount = n"
              :class="[
                'py-3 rounded-xl text-lg font-black transition-all duration-200',
                selectedCount === n
                  ? 'bg-emerald-500 text-white shadow-lg shadow-emerald-500/40 scale-105'
                  : 'bg-gray-100 text-gray-700 hover:bg-emerald-50 hover:text-emerald-700'
              ]"
            >{{ n }}</button>
          </div>

          <!-- Actions -->
          <div class="flex gap-3">
            <button @click="closeDialog"
                    class="flex-1 py-3.5 rounded-xl border border-gray-200 text-gray-600 font-semibold hover:bg-gray-50 transition-colors">
              İptal
            </button>
            <button @click="confirm"
                    class="flex-1 py-3.5 rounded-xl bg-emerald-500 hover:bg-emerald-600 text-white font-black transition-colors shadow-lg shadow-emerald-500/30 flex items-center justify-center gap-2">
              <SparklesIcon class="w-5 h-5" />
              Analiz Et
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </Transition>

  <!-- Suggestion Results Card (slides in from right) -->
  <Transition
    enter-active-class="transform transition-transform duration-350 ease-out"
    enter-from-class="translate-x-full"
    enter-to-class="translate-x-0"
    leave-active-class="transform transition-transform duration-200 ease-in"
    leave-from-class="translate-x-0"
    leave-to-class="translate-x-full"
  >
    <div v-if="mapStore.isSuggestionCardOpen && !showDialog"
         class="absolute top-20 right-4 z-[1050] w-80 md:w-96 max-h-[calc(100vh-180px)] flex flex-col bg-white/95 backdrop-blur-md rounded-2xl shadow-2xl border border-gray-100 overflow-hidden">

      <!-- Card Header -->
      <div class="flex items-center justify-between px-5 py-4 border-b border-gray-100 flex-shrink-0">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-emerald-100 rounded-xl">
            <SparklesIcon class="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <h3 class="text-sm font-black text-gray-900">İstasyon Önerileri</h3>
            <p class="text-xs text-gray-400">{{ mapStore.suggestionCount }} Optimal Konum</p>
          </div>
        </div>
        <button @click="mapStore.clearSuggestions()"
                class="text-gray-400 hover:text-gray-600 bg-gray-100 hover:bg-gray-200 p-1.5 rounded-full transition-colors">
          <XMarkIcon class="w-5 h-5" />
        </button>
      </div>

      <!-- Loading skeleton -->
      <div v-if="mapStore.loadingSuggestions" class="p-5 space-y-4">
        <div v-for="i in mapStore.suggestionCount" :key="i"
             class="animate-pulse p-4 rounded-xl bg-gray-100">
          <div class="h-4 bg-gray-200 rounded w-1/2 mb-3"></div>
          <div class="h-3 bg-gray-200 rounded w-full mb-2"></div>
          <div class="grid grid-cols-2 gap-2 mt-3">
            <div class="h-12 bg-gray-200 rounded-lg"></div>
            <div class="h-12 bg-gray-200 rounded-lg"></div>
          </div>
        </div>
      </div>

      <!-- Suggestion List -->
      <div v-else class="overflow-y-auto flex-grow p-4 space-y-3">
        <div
          v-for="suggestion in mapStore.stationSuggestions"
          :key="suggestion.index"
          class="bg-gradient-to-br from-emerald-50 to-white border border-emerald-100 rounded-xl p-4"
        >
          <!-- Index header -->
          <div class="flex items-center gap-2 mb-2">
            <span class="text-xl leading-none">⭐</span>
            <span class="text-sm font-black text-emerald-900">Öneri #{{ suggestion.index }}</span>
            <span class="ml-auto text-xs font-mono text-gray-400">
              {{ suggestion.latitude.toFixed(4) }}°N, {{ suggestion.longitude.toFixed(4) }}°E
            </span>
          </div>

          <!-- Reason -->
          <p class="text-xs text-gray-600 leading-relaxed mb-3 border-l-2 border-emerald-300 pl-2">
            {{ suggestion.reason }}
          </p>

          <!-- Stats -->
          <div class="grid grid-cols-2 gap-2">
            <div class="bg-orange-50 border border-orange-100 rounded-lg p-2.5">
              <p class="text-xs text-orange-600 font-semibold">Mevcut Süre</p>
              <p class="text-xl font-extrabold text-orange-800">{{ suggestion.currentAvgResponseTime }}<span class="text-xs font-normal ml-0.5">dk</span></p>
            </div>
            <div class="bg-emerald-50 border border-emerald-200 rounded-lg p-2.5">
              <p class="text-xs text-emerald-600 font-semibold">Tahmini Yeni</p>
              <p class="text-xl font-extrabold text-emerald-800">{{ suggestion.estimatedNewResponseTime }}<span class="text-xs font-normal ml-0.5">dk</span></p>
            </div>
          </div>

          <!-- Improvement pill -->
          <div class="mt-2 flex justify-end">
            <span class="inline-flex items-center gap-1 bg-emerald-500/10 text-emerald-700 text-xs font-bold px-2 py-0.5 rounded-full">
              ▼ {{ ((1 - suggestion.estimatedNewResponseTime / suggestion.currentAvgResponseTime) * 100).toFixed(0) }}% iyileşme
            </span>
          </div>
        </div>
      </div>

    </div>
  </Transition>
</template>

<script setup>
import { ref } from 'vue'
import { useMapStore } from '../stores/useMapStore'
import { SparklesIcon, XMarkIcon } from '@heroicons/vue/24/solid'

const mapStore = useMapStore()

const showDialog = ref(false)
const selectedCount = ref(3)

function open() {
  showDialog.value = true
  selectedCount.value = 3
}

function closeDialog() {
  showDialog.value = false
}

async function confirm() {
  showDialog.value = false
  await mapStore.loadStationSuggestions(selectedCount.value)
}

// Expose open() so FilterBar can call it
defineExpose({ open })
</script>
