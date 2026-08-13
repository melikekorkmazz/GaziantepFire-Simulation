<template>
  <div class="absolute top-4 left-1/2 transform -translate-x-1/2 z-[1000] flex flex-col md:flex-row gap-4 w-11/12 max-w-4xl">
    <div class="relative flex-grow shadow-lg">
      <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
        <MagnifyingGlassIcon class="w-5 h-5 text-gray-500" />
      </div>
      <input type="text" 
             v-model="searchQuery"
             @focus="showDropdown = true"
             @blur="hideDropdownDelayed"
             class="block w-full p-4 pl-10 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white/90 backdrop-blur-sm focus:ring-blue-500 focus:border-blue-500 outline-none" 
             placeholder="İlçe veya Mahalle ara... (Örn: Karataş)" />
             
      <!-- Search Dropdown -->
      <div v-if="showDropdown && filteredResults.length > 0" 
           class="absolute top-full left-0 right-0 mt-2 bg-white rounded-lg shadow-xl border border-gray-100 max-h-64 overflow-y-auto z-50">
        <ul>
          <li v-for="result in filteredResults" :key="result.id"
              @click="selectResult(result)"
              class="px-4 py-3 hover:bg-blue-50 cursor-pointer border-b border-gray-50 last:border-0 flex justify-between items-center transition-colors">
            <span class="font-medium text-gray-800">{{ result.name }}</span>
            <span class="text-xs font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded-full">{{ result.districtName }}</span>
          </li>
        </ul>
      </div>
    </div>
    
    <div class="flex self-center">
      <!-- Harita Modu ve Dropdownlar -->
      <div class="flex shadow-lg rounded-lg overflow-visible bg-white/90 backdrop-blur-sm">
        
        <!-- Tema Dropdown (Standart/Koyu) -->
        <div class="relative">
          <button @click="toggleDropdown('mapStyle')"
                  :class="['px-3 py-2 text-sm font-medium transition-colors rounded-l-lg flex items-center gap-1 border-r border-gray-200', (mapStore.mapMode === 'standard' || mapStore.mapMode === 'dark' || openDropdown === 'mapStyle') ? 'bg-blue-600 text-white' : 'text-gray-700 hover:bg-gray-100']">
            {{ mapStore.mapMode === 'dark' ? 'Koyu' : 'Standart' }} <ChevronDownIcon class="w-4 h-4" />
          </button>
          <div v-if="openDropdown === 'mapStyle'" class="absolute top-full left-0 mt-1 w-32 bg-white rounded-lg shadow-xl border border-gray-100 py-1 z-50">
            <button @click="selectMapStyle('standard')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Standart</button>
            <button @click="selectMapStyle('dark')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Koyu</button>
          </div>
        </div>

        <!-- Uydu Dropdown -->
        <div class="relative">
          <button @click="toggleDropdown('satellite')"
                  :class="['px-3 py-2 text-sm font-medium transition-colors border-l border-gray-200 flex items-center gap-1', (mapStore.mapMode === 'satellite' || openDropdown === 'satellite') ? 'bg-blue-600 text-white' : 'text-gray-700 hover:bg-gray-100']">
            Uydu <ChevronDownIcon class="w-4 h-4" />
          </button>
          <div v-if="openDropdown === 'satellite'" class="absolute top-full left-0 mt-1 w-32 bg-white rounded-lg shadow-xl border border-gray-100 py-1 z-50">
            <button @click="selectSatellite('osm')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Esri (ArcGIS)</button>
            <button @click="selectSatellite('google')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Google</button>
            <button @click="selectSatellite('yandex')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Yandex</button>
          </div>
        </div>

        <!-- Harita Sağlayıcı Dropdown -->
        <div class="relative">
          <button @click="toggleDropdown('provider')"
                  :class="['px-3 py-2 text-sm font-medium transition-colors border-l border-gray-200 flex items-center gap-1', openDropdown === 'provider' ? 'bg-emerald-500 text-white' : 'text-gray-700 hover:bg-gray-100']">
            Sağlayıcı <ChevronDownIcon class="w-4 h-4" />
          </button>
          <div v-if="openDropdown === 'provider'" class="absolute top-full left-0 mt-1 w-32 bg-white rounded-lg shadow-xl border border-gray-100 py-1 z-50">
            <button @click="selectProvider('osm')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">OSM</button>
            <button @click="selectProvider('google')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Google</button>
            <button @click="selectProvider('yandex')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50">Yandex</button>
          </div>
        </div>

        <!-- Trafik Dropdown -->
        <div class="relative">
          <button @click="toggleDropdown('traffic')"
                  :class="['px-3 py-2 text-sm font-medium transition-colors border-l border-gray-200 flex items-center gap-1 rounded-r-lg', (mapStore.trafficProvider !== 'none' || openDropdown === 'traffic') ? 'bg-orange-500 text-white' : 'text-gray-700 hover:bg-gray-100']">
            Trafik <ChevronDownIcon class="w-4 h-4" />
          </button>
          <div v-if="openDropdown === 'traffic'" class="absolute top-full right-0 mt-1 w-32 bg-white rounded-lg shadow-xl border border-gray-100 py-1 z-50">
            <button @click="selectTraffic('none')" class="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 font-medium">Kapat</button>
            <div class="h-px bg-gray-100 my-1"></div>
            <button @click="selectTraffic('google')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-orange-50">Google</button>
            <button @click="selectTraffic('yandex')" class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-orange-50">Yandex</button>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useMapStore } from '../stores/useMapStore'
import { MagnifyingGlassIcon, ChevronDownIcon } from '@heroicons/vue/24/solid'

const mapStore = useMapStore()

const searchQuery = ref('')
const showDropdown = ref(false)
const openDropdown = ref(null)

const filteredResults = computed(() => {
  if (!searchQuery.value || searchQuery.value.trim().length < 2) return []
  
  const query = searchQuery.value.toLowerCase().trim()
  return mapStore.neighborhoods
    .filter(n => n.name.toLowerCase().includes(query) || (n.districtName && n.districtName.toLowerCase().includes(query)))
    .slice(0, 8)
})

function hideDropdownDelayed() {
  setTimeout(() => {
    showDropdown.value = false
  }, 200)
}

function selectResult(result) {
  searchQuery.value = result.name
  showDropdown.value = false
  mapStore.selectNeighborhood(result.id)
}

function toggleDropdown(name) {
  openDropdown.value = openDropdown.value === name ? null : name
}

function selectSatellite(provider) {
  mapStore.setMapMode('satellite')
  mapStore.setMapProvider(provider)
  openDropdown.value = null
}

function selectProvider(provider) {
  mapStore.setMapMode('standard')
  mapStore.setMapProvider(provider)
  openDropdown.value = null
}

function selectTraffic(provider) {
  mapStore.setTrafficProvider(provider)
  openDropdown.value = null
}

function selectMapStyle(style) {
  mapStore.setMapMode(style)
  openDropdown.value = null
}
</script>
