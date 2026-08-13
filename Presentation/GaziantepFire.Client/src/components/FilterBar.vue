<template>
  <div class="absolute right-4 top-24 z-[1000] flex flex-col gap-4">
    
    <!-- Collapsed State Button -->
    <button v-if="isCollapsed" @click="isCollapsed = false" title="Filtreleri Aç" class="bg-white/95 backdrop-blur-sm p-3 rounded-xl shadow-lg border border-gray-100 hover:bg-gray-50 transition-colors flex items-center justify-center">
      <svg class="w-6 h-6 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"></path></svg>
    </button>

    <!-- Expanded State Panels -->
    <div v-else class="flex flex-col gap-4 relative">
      <!-- Close Button -->
      <button @click="isCollapsed = true" title="Filtreleri Gizle" class="absolute -top-3 -right-3 w-7 h-7 bg-white rounded-full shadow-md border border-gray-200 flex items-center justify-center text-gray-500 hover:text-gray-800 z-10 transition-colors hover:bg-gray-50">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
      </button>

      <!-- Layers Panel -->
      <div class="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg p-4 w-64 border border-gray-100">
        <h3 class="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3 flex items-center">
          <span class="mr-2">🗺️</span> Katmanlar
        </h3>
        <div class="flex flex-col gap-3">
          <label class="flex items-center space-x-3 cursor-pointer group">
            <div class="relative flex items-center">
              <input type="checkbox" :checked="mapStore.layers.districts" @change="mapStore.toggleLayer('districts')" class="peer sr-only">
              <div class="w-5 h-5 rounded border border-gray-300 bg-white peer-checked:bg-blue-500 peer-checked:border-blue-500 transition-colors flex items-center justify-center">
                <svg class="w-3 h-3 text-white opacity-0 peer-checked:opacity-100" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path></svg>
              </div>
            </div>
            <span class="text-sm font-medium text-gray-700 group-hover:text-blue-600 transition-colors">İlçeler</span>
          </label>
          
          <label class="flex items-center space-x-3 cursor-pointer group">
            <div class="relative flex items-center">
              <input type="checkbox" :checked="mapStore.layers.neighborhoods" @change="mapStore.toggleLayer('neighborhoods')" class="peer sr-only">
              <div class="w-5 h-5 rounded border border-gray-300 bg-white peer-checked:bg-blue-500 peer-checked:border-blue-500 transition-colors flex items-center justify-center">
                <svg class="w-3 h-3 text-white opacity-0 peer-checked:opacity-100" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path></svg>
              </div>
            </div>
            <span class="text-sm font-medium text-gray-700 group-hover:text-blue-600 transition-colors">Mahalleler</span>
          </label>
          
          <label class="flex items-center space-x-3 cursor-pointer group">
            <div class="relative flex items-center">
              <input type="checkbox" :checked="mapStore.layers.stations" @change="mapStore.toggleLayer('stations')" class="peer sr-only">
              <div class="w-5 h-5 rounded border border-gray-300 bg-white peer-checked:bg-blue-500 peer-checked:border-blue-500 transition-colors flex items-center justify-center">
                <svg class="w-3 h-3 text-white opacity-0 peer-checked:opacity-100" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path></svg>
              </div>
            </div>
            <span class="text-sm font-medium text-gray-700 group-hover:text-blue-600 transition-colors">İstasyonlar</span>
          </label>
        </div>
      </div>

      <!-- Date Filter Panel -->
      <div class="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg p-4 w-64 border border-gray-100">
        <h3 class="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3 flex items-center">
          <span class="mr-2">📅</span> Tarih Filtresi
        </h3>
        <div class="flex flex-col gap-3">
          
          <!-- Başlangıç -->
          <div>
            <label class="block text-[10px] font-medium text-gray-500 uppercase mb-1">Başlangıç Tarihi</label>
            <div class="flex gap-1">
              <select v-model="startDay" class="w-14 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Gün</option>
                <option v-for="d in 31" :key="d" :value="String(d).padStart(2, '0')">{{ d }}</option>
              </select>
              <select v-model="startMonth" class="flex-1 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Ay</option>
                <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
              </select>
              <select v-model="startYear" class="w-16 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Yıl</option>
                <option v-for="y in [2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023, 2024, 2025, 2026]" :key="y" :value="String(y)">{{ y }}</option>
              </select>
            </div>
          </div>

          <!-- Bitiş -->
          <div>
            <label class="block text-[10px] font-medium text-gray-500 uppercase mb-1">Bitiş Tarihi</label>
            <div class="flex gap-1">
              <select v-model="endDay" class="w-14 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Gün</option>
                <option v-for="d in 31" :key="d" :value="String(d).padStart(2, '0')">{{ d }}</option>
              </select>
              <select v-model="endMonth" class="flex-1 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Ay</option>
                <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
              </select>
              <select v-model="endYear" class="w-16 text-[11px] p-1 border border-gray-200 rounded text-gray-700 outline-none focus:border-blue-400">
                <option value="">Yıl</option>
                <option v-for="y in [2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023, 2024, 2025, 2026]" :key="y" :value="String(y)">{{ y }}</option>
              </select>
            </div>
          </div>

          <div class="flex gap-2 mt-1">
            <button @click="applyDateFilter" class="flex-1 bg-blue-500 hover:bg-blue-600 text-white text-xs font-bold py-1.5 rounded-lg shadow-sm transition-colors">
              Filtrele
            </button>
            <button v-if="mapStore.dateFilter.start || mapStore.dateFilter.end || startDay || endDay" @click="clearDateFilter" class="px-2.5 bg-red-50 hover:bg-red-100 text-red-600 text-xs font-bold rounded-lg border border-red-100 transition-colors" title="Filtreyi Temizle">
              Sil
            </button>
          </div>
        </div>
      </div>

      <!-- Analysis Panel -->
      <div class="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg p-4 w-64 border border-gray-100">
        <h3 class="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3 flex items-center">
          <span class="mr-2">📊</span> Analizler
        </h3>
        <div class="flex flex-col gap-2">
          <button @click="mapStore.toggleAnalysis('fire')" 
                  :class="['w-full text-left px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200', mapStore.analysis.fire ? 'bg-red-500 text-white shadow-md shadow-red-500/30' : 'text-gray-700 hover:bg-red-50 hover:text-red-600']">
            🔥 Yangın
          </button>
          <button @click="mapStore.toggleAnalysis('rescue')" 
                  :class="['w-full text-left px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200', mapStore.analysis.rescue ? 'bg-orange-500 text-white shadow-md shadow-orange-500/30' : 'text-gray-700 hover:bg-orange-50 hover:text-orange-600']">
            ⛑️ Kurtarma
          </button>
          <button @click="mapStore.toggleAnalysis('risk')" 
                  :class="['w-full text-left px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200', mapStore.analysis.risk ? 'bg-purple-500 text-white shadow-md shadow-purple-500/30' : 'text-gray-700 hover:bg-purple-50 hover:text-purple-600']">
            ⚠️ Risk Haritası
          </button>
          <button @click="$emit('openSuggestionDialog')" 
                  :class="['w-full text-left px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200', mapStore.analysis.station_proposal ? 'bg-emerald-500 text-white shadow-md shadow-emerald-500/30' : 'text-gray-700 hover:bg-emerald-50 hover:text-emerald-700']">
            📍 İstasyon Önerisi
          </button>
          <button @click="mapStore.toggleSimulationMode()" 
                  :class="['w-full text-left px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200 mt-2 border border-dashed', mapStore.isSimulationMode ? 'bg-indigo-500 text-white shadow-md shadow-indigo-500/30 border-indigo-400 animate-pulse' : 'text-indigo-600 bg-indigo-50 border-indigo-200 hover:bg-indigo-100']">
            {{ mapStore.isSimulationMode ? '🛑 Simülasyonu Kapat' : '🎯 Simülasyon Modu' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useMapStore } from '../stores/useMapStore'

const mapStore = useMapStore()
const emit = defineEmits(['openSuggestionDialog'])
const isCollapsed = ref(false)

const startDay = ref('')
const startMonth = ref('')
const startYear = ref('')
const endDay = ref('')
const endMonth = ref('')
const endYear = ref('')

const months = [
  { value: '01', label: 'Ocak' }, { value: '02', label: 'Şubat' }, { value: '03', label: 'Mart' },
  { value: '04', label: 'Nisan' }, { value: '05', label: 'Mayıs' }, { value: '06', label: 'Haziran' },
  { value: '07', label: 'Temmuz' }, { value: '08', label: 'Ağustos' }, { value: '09', label: 'Eylül' },
  { value: '10', label: 'Ekim' }, { value: '11', label: 'Kasım' }, { value: '12', label: 'Aralık' }
]

function applyDateFilter() {
  let start = null
  let end = null
  
  if (startYear.value) {
    const m = startMonth.value || '01'
    const d = startDay.value || '01'
    start = `${startYear.value}-${m}-${d}`
  }
  
  if (endYear.value) {
    const m = endMonth.value || '12'
    let d = endDay.value
    if (!d) {
      const lastDay = new Date(endYear.value, parseInt(m), 0).getDate()
      d = String(lastDay).padStart(2, '0')
    }
    end = `${endYear.value}-${m}-${d}`
  }
  
  if (!start && !end) {
    mapStore.setDateFilter(null, null)
    return
  }
  
  mapStore.setDateFilter(start, end)
}

function clearDateFilter() {
  startDay.value = ''
  startMonth.value = ''
  startYear.value = ''
  endDay.value = ''
  endMonth.value = ''
  endYear.value = ''
  mapStore.setDateFilter(null, null)
}

// Initial hydration from store if any
onMounted(() => {
  if (mapStore.dateFilter.start) {
    const [y, m, d] = mapStore.dateFilter.start.split('-')
    if (y && m && d) {
      startYear.value = y
      startMonth.value = m
      startDay.value = d
    }
  }
  if (mapStore.dateFilter.end) {
    const [y, m, d] = mapStore.dateFilter.end.split('-')
    if (y && m && d) {
      endYear.value = y
      endMonth.value = m
      endDay.value = d
    }
  }
})
</script>
