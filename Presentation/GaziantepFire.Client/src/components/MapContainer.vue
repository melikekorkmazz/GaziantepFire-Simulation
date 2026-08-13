<template>
  <div class="absolute inset-0 z-0">
    <div id="map" class="w-full h-full"></div>
    <IncidentSimulationPanel />
  </div>
</template>

<script setup>
import { onMounted, ref, watch, onUnmounted, shallowRef } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import 'leaflet.markercluster'
import 'leaflet.heat'
import { useMapStore } from '../stores/useMapStore'
import IncidentSimulationPanel from './IncidentSimulationPanel.vue'

const mapStore = useMapStore()
const map = shallowRef(null)
const currentTileLayer = shallowRef(null)
const trafficLayer = shallowRef(null)
const districtLayers = shallowRef({})
const neighborhoodLayers = shallowRef({})
const stationMarkers = shallowRef([])
const suggestionMarkers = shallowRef([])
const simulationLayer = shallowRef(null)
const fireClusterLayer = shallowRef(null)
const rescueLayer = shallowRef(null)
const heatmapLayer = shallowRef(null)

const tiles = {
  // Standart sağlayıcılar
  standard_osm:     'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
  standard_google:  'https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}',
  standard_yandex:  'https://core-renderer-tiles.maps.yandex.net/tiles?l=map&v=21.06.18-0-b210616160705&x={x}&y={y}&z={z}&scale=1&lang=tr_TR',
  // Uydu sağlayıcılar
  satellite_esri:   'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
  satellite_google: 'https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}',
  satellite_yandex: 'https://core-sat.maps.yandex.net/tiles?l=sat&v=3.449.0&x={x}&y={y}&z={z}&scale=1',
  // Koyu tema
  dark: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
}

const TRAFFIC_TILES = {
  // Şeffaf trafik katmanları
  google: 'https://mt1.google.com/vt?lyrs=h,traffic|seconds_into_week:-1&style=3&x={x}&y={y}&z={z}',
  // Yandex doğrudan tile desteğini kestiği için şeffaf API'ye yönlendiriliyor
  yandex: 'https://mt1.google.com/vt?lyrs=h,traffic|seconds_into_week:-1&style=3&x={x}&y={y}&z={z}'
}

function getRiskColor(level) {
  if (level >= 8.0) return '#ef4444'
  if (level >= 6.0) return '#f97316'
  return '#22c55e'
}

function renderDistrictPolygons() {
  Object.values(districtLayers.value).forEach(layer => {
    map.value.removeLayer(layer)
  })

  districtLayers.value = {}

  if (!mapStore.layers.districts) return

  mapStore.districts.forEach(district => {
    const layer = L.geoJSON(district.geojson, {
      style: {
        color: '#111827',
        weight: 3,
        opacity: 0.9,
        fillColor: '#ffffff',
        fillOpacity: 0
      }
    })

    layer.bindTooltip(
      `<b>📍 ${district.name}</b>`,
      {
        direction: 'center',
        sticky: true,
        opacity: 0.95
      }
    )

    layer.on('mouseover', function () {
      this.setStyle({
        color: '#dc2626',
        weight: 5,
        opacity: 1
      })
      this.bringToFront()
    })

    layer.on('mouseout', function () {
      this.setStyle({
        color: '#111827',
        weight: 3,
        opacity: 0.9
      })
    })

    layer.addTo(map.value)
    districtLayers.value[district.id] = layer
  })
}

function renderNeighborhoodPolygons() {
  Object.values(neighborhoodLayers.value).forEach(l => map.value.removeLayer(l))
  neighborhoodLayers.value = {}

  if (!mapStore.layers.neighborhoods) return

  mapStore.neighborhoods.forEach(neighborhood => {
    try {
      const geojson = JSON.parse(neighborhood.polygonBoundaryGeoJson)
      const layer = L.geoJSON(geojson, {
        style: {
          color: '#2563eb',
          fillColor: '#3b82f6',
          fillOpacity: 0.05,
          weight: 2
        }
      })
      .bindTooltip(`<b>${neighborhood.name}</b><br>Risk: ${neighborhood.riskLevel}/10`, {
        direction: 'top',
        sticky: true
      })
      .on('click', () => {
        mapStore.selectNeighborhood(neighborhood.id)
      })

      layer.addTo(map.value)
      neighborhoodLayers.value[neighborhood.id] = layer
    } catch (e) {
      // skip invalid GeoJSON
    }
  })
}

function updateNeighborhoodStyles() {
  if (!mapStore.neighborhoods) return
  
  mapStore.neighborhoods.forEach(neighborhood => {
    const layer = neighborhoodLayers.value[neighborhood.id]
    if (!layer) return

    layer.setStyle({
      color: '#2563eb',
      fillColor: '#2563eb',
      fillOpacity: 0.05
    })
  })
}

// No frontend randomization functions anymore

function renderFires() {
  if (fireClusterLayer.value) {
    map.value.removeLayer(fireClusterLayer.value)
    fireClusterLayer.value = null
  }

  if (!mapStore.analysis.fire) return

  // Load fires if not loaded
  if (mapStore.fires.length === 0) {
    mapStore.loadFires().then(() => {
      if (mapStore.analysis.fire) renderFires()
    })
    return
  }

  // Kümeleme iptal edildi, doğrudan katman grubu oluşturuluyor
  fireClusterLayer.value = L.featureGroup();

  console.log(`[DEBUG] Frontend fire records received: ${mapStore.fires.length}`);
  let renderedCount = 0;

  const firesToRender = mapStore.filteredFires;

  firesToRender.forEach(fire => {
    let lat = fire.latitude;
    let lng = fire.longitude;

    if (lat && lng) {
      // Görseldeki gibi basit, performanslı kırmızı nokta (CircleMarker)
      const marker = L.circleMarker([lat, lng], {
        radius: 5,
        fillColor: "#ef4444",
        color: "#ffffff",
        weight: 1.5,
        opacity: 1,
        fillOpacity: 0.9
      }).bindPopup(`
          <div style="min-width: 150px; font-family: sans-serif;">
            <strong style="color: #ef4444;">🔥 Yangın İhbarı</strong><br />
            Risk: ${fire.subType || 'Bilinmiyor'}<br />
            Mahalle: ${fire.neighborhoodName}
          </div>
        `);
      
      fireClusterLayer.value.addLayer(marker);
      renderedCount++;
    }
  });

  console.log(`[DEBUG] Rendered fire dots: ${renderedCount}`);

  map.value.addLayer(fireClusterLayer.value);
}

function renderRescues() {
  if (rescueLayer.value) {
    map.value.removeLayer(rescueLayer.value)
    rescueLayer.value = null
  }

  if (!mapStore.analysis.rescue) return

  if (mapStore.rescues.length === 0) {
    mapStore.loadRescues().then(() => {
      if (mapStore.analysis.rescue) renderRescues()
    })
    return
  }

  rescueLayer.value = L.featureGroup();

  let renderedCount = 0;
  const rescuesToRender = mapStore.filteredRescues;

  rescuesToRender.forEach(rescue => {
    let lat = rescue.latitude;
    let lng = rescue.longitude;

    if (lat && lng) {
      const marker = L.circleMarker([lat, lng], {
        radius: 5,
        fillColor: "#f97316", // Orange
        color: "#ffffff",
        weight: 1.5,
        opacity: 1,
        fillOpacity: 0.9
      }).bindPopup(`
          <div style="min-width: 150px; font-family: sans-serif;">
            <strong style="color: #f97316;">⛑️ Kurtarma Olayı</strong><br />
            Tür: ${rescue.subType || 'Bilinmiyor'}<br />
            Tarih: ${new Date(rescue.createdAt).toLocaleDateString('tr-TR')}<br />
            Mahalle: ${rescue.neighborhoodName}
          </div>
        `);
      
      rescueLayer.value.addLayer(marker)
      renderedCount++
    }
  })

  console.log(`[DEBUG] Rendered rescue dots: ${renderedCount}`)

  map.value.addLayer(rescueLayer.value)
}

function renderHeatmap() {
  if (heatmapLayer.value) {
    map.value.removeLayer(heatmapLayer.value)
    heatmapLayer.value = null
  }

  if (!mapStore.analysis.risk) return

  const fireCluster = L.markerClusterGroup({
    chunkedLoading: true,
    maxClusterRadius: 50,
    iconCreateFunction: function (cluster) {
      return L.divIcon({
        html: `<div style="background-color: rgba(239, 68, 68, 0.7); border-radius: 50%; width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; border: 2px solid #ef4444; color: white; font-weight: bold; box-shadow: 0 0 10px rgba(239, 68, 68, 0.6); text-shadow: 1px 1px 2px rgba(0,0,0,0.8);">${cluster.getChildCount()}</div>`,
        className: 'custom-cluster',
        iconSize: [40, 40]
      });
    }
  });

  const rescueCluster = L.markerClusterGroup({
    chunkedLoading: true,
    maxClusterRadius: 50,
    iconCreateFunction: function (cluster) {
      return L.divIcon({
        html: `<div style="background-color: rgba(249, 115, 22, 0.7); border-radius: 50%; width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; border: 2px solid #f97316; color: white; font-weight: bold; box-shadow: 0 0 10px rgba(249, 115, 22, 0.6); text-shadow: 1px 1px 2px rgba(0,0,0,0.8);">${cluster.getChildCount()}</div>`,
        className: 'custom-cluster',
        iconSize: [40, 40]
      });
    }
  });

  let hasData = false;

  mapStore.filteredFires.forEach(f => {
    if (f.latitude && f.longitude) {
      const marker = L.circleMarker([f.latitude, f.longitude], {
        radius: 6,
        fillColor: "#ef4444",
        color: "#ffffff",
        weight: 1.5,
        opacity: 1,
        fillOpacity: 0.9
      }).bindPopup(`
          <div style="min-width: 150px; font-family: sans-serif;">
            <strong style="color: #ef4444;">🔥 Yangın</strong><br />
            Tür: ${f.subType || 'Bilinmiyor'}<br />
            Mahalle: ${f.neighborhoodName}
          </div>
        `);
      fireCluster.addLayer(marker);
      hasData = true;
    }
  });
  
  mapStore.filteredRescues.forEach(r => {
    if (r.latitude && r.longitude) {
      const marker = L.circleMarker([r.latitude, r.longitude], {
        radius: 6,
        fillColor: "#f97316",
        color: "#ffffff",
        weight: 1.5,
        opacity: 1,
        fillOpacity: 0.9
      }).bindPopup(`
          <div style="min-width: 150px; font-family: sans-serif;">
            <strong style="color: #f97316;">⛑️ Kurtarma</strong><br />
            Tür: ${r.subType || 'Bilinmiyor'}<br />
            Mahalle: ${r.neighborhoodName}
          </div>
        `);
      rescueCluster.addLayer(marker);
      hasData = true;
    }
  });

  if (!hasData) return;

  heatmapLayer.value = L.layerGroup([fireCluster, rescueCluster]);
  map.value.addLayer(heatmapLayer.value);
}


function renderSuggestionMarkers() {
  // Clear existing suggestion markers
  suggestionMarkers.value.forEach(m => map.value.removeLayer(m))
  suggestionMarkers.value = []

  if (!mapStore.stationSuggestions || mapStore.stationSuggestions.length === 0) return

  mapStore.stationSuggestions.forEach((suggestion, index) => {
    const starIcon = L.divIcon({
      html: `<div style="
        font-size: 32px;
        line-height: 1;
        filter: drop-shadow(0 2px 4px rgba(0,0,0,0.4));
        transform: translate(-50%, -100%);
      ">⭐</div>`,
      className: '',
      iconAnchor: [0, 0],
      popupAnchor: [0, -32]
    })

    const marker = L.marker([suggestion.latitude, suggestion.longitude], { icon: starIcon })
      .bindPopup(`
        <div style="min-width:180px;font-family:sans-serif">
          <b style="color:#16a34a;font-size:14px">📍 Öneri #${index + 1}</b><br>
          <small style="color:#6b7280">${suggestion.reason}</small><br><br>
          <table style="width:100%;font-size:12px">
            <tr><td style="color:#9ca3af">Mevcut Süre</td><td><b>${suggestion.currentAvgResponseTime} dk</b></td></tr>
            <tr><td style="color:#9ca3af">Tahmini Yeni</td><td><b style="color:#16a34a">${suggestion.estimatedNewResponseTime} dk</b></td></tr>
          </table>
        </div>
      `, { maxWidth: 220 })
      .addTo(map.value)

    suggestionMarkers.value.push(marker)
  })

  // Fit map to suggestions
  if (mapStore.stationSuggestions.length > 0) {
    const group = L.featureGroup(suggestionMarkers.value)
    map.value.fitBounds(group.getBounds().pad(0.3))
  }
}

function renderStations() {
  stationMarkers.value.forEach(m => map.value.removeLayer(m))
  stationMarkers.value = []

  if (!mapStore.layers.stations) return

  mapStore.stations.forEach(station => {
    const stationIcon = L.divIcon({
      html: `
        <div style="background-color: #dc2626; width: 32px; height: 32px; border-radius: 10px; border: 2px solid white; box-shadow: 0 4px 6px rgba(220, 38, 38, 0.4); display: flex; align-items: center; justify-content: center;">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="width: 18px; height: 18px;">
            <path d="M12 2L4 5v5.82a10.97 10.97 0 0 0 8 10.18 10.97 10.97 0 0 0 8-10.18V5l-8-3z" />
            <path d="M12 8c-1.5 1.5-2.5 3-2.5 4.5S10.5 15 12 15s2.5-1 2.5-2.5S13.5 9.5 12 8z" fill="white"/>
          </svg>
        </div>
      `,
      className: '',
      iconSize: [32, 32],
      iconAnchor: [16, 16]
    })

    const marker = L.marker([station.latitude, station.longitude], { icon: stationIcon })
      .bindTooltip(`<b>${station.name}</b>`, { direction: 'top' })
      .bindPopup(`
        <div style="min-width: 150px; font-family: sans-serif; text-align: center;">
          <h4 style="margin: 0; padding-bottom: 5px; border-bottom: 1px solid #eee; color: #dc2626;">🚒 ${station.name}</h4>
          <p style="margin: 5px 0 0 0; font-size: 13px; color: #6b7280;">Gaziantep İtfaiye İstasyonu</p>
        </div>
      `)
      .on('click', () => {
        // Fly to station location smoothly at zoom level 16
        map.value.flyTo([station.latitude, station.longitude], 16, {
          duration: 1.5,
          easeLinearity: 0.25
        })
      })
      .addTo(map.value)

    stationMarkers.value.push(marker)
  })
}

onMounted(async () => {
  map.value = L.map('map', { zoomControl: false }).setView([37.0662, 37.3833], 12)
  L.control.zoom({ position: 'bottomright' }).addTo(map.value)
  setTileLayer(mapStore.mapMode)

  simulationLayer.value = L.layerGroup().addTo(map.value)

  map.value.on('click', (e) => {
    if (mapStore.isSimulationMode) {
      const { lat, lng } = e.latlng
      mapStore.runSimulation(lat, lng)
    }
  })

  setTimeout(() => map.value?.invalidateSize(), 200)

  await mapStore.loadDashboardStats()
  await mapStore.loadDistricts()
  await mapStore.loadNeighborhoods()
  await mapStore.loadStations()
  await mapStore.loadFires()
  await mapStore.loadRescues()
  
  renderDistrictPolygons()
  renderNeighborhoodPolygons()
  renderStations()
  renderFires()
  renderRescues()
  renderHeatmap()
})

onUnmounted(() => {
  if (map.value) map.value.remove()
})

watch(() => mapStore.mapMode, setTileLayer)
watch(() => mapStore.mapProvider, () => setTileLayer(mapStore.mapMode))
watch(() => mapStore.trafficProvider, (newVal) => {
  setTrafficLayer(newVal)
  if (mapStore.isSimulationPanelOpen && mapStore.simulatedLocation) {
    mapStore.runSimulation(mapStore.simulatedLocation.lat, mapStore.simulatedLocation.lng)
  }
})
watch(() => mapStore.districts, renderDistrictPolygons, { deep: true })
watch(() => mapStore.layers.districts, renderDistrictPolygons)
watch(() => mapStore.neighborhoods, renderNeighborhoodPolygons, { deep: true })
watch(() => mapStore.layers.neighborhoods, renderNeighborhoodPolygons)
watch(() => mapStore.stations, renderStations, { deep: true })
watch(() => mapStore.layers.stations, renderStations)
watch(() => mapStore.stationSuggestions, renderSuggestionMarkers, { deep: true })
watch(() => mapStore.analysis, () => {
  updateNeighborhoodStyles()
  renderFires()
  renderRescues()
  renderHeatmap()
}, { deep: true })

watch(() => mapStore.fires, () => {
  renderFires()
  renderHeatmap()
}, { deep: true })

watch(() => mapStore.rescues, () => {
  renderRescues()
  renderHeatmap()
}, { deep: true })

watch(() => mapStore.dateFilter, () => {
  renderFires()
  renderRescues()
  renderHeatmap()
}, { deep: true })
watch(() => mapStore.selectedNeighborhoodDetails, (details) => {
  if (details && neighborhoodLayers.value[details.id]) {
    const layer = neighborhoodLayers.value[details.id]
    map.value.flyToBounds(layer.getBounds(), { padding: [50, 50], duration: 1.5 })
  }
})
watch(() => mapStore.simulatedLocation, (location) => {
  simulationLayer.value.clearLayers()
  if (location) {
    const pinIcon = L.divIcon({
      className: 'custom-div-icon',
      html: `<div style="background-color: #ef4444; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; border: 2px solid white; box-shadow: 0 0 10px rgba(239, 68, 68, 0.8);">📍</div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12]
    })
    L.marker([location.lat, location.lng], { icon: pinIcon })
      .bindPopup(`<b>🔥 Yeni Olay Simülasyonu</b><br/>En yakın istasyonlar aranıyor...`)
      .addTo(simulationLayer.value)
      .openPopup()
  }
})

let animationFrameId = null;

watch(() => mapStore.simulationResults, (results) => {
  if (results && results.stations && results.stations.length > 0 && mapStore.simulatedLocation) {
    const closestStation = results.stations[0];
    const startLat = closestStation.latitude;
    const startLng = closestStation.longitude;
    const endLat = mapStore.simulatedLocation.lat;
    const endLng = mapStore.simulatedLocation.lng;

    let fireStationVehicleCount = 0;
    if (results.recommendedVehicles) {
      results.recommendedVehicles.forEach(v => {
        const vStr = v.toLowerCase();
        if (vStr.includes('itfaiye') || vStr.includes('merdivenli') || vStr.includes('arazöz') || vStr.includes('tanker') || vStr.includes('kurtarma') || vStr.includes('kbrn')) {
          const match = v.match(/\d+/);
          if (match) {
            fireStationVehicleCount += parseInt(match[0], 10);
          } else {
            fireStationVehicleCount += 1;
          }
        }
      });
    }
    if (fireStationVehicleCount === 0) fireStationVehicleCount = 1;

    if (startLat && startLng && endLat && endLng) {
      if (animationFrameId) cancelAnimationFrame(animationFrameId);
      
      const truckIcon = L.divIcon({
        className: 'custom-div-icon',
        html: `<div style="font-size: 24px; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.5)); transform: scaleX(-1);">🚒</div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12]
      });

      // OSRM Public API ile gerçek yol rotasını çek
      fetch(`https://router.project-osrm.org/route/v1/driving/${startLng},${startLat};${endLng},${endLat}?overview=full&geometries=geojson`)
        .then(res => res.json())
        .then(data => {
          if (data.code !== 'Ok' || !data.routes || data.routes.length === 0) {
            runStraightLineAnimation();
            return;
          }

          const route = data.routes[0];
          
          // Trafik durumuna göre süreyi hesapla (Trafik açıksa %30 daha yavaş)
          const trafficMultiplier = mapStore.trafficProvider !== 'none' ? 1.3 : 1.0;
          closestStation.estimatedArrivalTimeMinutes = ((route.duration / 60) * trafficMultiplier).toFixed(1);

          const coords = route.geometry.coordinates.map(c => [c[1], c[0]]); // [lat, lng]
          
          // Gerçek rotayı haritaya çiz
          L.polyline(coords, { color: '#ef4444', weight: 4, dashArray: '5, 10', opacity: 0.7 }).addTo(simulationLayer.value);

          const trucks = [];
          for(let k = 0; k < fireStationVehicleCount; k++) {
             const marker = L.marker(coords[0], { icon: truckIcon }).addTo(simulationLayer.value);
             if(k === 0) marker.bindTooltip(`<b>${closestStation.stationName}</b> Yola Çıktı!`, { permanent: true, direction: 'top', opacity: 0.8 });
             trucks.push(marker);
          }

          // Rota segmentlerinin mesafelerini hesapla
          let totalDist = 0;
          const dists = [0];
          for (let i = 0; i < coords.length - 1; i++) {
            const d = L.latLng(coords[i]).distanceTo(L.latLng(coords[i+1]));
            totalDist += d;
            dists.push(totalDist);
          }

          const staggerDist = 200; // 200 meters delay between trucks
          const totalConvoyDist = totalDist + (trucks.length - 1) * staggerDist;
          
          let startTime = null;
          const duration = 4000 + (trucks.length - 1) * 800; // Animasyon araç sayısına göre uzar

          function animateMarker(timestamp) {
            if (!startTime) startTime = timestamp;
            const progress = Math.min((timestamp - startTime) / duration, 1);
            const easeProgress = 1 - Math.pow(1 - progress, 3); // easeOutCubic
            
            const targetDist = easeProgress * totalConvoyDist;
            
            trucks.forEach((marker, index) => {
              const delayDist = index * staggerDist;
              let tDist = targetDist - delayDist;
              if (tDist < 0) tDist = 0;
              
              let i = 0;
              while (i < dists.length - 1 && dists[i+1] < tDist) i++;
              
              if (tDist >= totalDist) {
                marker.setLatLng(coords[coords.length - 1]);
              } else if (i < coords.length - 1) {
                const segmentDist = dists[i+1] - dists[i];
                const segmentProgress = segmentDist === 0 ? 0 : (tDist - dists[i]) / segmentDist;
                const currentLat = coords[i][0] + (coords[i+1][0] - coords[i][0]) * segmentProgress;
                const currentLng = coords[i][1] + (coords[i+1][1] - coords[i][1]) * segmentProgress;
                marker.setLatLng([currentLat, currentLng]);
              }
            });

            if (progress < 1) {
              animationFrameId = requestAnimationFrame(animateMarker);
            } else {
              trucks[0].setTooltipContent(`<b>Olay Yerine Ulaşıldı</b>`);
              setTimeout(() => trucks[0].closeTooltip(), 3000);
            }
          }

          animationFrameId = requestAnimationFrame(animateMarker);
        })
        .catch(err => {
          console.error("OSM Routing API error:", err);
          runStraightLineAnimation();
        });

      function runStraightLineAnimation() {
        const trucks = [];
        for(let k = 0; k < fireStationVehicleCount; k++) {
           const marker = L.marker([startLat, startLng], { icon: truckIcon }).addTo(simulationLayer.value);
           if(k === 0) marker.bindTooltip(`<b>${closestStation.stationName}</b> Yola Çıktı! (Kuş Uçuşu)`, { permanent: true, direction: 'top', opacity: 0.8 });
           trucks.push(marker);
        }

        let startTime = null;
        const duration = 2000 + (trucks.length - 1) * 300;

        function animateMarker(timestamp) {
          if (!startTime) startTime = timestamp;
          const progress = Math.min((timestamp - startTime) / duration, 1);
          const easeProgress = 1 - Math.pow(1 - progress, 3);
          
          const totalConvoyProgress = easeProgress * (1 + (trucks.length - 1) * 0.15); 

          trucks.forEach((marker, index) => {
             const delayProgress = index * 0.15;
             let p = totalConvoyProgress - delayProgress;
             if (p < 0) p = 0;
             if (p > 1) p = 1;

             const currentLat = startLat + (endLat - startLat) * p;
             const currentLng = startLng + (endLng - startLng) * p;
             marker.setLatLng([currentLat, currentLng]);
          });

          if (progress < 1) {
            animationFrameId = requestAnimationFrame(animateMarker);
          } else {
            trucks[0].setTooltipContent(`<b>Olay Yerine Ulaşıldı</b>`);
            setTimeout(() => trucks[0].closeTooltip(), 3000);
          }
        }
        animationFrameId = requestAnimationFrame(animateMarker);
      }
    }
  }
}, { deep: true });

function setTileLayer(mode) {
  if (currentTileLayer.value) map.value.removeLayer(currentTileLayer.value)
  
  const provider = mapStore.mapProvider || 'osm'
  let url = tiles.standard_osm
  
  if (mode === 'dark') {
    url = tiles.dark
  } else if (mode === 'satellite') {
    url = provider === 'google' ? tiles.satellite_google
         : provider === 'yandex' ? tiles.satellite_yandex
         : tiles.satellite_esri
  } else {
    url = provider === 'google' ? tiles.standard_google
         : provider === 'yandex' ? tiles.standard_yandex
         : tiles.standard_osm
  }
  
  currentTileLayer.value = L.tileLayer(url, {
    maxZoom: 19,
    attribution: '© OpenStreetMap / Google / Yandex'
  }).addTo(map.value)
}

function setTrafficLayer(provider) {
  if (trafficLayer.value) {
    map.value.removeLayer(trafficLayer.value)
    trafficLayer.value = null
  }
  if (provider && provider !== 'none') {
    const url = TRAFFIC_TILES[provider] || TRAFFIC_TILES.google
    trafficLayer.value = L.tileLayer(url, {
      maxZoom: 19,
      opacity: 0.9,
      zIndex: 100, // Ensure traffic is above base tiles
      attribution: provider === 'google' ? '© Google Trafik' : '© Yandex Trafik'
    }).addTo(map.value)
  }
}
</script>

<style>
.leaflet-bottom.leaflet-right { bottom: 200px; right: 20px; }
.leaflet-bar { border: none !important; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1) !important; border-radius: 0.5rem !important; overflow: hidden; }
.leaflet-bar a { background-color: rgba(255,255,255,0.9) !important; color: #374151 !important; width: 36px !important; height: 36px !important; line-height: 36px !important; }
.leaflet-bar a:hover { background-color: #f3f4f6 !important; }
</style>
