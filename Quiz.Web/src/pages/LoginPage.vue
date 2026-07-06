<script lang="ts">

import LoginForm from '@components/LoginForm.vue'
import BaseLayout from '@layouts/BaseLayout.vue'

export default {
    components: {
        BaseLayout,
        LoginForm
    }
}
</script>

<template>
    <BaseLayout>
        <LoginForm />

    </BaseLayout>
</template>