<script lang="ts">
import { useQuizzes } from '@stores/quizzesStore'
import { useUser } from '@stores/userStore'
import type { quizRequestDto } from '@stores/requestDtos/quizRequestDto'
import BaseLayout from '@layouts/BaseLayout.vue'

export default {
    components: {
        BaseLayout
    },
    data() {
        return {
            maxQuestions: 20,
            quizRequestDto: {
                title: '',
                questions: [
                    {
                        questionText: '',
                        answers: ['', ''],
                        indexOfCorrectAnswer: 0
                    }
                ]
            } as quizRequestDto,
            useQuizzes: useQuizzes(),
            problem: false,
            problemDetail: ''
        }
    },
    computed: {
        questionCount() {
            return this.quizRequestDto.questions.length
        }
    },
    methods: {
        addQuestion() {
            if (this.questionCount < this.maxQuestions) {
                this.quizRequestDto.questions.push({
                    questionText: '',
                    answers: ['', ''],
                    indexOfCorrectAnswer: 0
                })
            }
        },
        removeQuestion(index: number) {
            if (this.questionCount > 1) {
                this.quizRequestDto.questions.splice(index, 1)
            }
        },
        expandAnswers(question: any) {
            if (question.answers.length === 2) {
                question.answers.push('', '')
            }
        },
        async submitForm() {
            const response = await useUser().createQuiz(this.quizRequestDto)
            if (response === "ok") {
                this.$router.push({ name: 'myAccount' })
            }
            else {
                this.problem = true,
                this.problemDetail = response
            }
        }
    }
}
</script>

<template>
    <BaseLayout>
        <div class="container mt-4">
            <form @submit.prevent="submitForm" class="card p-4 shadow-sm">
                <h1 v-if="problem" class="text-danger">{{ problemDetail }}</h1>
                <h2 class="mb-4">New Quiz</h2>

                <div class="mb-3">
                    <label class="form-label">Title</label>
                    <input type="text" class="form-control" v-model="quizRequestDto.title" required minlength="3"
                        maxlength="255" />
                </div>

                <div class="mb-3 text-muted">
                    Questions: {{ questionCount }} / {{ maxQuestions }}
                </div>

                <div v-for="(question, qIndex) in quizRequestDto.questions" :key="qIndex" class="card mb-3 p-3 border">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h5 class="mb-0">Question {{ qIndex + 1 }}</h5>

                        <button v-if="questionCount > 1" type="button" class="btn btn-sm btn-danger"
                            @click="removeQuestion(qIndex)">
                            Delete
                        </button>
                    </div>
                    <div class="mb-3">
                        <input type="text" class="form-control" v-model="question.questionText" required minlength="3"
                            maxlength="255" />
                    </div>

                    <div v-for="(answer, aIndex) in question.answers" :key="aIndex" class="input-group mb-2">
                        <div class="input-group-text">
                            <input type="radio" :name="'correct-' + qIndex" :value="aIndex"
                                v-model="question.indexOfCorrectAnswer" />
                        </div>

                        <input type="text" class="form-control" v-model="question.answers[aIndex]" placeholder="Answer"
                            required minlength="3" maxlength="255" />
                    </div>

                    <button v-if="question.answers.length === 2" type="button"
                        class="btn btn-outline-secondary btn-sm mt-2" @click="expandAnswers(question)">
                        + add more answers (max 4)
                    </button>
                </div>

                <button v-if="questionCount < maxQuestions" type="button" class="btn btn-primary mb-3"
                    @click="addQuestion">
                    + add question
                </button>

                <input type="submit" class="btn btn-success" value="Save" />

            </form>
        </div>
    </BaseLayout>
</template>