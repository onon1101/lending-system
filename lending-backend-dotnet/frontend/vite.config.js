import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { resolve } from 'node:path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [svelte()],
  build: {
    rollupOptions: {
      input: {
        index: resolve(__dirname, 'index.html'),
        item: resolve(__dirname, 'item.html'),
        login: resolve(__dirname, 'login.html'),
      },
    },
  },
  
  // ======================================
  // 核心配置：讓伺服器監聽所有介面 (0.0.0.0)
  // ======================================
  server: {
    host: '0.0.0.0', // 讓 Vite 伺服器監聽所有可用的網路介面
    port: 5173,      // 預設埠號，可以根據需要更改
    // 監聽宿主機的指定埠號，但這通常由 'host: 0.0.0.0' 自動處理
  },
});
