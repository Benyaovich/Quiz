<script lang="ts">
import { RouterLink } from 'vue-router'
import type { userRequestDto } from '@stores/requestDtos/userRequestDto'
import { useAuth } from '@stores/authStore'

export default {
    components: {
        RouterLink
    },
    data() {
        return {
            userRequestDto: {
                userName: '',
                email: '',
                password: ''
            } as userRequestDto,
            useAuth: useAuth(),
            problem: false,
            problemDetail: ''
        }
    },
    methods: {
        async register() {
            const response = await this.useAuth.register(this.userRequestDto)
            if (response === "ok") {
                this.$router.push('login')
            }
            else {
                this.problem = true
                this.problemDetail = response
            }
        }
    }
}
</script>

<template>
    <form class="w-50 bg-secondary p-3 rounded m-auto" @submit.prevent="register">
        <h5 v-if="problem" class="text-danger">{{ problemDetail }}</h5>
        <div class="my-3">
            <label for="username" class="form-label">Username:</label>
            <input v-model="userRequestDto.userName" type="text" id="username" class="form-control" required
                minlength="3">
        </div>

        <div class="my-3">
            <label for="email" class="form-label">Email:</label>
            <input v-model="userRequestDto.email" type="email" id="email" class="form-control" required>
        </div>

        <div class="my-3">
            <label for="password" class="form-label">Password:</label>
            <input v-model="userRequestDto.password" type="password" id="password" class="form-control" required
                pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$">
        </div>

        <input type="submit" value="Register" class="form-control bg-primary">

        <RouterLink :to="{ name: 'login' }">Login</RouterLink>
    </form>
</template>