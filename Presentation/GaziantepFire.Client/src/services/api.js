import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5076/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 5000
})

export default {
  async getDashboardStats() {
    try {
      const response = await apiClient.get('/dashboard/stats')
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable, using fallback mock stats:', error)
      return null
    }
  },

  async getNeighborhoods() {
    try {
      const response = await apiClient.get('/neighborhoods')
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable for neighborhoods:', error)
      return []
    }
  },

  async getNeighborhoodDetails(id) {
    try {
      const response = await apiClient.get(`/neighborhoods/${id}/details`)
      return response.data
    } catch (error) {
      console.warn(`Backend API unreachable for neighborhood ${id} details:`, error)
      return null
    }
  },

  async getStations() {
    try {
      const response = await apiClient.get('/stations')
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable for stations:', error)
      return []
    }
  },

  async getStationSuggestions(count) {
    try {
      const response = await apiClient.get(`/stations/suggestions?count=${count}`)
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable for station suggestions:', error)
      return null
    }
  },

  async calculateIncident(lat, lng) {
    try {
      const response = await apiClient.post('/simulation/calculate-incident', {
        latitude: lat,
        longitude: lng
      })
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable for incident calculation:', error)
      return null
    }
  },

  async getFires(startDate, endDate) {
    try {
      let url = '/incidents/fires'
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const queryString = params.toString()
      if (queryString) url += `?${queryString}`
        
      const response = await apiClient.get(url)
      return response.data
    } catch (error) {
      console.warn('Backend API unreachable for fires:', error)
      return []
    }
  }
}
