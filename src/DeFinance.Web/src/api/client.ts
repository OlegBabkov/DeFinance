import axios from 'axios'
import { TOKEN_KEY } from './auth'

const client = axios.create({ baseURL: '/api' })

client.interceptors.request.use(config => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401 && localStorage.getItem(TOKEN_KEY)) {
      localStorage.removeItem(TOKEN_KEY)
      window.location.href = '/'
    }
    return Promise.reject(err)
  }
)

export default client
