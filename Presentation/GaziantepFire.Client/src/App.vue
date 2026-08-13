<script setup>
import MapContainer from './components/MapContainer.vue'
import TopSearchBar from './components/TopSearchBar.vue'
import FilterBar from './components/FilterBar.vue'
import DashboardCards from './components/DashboardCards.vue'
import NeighborhoodDetailCard from './components/NeighborhoodDetailCard.vue'
import StationSuggestionCard from './components/StationSuggestionCard.vue'
import { ref } from 'vue'

const stationSuggestionCard = ref(null)

function openSuggestionDialog() {
  stationSuggestionCard.value?.open()
}
</script>

<template>
  <div class="h-screen w-full relative overflow-hidden font-sans bg-gray-50">
    <MapContainer />
    <TopSearchBar />
    <FilterBar @open-suggestion-dialog="openSuggestionDialog" />
    <NeighborhoodDetailCard />
    <StationSuggestionCard ref="stationSuggestionCard" />
    <DashboardCards />
  </div>
</template>
