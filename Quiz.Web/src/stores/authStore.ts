import { defineStore } from 'pinia'
import https, { addToken, removeToken } from '@utils/https'
import { AxiosError } from 'axios'
import { showProblemDetail } from '@utils/showProblemDetail'
import type { loginRequestDto } from '@stores/requestDtos/loginRequestDto'
import type { loginResponseDto } from '@stores/responseDtos/loginResponseDto'
import type { userRequestDto } from '@stores/requestDtos/userRequestDto'
import { useUser } from '@stores/userStore'
import { router } from '@router/index'

export const useAuth = defineStore('auth-store',{
    state(){
        return{
            user: null as loginResponseDto | null
        }
    },
    actions:{
         async register(userRequestDto: userRequestDto) {
            try {
                await https.post('/users/register', userRequestDto)
                return 'ok'
            }
            catch (error) {
                return showProblemDetail(error as AxiosError)
            }
        },
        async login(loginRequestDto: loginRequestDto) {
            try {
                const response = await https.post('/users/login', loginRequestDto)
                this.user = response.data as loginResponseDto
                localStorage.setItem('user', JSON.stringify(response.data))
                addToken(this.user.authToken)
                return 'ok'
            }
            catch (error) {
                return showProblemDetail(error as AxiosError)
            }
        },
        async logout() {
            try {
                await https.post('/users/logout', this.user?.refreshToken)
            }
            catch (error) {
                console.error('Error at logout:')
                console.error(showProblemDetail(error as AxiosError))
            }
            finally {
                this.user = null
                localStorage.removeItem('user')
                removeToken()
                useUser().clearUser()
                router.push({name: 'login'})
            }
        },
        async refreshToken() {
            try {
                const response = await https.post('/users/refresh', this.user?.refreshToken)
                this.user = response.data as loginResponseDto
                localStorage.setItem('user', JSON.stringify(response.data))
                addToken(this.user.authToken)
                return this.user.authToken
            }
            catch (error) {
                console.error('Error at refreshToken:')
                console.error(showProblemDetail(error as AxiosError))
                throw error
            }
        },
        initialize() {
            const storedUser = localStorage.getItem('user')
            if (storedUser) {
                this.user = JSON.parse(storedUser)
                addToken(this.user!.authToken)
            }
        }
    }
})