import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  envPrefix: ['VITE_', 'services__'],
  server: {
    proxy: {
      '/api': {
        target:  process.env.services__api__https__0 || process.env.services__api__http__0,
        changeOrigin: true
      }
    }
  }
})
