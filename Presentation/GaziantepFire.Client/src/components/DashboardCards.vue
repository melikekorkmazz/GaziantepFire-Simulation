<template>
  <div class="pointer-events-none">

    <!-- Collapsed State: küçük ikon butonu sol alt köşede -->
    <div class="absolute bottom-6 left-6 z-[1000]">
      <button
        v-if="isCollapsed"
        @click="isCollapsed = false"
        title="İstatistikleri Aç"
        class="pointer-events-auto bg-white/95 backdrop-blur-sm p-3 rounded-xl shadow-lg border border-gray-100 hover:bg-gray-50 transition-all duration-200 flex items-center justify-center hover:scale-105"
      >
        <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
      </button>
    </div>

    <!-- Expanded State: kartlar + X butonu -->
    <Transition name="slide-up">
      <div v-if="!isCollapsed" class="absolute bottom-6 left-6 z-[1000] pointer-events-auto">
        <div class="relative">
          <!-- X kapat butonu (sağdaki panel gibi) -->
          <button
            @click="isCollapsed = true"
            title="İstatistikleri Gizle"
            class="absolute -top-3 -right-3 w-7 h-7 bg-white rounded-full shadow-md border border-gray-200 flex items-center justify-center text-gray-500 hover:text-gray-800 z-10 transition-colors hover:bg-gray-50"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>

          <!-- Kartlar -->
          <div class="flex gap-2">

            <!-- Toplam Yangın -->
            <div class="w-[145px] h-[145px] shrink-0 bg-white/95 backdrop-blur-sm rounded-2xl shadow-lg border border-gray-100 p-4 flex flex-col justify-between hover:-translate-y-1 transition-transform duration-200">
              <div class="flex items-center justify-between">
                <h3 class="text-[10px] font-bold text-gray-500 uppercase tracking-wider leading-tight">Toplam<br>Yangın</h3>
                <div class="p-2 bg-red-100 rounded-xl">
                  <FireIcon class="w-4 h-4 text-red-600" />
                </div>
              </div>
              <div>
                <span class="text-3xl font-extrabold text-gray-900">{{ stats.totalFires.toLocaleString('tr-TR') }}</span>
                <div class="flex items-center mt-1">
                  <span class="text-[10px] font-medium text-red-600 flex items-center bg-red-50 px-1.5 py-0.5 rounded-full">
                    <ArrowUpRightIcon class="w-2.5 h-2.5 mr-0.5 stroke-[3]" /> 12%
                  </span>
                  <span class="text-[10px] text-gray-400 ml-1">geçen ay</span>
                </div>
              </div>
            </div>

            <!-- Toplam Kurtarma -->
            <div class="w-[145px] h-[145px] shrink-0 bg-white/95 backdrop-blur-sm rounded-2xl shadow-lg border border-gray-100 p-4 flex flex-col justify-between hover:-translate-y-1 transition-transform duration-200">
              <div class="flex items-center justify-between">
                <h3 class="text-[10px] font-bold text-gray-500 uppercase tracking-wider leading-tight">Toplam<br>Kurtarma</h3>
                <div class="p-2 bg-orange-100 rounded-xl">
                  <LifebuoyIcon class="w-4 h-4 text-orange-600" />
                </div>
              </div>
              <div>
                <span class="text-3xl font-extrabold text-gray-900">{{ stats.totalRescues.toLocaleString('tr-TR') }}</span>
                <div class="flex items-center mt-1">
                  <span class="text-[10px] font-medium text-green-600 flex items-center bg-green-50 px-1.5 py-0.5 rounded-full">
                    <ArrowDownRightIcon class="w-2.5 h-2.5 mr-0.5 stroke-[3]" /> 4%
                  </span>
                  <span class="text-[10px] text-gray-400 ml-1">geçen ay</span>
                </div>
              </div>
            </div>

            <!-- Ortalama Müdahale Süresi -->
            <div class="w-[145px] h-[145px] shrink-0 bg-white/95 backdrop-blur-sm rounded-2xl shadow-lg border border-gray-100 p-4 flex flex-col justify-between hover:-translate-y-1 transition-transform duration-200">
              <div class="flex items-center justify-between">
                <h3 class="text-[10px] font-bold text-gray-500 uppercase tracking-wider leading-tight">Ort.<br>Müdahale</h3>
                <div class="p-2 bg-blue-100 rounded-xl">
                  <ClockIcon class="w-4 h-4 text-blue-600" />
                </div>
              </div>
              <div>
                <span class="text-3xl font-extrabold text-gray-900">{{ stats.averageResponseTimeMinutes?.toFixed(1) || '33.7' }}<span class="text-lg font-semibold text-gray-400 ml-1">dk</span></span>
                <div class="flex items-center mt-1">
                  <span class="text-[10px] font-medium text-green-600 flex items-center bg-green-50 px-1.5 py-0.5 rounded-full">
                    <ArrowDownRightIcon class="w-2.5 h-2.5 mr-0.5 stroke-[3]" /> 0.2dk
                  </span>
                  <span class="text-[10px] text-gray-400 ml-1">hedef: 7dk</span>
                </div>
              </div>
            </div>

            <!-- En Riskli Bölge -->
            <div class="w-[145px] h-[145px] shrink-0 bg-white/95 backdrop-blur-sm rounded-2xl shadow-lg border border-gray-100 p-4 flex flex-col justify-between hover:-translate-y-1 transition-transform duration-200 relative overflow-hidden">
              <div class="absolute bottom-0 left-0 h-1 bg-gradient-to-r from-purple-500 to-fuchsia-500" :style="{ width: `${(stats.mostRiskyNeighborhoodScore / 10) * 100}%` }"></div>
              <div class="flex items-center justify-between">
                <h3 class="text-[10px] font-bold text-gray-500 uppercase tracking-wider leading-tight">En Riskli<br>Bölge</h3>
                <div class="p-2 bg-purple-100 rounded-xl">
                  <ExclamationTriangleIcon class="w-4 h-4 text-purple-600" />
                </div>
              </div>
              <div>
                <span class="text-xl font-extrabold text-gray-900 truncate block">{{ stats.mostRiskyNeighborhoodName }}</span>
                <p class="text-[10px] font-medium text-gray-500 mt-1">Risk: {{ stats.mostRiskyNeighborhoodScore?.toFixed(1) || '0.0' }}/10</p>
              </div>
            </div>

          </div>
        </div>
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useMapStore } from '../stores/useMapStore'
import { 
  FireIcon, 
  LifebuoyIcon, 
  ClockIcon, 
  ExclamationTriangleIcon, 
  ArrowUpRightIcon,
  ArrowDownRightIcon
} from '@heroicons/vue/24/outline'

const mapStore = useMapStore()
const stats = computed(() => mapStore.dashboardStats)
const isCollapsed = ref(false)
</script>

<style scoped>
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.25s ease;
}
.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(20px);
}
</style>
