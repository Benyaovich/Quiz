import { defineStore } from 'pinia'
import https from '@utils/https'
import type { AxiosError } from 'axios'
import { showProblemDetail } from '@utils/showProblemDetail'
import type { pagedQuizzesResponseDto } from '@stores/responseDtos/pagedQuizzesResponseDto'
import type { joinRequestDto } from '@stores/requestDtos/joinRequestDto'
import type { joinResponseDto } from '@stores/responseDtos/joinResponseDto'
import type { questionResponseDto } from '@stores/responseDtos/questionResponseDto'
import type { quizAccessRequestDto } from '@stores/requestDtos/quizAccessRequestDto'
import type { chatRequestDto } from './requestDtos/chatRequestDto'


export const useQuizzes = defineStore('quizzes-store', {
    state() {
        return {
            publishedQuizzes: null as pagedQuizzesResponseDto | null,
            joinedQuiz: null as joinResponseDto | null,
            quizSession: null as quizAccessRequestDto | null,
            actualQuestion: null as questionResponseDto | null,
            joinedQuizId: null as number | null
        }
    },
    actions: {
        async getPublishedQuizzes(page: number) {
            try{
                const response = await https.get(`/quizzes?page=${page}`)
                this.publishedQuizzes = response.data
            }
            catch(error){
                console.error('Error at getPublishedQuizzes')
                console.error(showProblemDetail(error as AxiosError))
            }

        },
        async joinQuiz(quizId: number, joinRequestDto: joinRequestDto) {
            try {
                const response = await https.post(`quizzes/${quizId}/join`, joinRequestDto)
                this.joinedQuiz = response.data
                if(this.joinedQuiz) this.joinedQuizId = this.joinedQuiz.id
                localStorage.setItem('joinedQuizId', JSON.stringify(this.joinedQuizId))
                this.quizSession = joinRequestDto.quizAccess
                localStorage.setItem('quizSession', JSON.stringify(this.quizSession))
                return 'ok'
            }
            catch (error) {
                return showProblemDetail(error as AxiosError)
            }
        },
        async initialize() {
            const quizSession = localStorage.getItem('quizSession')
            if (quizSession) this.quizSession = JSON.parse(quizSession)
            const joinedQuizId = localStorage.getItem('joinedQuizId')
            if (joinedQuizId) this.joinedQuizId = JSON.parse(joinedQuizId)
        },
        async getActiveQuestion() {
            try {
                const response = await https.post(`quizzes/${this.joinedQuiz?.id}/activeQuestion`, this.quizSession)
                this.actualQuestion = response.data
                return 'ok'
            }
            catch (error) {
                const errorMsg = showProblemDetail(error as AxiosError)
                if(errorMsg === 'The quiz is not started yet'){
                    return 'ok'
                }
                this.joinedQuiz = null
                this.quizSession = null
                this.actualQuestion = null
                this.joinedQuizId = null
                localStorage.removeItem('quizSession')
                localStorage.removeItem('joinedQuizId')
                console.error('Error at getActiveQuestion')
                return showProblemDetail(error as AxiosError)
            }
        },
        async showAnswer() {
            try {
                const response = await https.post(`quizzes/${this.joinedQuiz?.id}/activeQuestion/answer`, this.quizSession)
                return response.data
            }
            catch (error) {
                console.error('Error at showAnswer')
                console.error(showProblemDetail(error as AxiosError))
                return -1
            }
        },
        async syncJoinedQuiz(){
            try{
                const response = await https.post(`quizzes/${this.joinedQuizId}`,this.quizSession)
                this.joinedQuiz = response.data
            }
            catch(error){
                console.error('Error at syncJoinedQuiz')
                console.error(showProblemDetail(error as AxiosError))
            }
        },
        async sendMessage(message: string){
            try{
                const chatRequestDto = {
                    quizAccess: this.quizSession,
                    message: message
                } as chatRequestDto
                await https.post(`quizzes/${this.joinedQuizId}/chat`, chatRequestDto)
            }
            catch(error){
                console.error('Error at sendMessage')
                console.error(showProblemDetail(error as AxiosError))
            }
        }
    }
})