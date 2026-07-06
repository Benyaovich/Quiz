<script lang="ts">

import { useQuizzes } from '@stores/quizzesStore'
import type { joinRequestDto } from '@stores/requestDtos/joinRequestDto'
import type { quizResponseDto } from '@stores/responseDtos/quizResponseDto'
// @ts-ignore
import Modal from 'bootstrap/js/dist/modal'

export default {
    data() {
        return {
            quiz: null as quizResponseDto | null,
            joinRequestDto: {
                quizAccess: {
                    userName: '',
                    pin: ''
                }
            } as joinRequestDto,
            modal: null as any,
            problem: false,
            problemDetail: '',
            useQuizzes: useQuizzes()
        }
    },
    methods: {
        showModal(quiz: quizResponseDto) {
            this.quiz = quiz

            if (!this.modal) {
                const modalEl = document.getElementById('joinModal');
                if (modalEl) this.modal = new Modal(modalEl);
            }

            this.modal?.show();
        },
        async join() {
            const response = await this.useQuizzes.joinQuiz(this.quiz!.id, this.joinRequestDto)
            if (response === "ok") {
                this.modal.hide()
                this.$router.push({ name: 'activeQuestion', params: { quizId: this.quiz!.id } })
            }
            else {
                this.problem = true
                this.problemDetail = response
            }
        }
    }
};
</script>

<template>
    <div class="modal fade" id="joinModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">

                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">{{ quiz == null ? "" : quiz.title }}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <h5 v-if="problem" class="text-danger">{{ problemDetail }}</h5>
                    <form class="form" @submit.prevent="join" :actions="false">
                        <div class="mb-3">
                            <label for="userName" class="form-label">Username</label>
                            <input type="text" class="form-control" id="userName" v-model="joinRequestDto.quizAccess.userName"
                                required minlength="3">
                        </div>
                        <div class="mb-3">
                            <label for="pin" class="form-label">PIN</label>
                            <input type="text" class="form-control" id="pin" v-model="joinRequestDto.quizAccess.pin" required
                                pattern="[0-9]{6}">
                        </div>
                        <button type="submit" class="btn btn-success">Join</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>