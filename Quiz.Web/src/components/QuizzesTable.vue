<script lang="ts">
import type { quizResponseDto } from '@stores/responseDtos/quizResponseDto'
import JoinModal from '@components/JoinModal.vue'
import { useQuizzes } from '@stores/quizzesStore'

export default {
    components: {
        JoinModal
    },
    data() {
        return {
            joinModalRef: null as InstanceType<typeof JoinModal> | null,
            useQuizzes: useQuizzes()
        }
    },
    props: {
        quizzes: Array as () => quizResponseDto[]
    },
    methods: {
        openJoinModal(quiz: quizResponseDto) {
            const modal = this.$refs.joinModalRef as InstanceType<typeof JoinModal>
            modal.showModal(quiz)
        },
        continueQuiz() {
            this.$router.push({ name: 'activeQuestion', params: { quizId: this.useQuizzes.joinedQuizId } })
        }
    }
};
</script>

<template>
    <table class="table">
        <thead class="bg-secondary text-white">
            <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Author</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="quiz in quizzes" :key="quiz.id">
                <td>{{ quiz.title }}</td>
                <td>{{ quiz.isStarted ? 'Started' : 'Not Started' }}</td>
                <td>{{ quiz.author }}</td>
                <td>
                    <button class="btn btn-primary" v-if="useQuizzes.joinedQuizId == quiz.id"
                        @click="continueQuiz">Continue</button>
                    <button class="btn btn-success" v-else :disabled="quiz.isStarted" @click="openJoinModal(quiz)">
                        Join
                    </button>
                </td>
            </tr>
        </tbody>
    </table>

    <JoinModal ref="joinModalRef" />
</template>