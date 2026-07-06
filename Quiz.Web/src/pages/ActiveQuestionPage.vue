<script lang="ts">

import BaseLayout from '@layouts/BaseLayout.vue'
import type { HubConnection } from '@microsoft/signalr'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQuizzes } from '@stores/quizzesStore'

export default {
    data() {
        return {
            useQuizzes: useQuizzes(),
            selectedAnswer: {
                answerIndex: null as number | null,
                questionIndex: null as number | null
            },
            correctAnswerIndex: null as number | null,
            answerRevealed: false,
            signalRConnection: null as HubConnection | null,
            message: ''
        }
    },
    components: {
        BaseLayout
    },
    computed: {
        routeQuizId(): number | null {
            const id = Number(this.$route.params.quizId)
            return Number.isNaN(id) ? null : id
        }
    },
    async unmounted() {
        if (this.signalRConnection) {
            await this.signalRConnection.stop()
        }
    },
    async mounted() {
        this.signalRConnection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_APP_SIGNALR_BASEURL}/QuizHub`)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build()

        this.signalRConnection.on("QuizStarted", async () => {
            await this.useQuizzes.syncJoinedQuiz()
            await this.useQuizzes.getActiveQuestion()
        })

        this.signalRConnection.on("QuestionClosed", async () => {
            await this.revealAnswer()
        });

        this.signalRConnection.on("NextQuestion", async () => {
            localStorage.removeItem('selectedAnswer')
            this.selectedAnswer.answerIndex = null
            this.selectedAnswer.questionIndex = null
            this.answerRevealed = false
            const response = await this.useQuizzes.getActiveQuestion()
            if (response !== 'ok') {
                this.$router.push({ name: 'home' })
                return;
            }
        });

        this.signalRConnection.on("NewChatMessage", async () => {
            await this.useQuizzes.syncJoinedQuiz()
        })

        await this.signalRConnection.start()
        await this.signalRConnection.invoke("JoinQuiz", this.useQuizzes.joinedQuizId?.toString())

        await this.useQuizzes.syncJoinedQuiz()
        const response = await this.useQuizzes.getActiveQuestion()
        if (response !== 'ok') {
            this.$router.push({ name: 'home' })
            return
        }

        if (!this.useQuizzes.joinedQuiz?.isStarted) {
            localStorage.removeItem('selectedAnswer')
            this.selectedAnswer.answerIndex = null
        }

        if (this.useQuizzes.actualQuestion?.isClosed) {
            const json = localStorage.getItem('selectedAnswer')
            if (json) {
                this.selectedAnswer = JSON.parse(json)
            }
            this.revealAnswer()
        }

        if (this.useQuizzes.actualQuestion) {
            const json = localStorage.getItem('selectedAnswer')
            if (!json) {
                return
            }
            this.selectedAnswer = JSON.parse(json)
            if (this.useQuizzes.actualQuestion.id != this.selectedAnswer.questionIndex) {
                localStorage.removeItem('selectedAnswer')
                this.selectedAnswer.answerIndex = null
            }
        }
    },
    methods: {
        selectAnswer(index: number) {
            if (this.answerRevealed) return
            this.selectedAnswer.answerIndex = index
            this.selectedAnswer.questionIndex = this.useQuizzes.actualQuestion!.id
            localStorage.setItem('selectedAnswer', JSON.stringify(this.selectedAnswer))
        },
        async revealAnswer() {
            const correctAnswerIndex = await this.useQuizzes.showAnswer()

            if (correctAnswerIndex == -1) {
                return
            }

            this.correctAnswerIndex = correctAnswerIndex
            this.answerRevealed = true
        },
        getAnswerClass(index: number) {
            if (!this.answerRevealed) {
                return {
                    selected: this.selectedAnswer?.answerIndex === index
                }
            }

            return {
                correct: this.correctAnswerIndex === index,
                wrong: this.selectedAnswer.answerIndex === index && this.correctAnswerIndex !== index
            }
        },
        async sendMessage(){
            await this.useQuizzes.sendMessage(this.message)
        }
    }
}
</script>
<template>
    <BaseLayout>
        <div class="row">
            <div class="col-12 col-lg-8">
                <h1 v-if="!useQuizzes.joinedQuiz?.isStarted">
                    Waiting for the host to start the quiz.
                </h1>

                <div class="my-5" v-else>
                    <h3>{{ useQuizzes.actualQuestion?.questionText }}</h3>

                    <template v-for="(answer, index) in useQuizzes.actualQuestion?.answers" :key="index">
                        <div class="card my-2 answer-card" :class="getAnswerClass(index)" @click="selectAnswer(index)">
                            <div class="card-body">
                                <p class="card-text">{{ answer }}</p>
                            </div>
                        </div>
                    </template>
                </div>
            </div>

            <div class="col-12 col-lg-4">
                <div class="card chat-panel">
                    <div class="card-header">
                        Chat
                    </div>

                    <div class="card-body chat-messages">
                        <div v-for="message in useQuizzes.joinedQuiz?.chat" class="mb-2">
                            <span>{{ message }}</span>
                        </div>
                    </div>

                    <div class="card-footer">
                        <form @submit.prevent="sendMessage">
                            <div class="input-group">
                                <input v-model="message" type="text" class="form-control"
                                    placeholder="Write a message..." />

                                <button class="btn btn-primary" type="submit">
                                    Send
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </BaseLayout>
</template>

<style>
.answer-card {
    cursor: pointer;
    transition: 0.2s ease;
}

.answer-card:hover {
    transform: scale(1.02);
}

.selected {
    transform: scale(1.05);
    border: 2px solid #0d6efd;
}

.correct {
    border: 2px solid #198754 !important;
    background-color: #eaf7ee;
}

.wrong {
    border: 2px solid #dc3545 !important;
    background-color: #fdeeee;
}

.chat-panel {
  height: 70vh;
}

.chat-messages {
  overflow-y: auto;
}
</style>
