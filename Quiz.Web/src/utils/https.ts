import axios from 'axios'
import type { AxiosHeaderValue } from 'axios'
import { useAuth } from '@stores/authStore'

const https = axios.create({
    baseURL: '/api/',
    headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    }
})

https.interceptors.response.use(response => response,async error =>{
    const authStore = useAuth()
    const originalRequest = error.config

    if(error.response?.status === 401 && !originalRequest._retry && !originalRequest.url?.includes('/users/refresh')){
        originalRequest._retry = true
        try{
            const newAccessToken = await authStore.refreshToken()

            originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
            return https(originalRequest)
        }
        catch{
            await authStore.logout()
        }
    }
    return Promise.reject(error)
})


export function addToken(token: AxiosHeaderValue) {
    https.defaults.headers.common['Authorization'] = `Bearer ${token}`
}

export function removeToken() {
    delete https.defaults.headers.common['Authorization'];
}

export default https