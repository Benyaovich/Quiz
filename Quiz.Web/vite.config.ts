import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  server:{
    proxy:{
      '/api' : {
        target: 'https://localhost:7181',
        changeOrigin: true, //only for development
        secure: false, // only for development
        rewrite: (path) => path.replace(/^\/api/, '')
      },
      '/signalr':{
        target: 'https://localhost:7181',
        changeOrigin: true,
        ws: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/signalr/,'')
      }
    }
  },
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@router': fileURLToPath(new URL('./src/router',import.meta.url)),
      '@pages': fileURLToPath(new URL('./src/pages',import.meta.url)),
      '@guards': fileURLToPath(new URL('./src/router/guards',import.meta.url)),
      '@stores': fileURLToPath(new URL('./src/stores',import.meta.url)),
      '@layouts': fileURLToPath(new URL('./src/layouts',import.meta.url)),
      '@components': fileURLToPath(new URL('./src/components',import.meta.url)),
      '@utils': fileURLToPath(new URL('./src/utils',import.meta.url))
    }
  }
})
