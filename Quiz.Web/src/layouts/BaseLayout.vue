<script lang="ts">
import Navbar from '@components/Navbar.vue'

export default {
    components: {
        Navbar
    }

}

</script>

<template>
    <header>
        <Navbar />
    </header>

    <main class="container">
        <slot />
    </main>

    <footer>

    </footer>
</template>