<script lang="ts">

import BaseLayout from '@layouts/BaseLayout.vue'
import { useUser } from '@stores/userStore'
import { useQuizzes } from '@stores/quizzesStore'
import QuestionCard from '@components/QuestionCard.vue'
import type { extendedQuestionResponseDto } from '@stores/responseDtos/extendedQuestionResponseDto'
import type { extendedQuizResponseDto } from '@stores/responseDtos/extendedQuizResponseDto'

export default {
    components: {
        BaseLayout,
        QuestionCard
    },
    data() {
        return {
            useUser: useUser(),
            quizId: Number(this.$route.params.quizId),
            useQuizzes: useQuizzes(),
            quiz: null as extendedQuizResponseDto | null
        }
    },
    computed: {
        isLastQuestion(): boolean {
            if (!this.quiz) return false
            return this.quiz.indexOfActiveQuestion === this.quiz.questions.length - 1
        },
        activeQuestion(): extendedQuestionResponseDto | null {
            if (!this.quiz) return null

            return this.quiz.questions[this.quiz.indexOfActiveQuestion] ?? null
        }
    },
    async mounted() {
        this.quiz = await this.useUser.getMyQuiz(this.quizId)
        if (!this.quiz?.isStarted) {
            this.$router.push({ name: 'myQuiz', params: { quizId: this.quizId } })
        }
    },
    methods: {
        async nextQuestion() {
            if (this.isLastQuestion) {
                this.$router.push({ name: 'myAccount' })
            }
            const response = await this.useUser.nextQuestion(this.quizId)
            if (response !== 'ok') {
                this.$router.push({ name: 'myQuiz', params: { quizId: this.quizId } })
                return
            }
            this.quiz = await this.useUser.getMyQuiz(this.quizId)
        },
        async closeQuestion() {
            const response = await this.useUser.closeQuestion(this.quizId)
            if (response !== 'ok') {
                this.$router.push({ name: 'myQuiz', params: { quizId: this.quizId } })
                return
            }
            this.quiz = await this.useUser.getMyQuiz(this.quizId)
        }
    }
}

</script>

<template>
    <BaseLayout>
        <template v-if="activeQuestion">
            <QuestionCard :question="activeQuestion" />

            <div class="float-end">
                <button v-if="!activeQuestion.isClosed" class="btn bg-secondary text-white" @click="closeQuestion">
                    Close Question
                </button>

                <button v-else class="btn bg-secondary text-white" @click="nextQuestion">
                    {{ isLastQuestion ? 'End Quiz' : 'Next Question' }}
                </button>
            </div>
        </template>
    </BaseLayout>
</template>