<script lang="ts">
import BaseLayout from '@layouts/BaseLayout.vue'
import QuizzesTable from '@components/QuizzesTable.vue'
import { useQuizzes } from '@stores/quizzesStore'
import Pagination from '@components/Pagination.vue'

export default {
    components: {
        BaseLayout,
        QuizzesTable,
        Pagination
    },
    data() {
        return {
            quizzesStore: useQuizzes(),
            page: 1
        }
    },
    async mounted() {
        await this.quizzesStore.getPublishedQuizzes(this.page)
    },
    computed: {
        totalPages(): number {
            const totalCount = this.quizzesStore.publishedQuizzes?.pagination.totalCount ?? 0
            const pageSize = this.quizzesStore.publishedQuizzes?.pagination.pageSize ?? 1
            return Math.ceil(totalCount / pageSize)
        }
    },
    methods: {
        async changePage(page: number) {
            this.page = page
            await this.quizzesStore.getPublishedQuizzes(this.page)
        }
    }
}
</script>

<template>
    <BaseLayout>
        <h1>Published Quizzes</h1>
        <QuizzesTable :quizzes="quizzesStore.publishedQuizzes?.quizzes" />

        <Pagination :page="page" :totalPages="totalPages" @page-changed="changePage" />
    </BaseLayout>
</template>