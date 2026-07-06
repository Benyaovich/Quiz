<script lang="ts">

import { RouterView } from 'vue-router'
import { useAuth } from '@stores/authStore'
import { useQuizzes } from '@stores/quizzesStore'

export default {
  components: {
    RouterView
  },
  async mounted() {
    useAuth().initialize()
    await useQuizzes().initialize()
  }
}

</script>

<template>
  <RouterView />
</template>
