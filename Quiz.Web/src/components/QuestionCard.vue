<script lang="ts">
import type { extendedQuestionResponseDto } from '@stores/responseDtos/extendedQuestionResponseDto'
import type { PropType } from 'vue'

export default {
    props: {
        question: {
            type: Object as PropType<extendedQuestionResponseDto>,
            required: true
        }
    }
}

</script>

<template>
    <div class="my-5">
        <h3>{{ question.questionText }}</h3>
        <template v-for="(answer, i) in question.answers">
            <div class="card my-2" :class="question.indexOfCorrectAnswer === i ? 'correct' : ''">
                <div class="card-body">
                    <p class="card-text">{{ answer }}</p>
                </div>
            </div>
        </template>
    </div>
</template>

<style>
.correct {
    border: 1px solid green;
}
</style>