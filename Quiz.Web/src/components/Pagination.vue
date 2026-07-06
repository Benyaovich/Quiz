<script lang="ts">

export default {
    props: {
        page: {
            type: Number,
            required: true
        },
        totalPages: {
            type: Number,
            required: true
        }
    },
    emits: ['page-changed'],
    computed: {
        pages(){
            const pages = []

            const current = this.page
            const total = this.totalPages
            const maxVisible = 5

            if (total <= maxVisible) {
                for (let i = 1; i <= total; i++) pages.push(i)
                return pages
            }

            let start = current - 2
            let end = current + 2

            if (start <= 1) {
                start = 1
                end = 5
            }

            if (end >= total) {
                end = total
                start = total - 4
            }

            if (start > 1) {
                pages.push(1)
                if (start > 2) pages.push('...')
            }

            for (let i = start; i <= end; i++) {
                pages.push(i)
            }

            if (end < total) {
                if (end < total - 1) pages.push('...')
                pages.push(total)
            }

            return pages
        }
    },
    methods: {
        changePage(p: number) {
            if (p >= 1 && p <= this.totalPages && p !== this.page) {
                this.$emit('page-changed', p)
            }
        }
    }
}
</script>

<template>
    <nav v-if="totalPages > 1" class="mt-4 d-flex justify-content-center">
        <ul class="pagination">
            <li class="page-item" :class="{ disabled: page === 1 }">
                <button class="page-link" :disabled="page === 1" @click="changePage(page - 1)">
                    Previous
                </button>
            </li>

            <li v-for="(p, index) in pages" :key="index" class="page-item"
                :class="{ active: p === page, disabled: p === '...' }">
                <button v-if="Number.isFinite(p)" class="page-link" @click="changePage(p as number)">
                    {{ p }}
                </button>

                <span v-else class="page-link">...</span>
            </li>

            <li class="page-item" :class="{ disabled: page === totalPages }">
                <button class="page-link" :disabled="page === totalPages" @click="changePage(page + 1)">
                    Next
                </button>
            </li>
        </ul>
    </nav>
</template>