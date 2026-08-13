<template>
  <Transition
    enter-active-class="transform transition-transform duration-300 ease-out"
    enter-from-class="-translate-x-full"
    enter-to-class="translate-x-0"
    leave-active-class="transform transition-transform duration-200 ease-in"
    leave-from-class="translate-x-0"
    leave-to-class="-translate-x-full"
  >
    <div v-if="mapStore.isDetailCardOpen && !mapStore.isSimulationMode" 
         class="absolute top-20 left-4 z-[1050] w-80 md:w-96 bg-white/95 backdrop-blur-md rounded-2xl shadow-2xl border border-gray-100 p-6 text-gray-800">
      
      <!-- Close Button -->
      <button @click="mapStore.closeDetailCard()" 
              class="absolute top-4 right-4 text-gray-400 hover:text-gray-600 bg-gray-100 hover:bg-gray-200 p-1.5 rounded-full transition-colors">
        <XMarkIcon class="w-5 h-5" />
      </button>

      <!-- Skeleton Loading -->
      <div v-if="mapStore.loading" class="animate-pulse space-y-4 py-2">
        <div class="h-6 bg-gray-200 rounded w-3/4"></div>
        <div class="h-4 bg-gray-200 rounded w-1/2"></div>
        <div class="grid grid-cols-2 gap-3 pt-4">
          <div class="h-20 bg-gray-200 rounded-xl"></div>
          <div class="h-20 bg-gray-200 rounded-xl"></div>
          <div class="h-20 bg-gray-200 rounded-xl"></div>
          <div class="h-20 bg-gray-200 rounded-xl"></div>
        </div>
      </div>

      <!-- Detail Content -->
      <div v-else-if="details" class="flex flex-col gap-4">
        
        <!-- Header & Risk Badge -->
        <div>
          <div class="flex items-center gap-2">
            <span class="px-2.5 py-0.5 rounded-full text-xs font-bold uppercase tracking-wider text-white shadow-sm"
                  :class="getRiskBadgeColor(details.riskLevel)">
              Risk: {{ details.riskLevel }}/10
            </span>
            <span class="text-xs font-semibold text-gray-400 uppercase">{{ details.districtName }}</span>
          </div>
          <h2 class="text-2xl font-black text-gray-900 mt-1">{{ details.name }}</h2>
        </div>

        <hr class="border-gray-100 my-1" />

        <!-- 4 Grid Cards -->
        <div class="grid grid-cols-2 gap-3">
          
          <!-- Yangın Sayısı -->
          <div class="bg-red-50/70 border border-red-100 p-3.5 rounded-xl">
            <div class="flex items-center justify-between text-red-600">
              <span class="text-xs font-bold uppercase">Yangın</span>
              <FireIcon class="w-4 h-4" />
            </div>
            <p class="text-2xl font-extrabold text-red-900 mt-1">{{ details.fireCount }} <span class="text-xs font-normal text-red-700">Vaka</span></p>
          </div>

          <!-- Kurtarma Sayısı -->
          <div class="bg-orange-50/70 border border-orange-100 p-3.5 rounded-xl">
            <div class="flex items-center justify-between text-orange-600">
              <span class="text-xs font-bold uppercase">Kurtarma</span>
              <LifebuoyIcon class="w-4 h-4" />
            </div>
            <p class="text-2xl font-extrabold text-orange-900 mt-1">{{ details.rescueCount }} <span class="text-xs font-normal text-orange-700">Vaka</span></p>
          </div>

          <!-- Ort. Ulaşma Süresi -->
          <div class="bg-blue-50/70 border border-blue-100 p-3.5 rounded-xl col-span-2">
            <div class="flex items-center justify-between text-blue-600">
              <span class="text-xs font-bold uppercase">Ortalama Ulaşma Süresi</span>
              <ClockIcon class="w-4 h-4" />
            </div>
            <p class="text-2xl font-extrabold text-blue-900 mt-1">
              {{ details.averageResponseTimeMinutes }} <span class="text-sm font-semibold text-blue-700">Dakika</span>
            </p>
          </div>

        </div>

        <!-- En Yakın İstasyon Card -->
        <div class="bg-emerald-50/80 border border-emerald-100 p-4 rounded-xl flex items-start gap-3">
          <div class="p-2 bg-emerald-500 text-white rounded-lg shadow-sm">
            <BuildingOffice2Icon class="w-5 h-5" />
          </div>
          <div>
            <h4 class="text-xs font-bold text-emerald-800 uppercase tracking-wider">En Yakın İstasyon</h4>
            <p class="text-sm font-extrabold text-emerald-950 mt-0.5">{{ details.nearestStationName }}</p>
          </div>
        </div>

      </div>
    </div>
  </Transition>
</template>

<script setup>
import { computed } from 'vue'
import { useMapStore } from '../stores/useMapStore'
import { 
  XMarkIcon, 
  FireIcon, 
  LifebuoyIcon, 
  ClockIcon, 
  BuildingOffice2Icon 
} from '@heroicons/vue/24/solid'

const mapStore = useMapStore()

const details = computed(() => mapStore.selectedNeighborhoodDetails)

function getRiskBadgeColor(level) {
  if (level >= 8.0) return 'bg-red-600'
  if (level >= 6.0) return 'bg-orange-500'
  return 'bg-emerald-500'
}
</script>
