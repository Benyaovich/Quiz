<script lang="ts">

import type { loginRequestDto } from '@stores/requestDtos/loginRequestDto'
import { RouterLink } from 'vue-router'
import { useAuth } from '@stores/authStore'

export default {
    components: {
        RouterLink
    },
    data() {
        return {
            loginRequestDto: {
                email: '',
                password: ''
            } as loginRequestDto,
            useAuth: useAuth(),
            problem: false,
            problemDetail: ''
        }
    },
    methods: {
        async login() {
            const response = await this.useAuth.login(this.loginRequestDto)
            if (response === "ok") {
                this.$router.push('/myAccount')
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
    <form class="w-50 bg-secondary p-3 rounded m-auto" @submit.prevent="login">
        <h5 v-if="problem" class="text-danger">{{ problemDetail }}</h5>
        <div class="my-3">
            <label for="email" class="form-label">Email:</label>
            <input v-model="loginRequestDto.email" type="email" id="email" class="form-control" required>
        </div>
        <div class="my-3">
            <label for="password" class="form-label">Password:</label>
            <input v-model="loginRequestDto.password" type="password" id="password" class="form-control" required>
        </div>
        <input type="submit" value="Login" class="form-control bg-primary">
        <RouterLink :to="{ name: 'register' }">Register</RouterLink>
    </form>

</template>