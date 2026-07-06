import { defineStore } from 'pinia'
import https from '@utils/https'
import { AxiosError } from 'axios'
import { showProblemDetail } from '@utils/showProblemDetail'
import type { quizRequestDto } from '@stores/requestDtos/quizRequestDto'
import { useAuth } from '@stores/authStore'
import type { pagedUserResponseDto } from '@stores/responseDtos/pagedUserResponseDto'

export const useUser = defineStore("user-store", {
    state() {
        return {
            userData: null as pagedUserResponseDto | null
        }
    },
    actions: {
        async getUser(page: number) {
            try {
                const response = await https.get(`/users/${useAuth().user?.userId}?page=${page}`)
                this.userData = response.data
            }
            catch (error) {
                console.error('Error at getUser:')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async publishQuiz(quizId: number) {
            try {
                await https.get(`/quizzes/${quizId}/publish`)
                return 'ok'
            }
            catch (error) {
                console.error('Error at publishQuiz:')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async startQuiz(quizId: number) {
            try {
                await https.get(`quizzes/${quizId}/start`)
                return 'ok'
            }
            catch (error) {
                console.error('Error at startQuiz:')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async nextQuestion(quizId: number) {
            try {
                await https.get(`quizzes/${quizId}/nextQuestion`)
                return 'ok'
            }
            catch (error) {
                console.error('Error at nextQuestion')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async closeQuestion(quizId: number) {
            try {
                await https.get(`quizzes/${quizId}/closeQuestion`)
                return 'ok'
            }
            catch (error) {
                console.error('Error at closeQuestion')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async createQuiz(quizRequestDto: quizRequestDto) {
            try {
                await https.post('quizzes/create', quizRequestDto)
                return "ok"
            }
            catch (error) {
                return showProblemDetail(error as AxiosError)
            }
        },
        clearUser(){
            this.userData = null
        },
        async getMyQuiz(quizId: number){
            try{
                const response = await https.get(`quizzes/my-quizzes/${quizId}`)
                return response.data
            }
            catch(error){
                console.error('Error at getMyQuiz')
                console.error(showProblemDetail(error as AxiosError))
            }
        }
    }
})