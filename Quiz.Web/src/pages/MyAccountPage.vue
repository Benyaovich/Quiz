<script lang="ts">

import BaseLayout from '@layouts/BaseLayout.vue'
import MyQuizzesTable from '@components/MyQuizzesTable.vue'
import { useUser } from '@stores/userStore'
import { RouterLink } from 'vue-router'
import Pagination from '@components/Pagination.vue'

export default {
    components: {
        BaseLayout,
        MyQuizzesTable,
        RouterLink,
        Pagination
    },
    data() {
        return {
            useUser: useUser(),
            page: 1
        }
    },
    async mounted() {
        const pageFromQuery = Number(this.$route.query.page ?? 1)
        this.page = pageFromQuery
        await this.useUser.getUser(this.page)
    },
    computed: {
        totalPages(): number {
            const totalCount = this.useUser.userData?.pagination.totalCount ?? 0
            const pageSize = this.useUser.userData?.pagination.pageSize ?? 1

            return Math.ceil(totalCount / pageSize)
        }
    },
    methods: {
        async changePage(page: number) {
            this.page = page
            this.$router.push({ query: { page: page } })
            await this.useUser.getUser(this.page)
        }
    }
}

</script>

<template>
    <BaseLayout>
        <h1>{{ useUser.userData?.user.userName }}</h1>
        <div class="d-flex justify-content-between">
            <h2>My quizzes</h2>
            <RouterLink :to="{ name: 'createQuiz' }" class="btn btn-primary">Create Quiz</RouterLink>
        </div>
        <MyQuizzesTable :quizzes="useUser.userData?.user.quizzes" />

        <Pagination :page="page" :totalPages="totalPages" @page-changed="changePage" />
    </BaseLayout>
</template>