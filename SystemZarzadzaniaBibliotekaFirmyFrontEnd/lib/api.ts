import axios from 'axios'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5205'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Interceptor żądań - dodaje token autoryzacyjny do każdego zapytania
apiClient.interceptors.request.use(
  (config) => {
    // Pobierz token z localStorage, jeśli istnieje
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null
    if (token) {
      // Dodaj nagłówek Authorization: Bearer <token>
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Interceptor odpowiedzi - obsługuje globalne błędy (np. wygaśnięcie sesji)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Obsługa błędu 401 Unauthorized - wyczyść token i przekieruj do logowania
      if (typeof window !== 'undefined') {
        localStorage.removeItem('token')
        // Przekieruj tylko jeśli użytkownik nie jest już na stronie logowania
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
      }
    }
    return Promise.reject(error)
  }
)

export default apiClient

