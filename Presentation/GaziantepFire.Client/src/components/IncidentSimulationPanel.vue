<template>
  <div v-if="mapStore.isSimulationPanelOpen" 
       class="absolute top-24 left-4 w-80 bg-white dark:bg-slate-800 rounded-xl shadow-2xl overflow-hidden z-[1001] transition-all duration-300">
    
    <!-- Header -->
    <div class="bg-gradient-to-r from-red-600 to-orange-500 p-4 flex justify-between items-center text-white">
      <div class="flex items-center gap-2">
        <span class="text-xl">🔥</span>
        <h3 class="font-semibold">Olay Simülasyonu</h3>
      </div>
      <button @click="mapStore.closeSimulationPanel()" class="text-white hover:text-red-200 transition">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
        </svg>
      </button>
    </div>

    <!-- Content -->
    <div class="p-4">
      <div v-if="mapStore.loadingSimulation" class="py-8 flex flex-col items-center justify-center text-slate-500">
        <div class="w-8 h-8 border-4 border-red-500 border-t-transparent rounded-full animate-spin mb-3"></div>
        <p class="text-sm">En yakın istasyonlar aranıyor...</p>
      </div>
      
      <div v-else-if="mapStore.simulationResults?.stations?.length > 0">
        <div class="text-sm text-slate-600 mb-3 bg-slate-100 p-2 rounded">
          <p><b>Konum:</b> {{ mapStore.simulatedLocation?.lat.toFixed(4) }}, {{ mapStore.simulatedLocation?.lng.toFixed(4) }}</p>
        </div>

        <div class="mb-4">
          <label class="block text-xs font-bold text-slate-700 uppercase tracking-wide mb-2">Olay Türü</label>
          <select v-model="incidentType" class="w-full bg-white border border-slate-300 text-slate-700 rounded-lg p-2 outline-none focus:border-red-400 focus:ring-1 focus:ring-red-400 text-sm shadow-sm transition">
            <option value="bina">Bina Yangını (Konut/İşyeri)</option>
            <option value="arazi">Arazi / Ot Yangını</option>
            <option value="kaza">Trafik Kazası / Sıkışma</option>
            <option value="hayvan">Hayvan Kurtarma</option>
            <option value="tehlikeli">Tehlikeli Madde Sızıntısı (KBRN)</option>
          </select>
        </div>

        <h4 class="text-sm font-bold text-slate-700 mb-2 uppercase tracking-wide">Müdahale Ekipleri</h4>
        
        <div class="space-y-3">
          <div v-for="(result, index) in mapStore.simulationResults.stations" :key="result.stationId"
               class="bg-white border border-slate-100 shadow-sm rounded-lg p-3 flex items-center justify-between"
               :class="{'border-red-200 bg-red-50': index === 0}">
            
            <div class="flex items-center gap-3">
              <div class="w-6 h-6 rounded-full flex items-center justify-center font-bold text-xs"
                   :class="index === 0 ? 'bg-red-500 text-white' : 'bg-slate-200 text-slate-600'">
                {{ index + 1 }}
              </div>
              <div>
                <p class="font-medium text-slate-800 text-sm leading-tight">{{ result.stationName }}</p>
                <p class="text-xs text-slate-500 mt-0.5">{{ result.distanceInKm }} km mesafe</p>
              </div>
            </div>
            
            <div class="text-right">
              <p class="font-bold text-lg leading-tight" :class="index === 0 ? 'text-red-600' : 'text-slate-700'">
                {{ result.estimatedTimeInMinutes }}<span class="text-xs font-normal"> dk</span>
              </p>
            </div>
          </div>
        </div>
        
        <div class="mt-4 pt-3 border-t border-slate-200">
          <div class="flex justify-between items-center mb-3">
            <span class="text-sm font-semibold text-slate-600">İlk Ekip Varış:</span>
            <span class="text-lg font-black text-red-600">{{ mapStore.simulationResults.stations[0]?.estimatedTimeInMinutes }} dk</span>
          </div>
          
          <div v-if="mapStore.simulationResults.recommendedVehicles?.length > 0">
            <h4 class="text-xs font-bold text-slate-700 mb-2 uppercase tracking-wide">Önerilen Araçlar</h4>
            <div class="flex flex-wrap gap-2">
              <span v-for="(vehicle, idx) in mapStore.simulationResults.recommendedVehicles" :key="idx" 
                    class="bg-blue-50 text-blue-700 border border-blue-200 text-xs px-2 py-1 rounded-md font-medium">
                {{ vehicle }}
              </span>
            </div>
          </div>
        </div>
      </div>
      
      <div v-else class="py-6 text-center text-slate-500 text-sm">
        Sonuç bulunamadı.
      </div>
    </div>
  </div>
</template>

<script setup>
import { useMapStore } from '../stores/useMapStore'
import { ref, watch } from 'vue'

const mapStore = useMapStore()

const incidentType = ref('bina')

const vehicleRecommendations = {
  'bina': ['🚒 2 İtfaiye Aracı', '🚒 1 Merdivenli Araç', '🚑 1 Ambulans'],
  'arazi': ['🚒 1 Arazöz', '💧 1 Su Tankeri'],
  'kaza': ['🧰 1 Kurtarma Aracı', '🚑 2 Ambulans', '🚓 1 Trafik Ekibi'],
  'hayvan': ['🧰 1 Kurtarma Aracı', '🐾 1 Uzman Ekip'],
  'tehlikeli': ['☣️ 1 KBRN Müdahale Aracı', '🚒 2 İtfaiye Aracı', '🚑 1 Ambulans']
}

watch(incidentType, (newVal) => {
  if (mapStore.simulationResults) {
     mapStore.simulationResults.recommendedVehicles = vehicleRecommendations[newVal];
  }
})

watch(() => mapStore.simulationResults, (newResults) => {
  if (newResults) {
     newResults.recommendedVehicles = vehicleRecommendations[incidentType.value];
  }
})
</script>
