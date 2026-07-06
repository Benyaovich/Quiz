<script lang="ts">
import type { extendedQuizResponseDto } from '@stores/responseDtos/extendedQuizResponseDto'
import { RouterLink } from 'vue-router'

export default {
    props: {
        quizzes: Array as () => extendedQuizResponseDto[]
    },
    components: {
        RouterLink
    }
}

</script>

<template>

    <table class="table">
        <thead>
            <tr>
                <th>Pin</th>
                <th>Title</th>
                <th>Questions</th>
                <th>Participants</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="quiz in quizzes">
                <td>{{ quiz.isPublished ? quiz.pin : "Not Published yet" }}</td>
                <td>{{ quiz.title }}</td>
                <td>{{ quiz.questions.length }} / 20</td>
                <td>{{ quiz.participants.length }}</td>
                <td>{{ quiz.isStarted ? "Started" : "Not Started" }}</td>
                <td>
                    <RouterLink :to="{ name: 'myQuiz', params: { 'quizId': quiz.id } }">Details</RouterLink>
                </td>
            </tr>
        </tbody>
    </table>


</template>