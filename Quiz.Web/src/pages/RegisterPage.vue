<script lang="ts">

import RegisterForm from '@components/RegisterForm.vue'
import BaseLayout from '@layouts/BaseLayout.vue'

export default {
    components: {
        BaseLayout,
        RegisterForm
    }
}
</script>

<template>
    <BaseLayout>
        <RegisterForm />
    </BaseLayout>
</template>