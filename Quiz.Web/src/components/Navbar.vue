<script lang="ts">

import { RouterLink } from 'vue-router'
import { useAuth } from '@stores/authStore'

export default {
    components: {
        RouterLink
    },
    data() {
        return {
            useAuth: useAuth()
        }
    },
    methods: {
        async logout() {
            await this.useAuth.logout()
        }
    }
}
</script>

<template>
    <nav class="navbar navbar-expand-lg bg-body-tertiary">
        <div class="container-fluid">
            <RouterLink class="navbar-brand" :to="{ name: 'home' }">Home</RouterLink>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
                aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav">
                    <template v-if="!useAuth.user">
                        <li class="nav-item">
                            <RouterLink :to="{ name: 'login' }" class="nav-link">Login</RouterLink>
                        </li>
                        <li class="nav-item">
                            <RouterLink :to="{ name: 'register' }" class="nav-link">Register</RouterLink>
                        </li>
                    </template>

                    <template v-else>
                        <li class="nav-item">
                            <RouterLink :to="{ name: 'myAccount' }" class="nav-link">My Account</RouterLink>
                        </li>
                        <li class="nav-item">
                            <button class="nav-link text-danger" @click="logout">Logout</button>
                        </li>
                    </template>
                </ul>
            </div>
        </div>
    </nav>
</template>

<style></style>