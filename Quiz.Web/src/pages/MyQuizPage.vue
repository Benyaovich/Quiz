<script lang="ts">

import BaseLayout from '@layouts/BaseLayout.vue'
import { useUser } from '@stores/userStore'
import { useQuizzes } from '@stores/quizzesStore'
import type { extendedQuizResponseDto } from '@stores/responseDtos/extendedQuizResponseDto'
import QuestionCard from '@components/QuestionCard.vue'
import { RouterLink } from 'vue-router'

export default {
    components: {
        BaseLayout,
        QuestionCard,
        RouterLink
    },
    data() {
        return {
            useUser: useUser(),
            useQuizzes: useQuizzes(),
            quiz: null as extendedQuizResponseDto | null,
            quizId: null as number | null
        }
    },
    async mounted() {
        this.quizId = Number(this.$route.params.quizId)
        if (!Number.isNaN(this.quizId)) {
            this.quiz = await this.useUser.getMyQuiz(this.quizId)
            if (this.quiz) return
        }
        this.$router.push('/myAccount')
    },
    methods: {
        async publish() {
            if (!this.quizId) return
            const response = await this.useUser.publishQuiz(this.quizId)
            if (response !== 'ok') {
                this.$router.push({ name: 'myAccount' })
                return
            }
            this.quiz = await this.useUser.getMyQuiz(this.quizId)
        },
        async start() {
            if (!this.quizId) return
            const response = await this.useUser.startQuiz(this.quizId)
            if (response !== 'ok') {
                this.$router.push({ name: 'myAccount' })
                return
            }
            this.quiz = await this.useUser.getMyQuiz(this.quizId)
            if (this.quiz && this.quiz.isStarted) {
                this.$router.push({
                    name: 'userActiveQuestion',
                    params: {
                        quizId: this.quiz?.id
                    }
                })
            }
        }
    }
}

</script>

<template>
    <BaseLayout>
        <template v-if="quiz">
            <h1>{{ quiz.title }} {{ quiz.pin ? "-" : " " }} {{ quiz.pin }}</h1>
            <button class="btn bg-primary text-white" v-if="!quiz.isPublished" @click="publish">Publish</button>
            <button class="btn bg-primary text-white" v-if="quiz.isPublished && !quiz.isStarted"
                @click="start">Start</button>
            <RouterLink :to="{ name: 'userActiveQuestion', params: { quizId: quiz.id } }"
                class="btn bg-primary text-white" v-if="quiz.isPublished && quiz.isStarted">Continue</RouterLink>
            <template v-for="question in quiz.questions">
                <QuestionCard :question="question" />
            </template>
        </template>

    </BaseLayout>
</template>