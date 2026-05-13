# Dockerfile (位於 lending-frontend/ 根目錄)
# --- 階段 1: 建置 Svelte (Builder) ---
FROM node:20-alpine AS builder

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

# 🎯 修正點 1: 宣告一個建構參數 (ARG)
ARG VITE_APP_API_URL

# 🎯 修正點 2: 將建構參數轉換為環境變數 (ENV)，供 npm run build 使用
ENV VITE_APP_API_URL=${VITE_APP_API_URL}

# 複製程式碼並建置
COPY . .
RUN npm run build  # 執行 Svelte/Vite 的建置命令，產生 /dist

# --- 階段 2: 運行 Nginx (Final) ---
FROM nginx:stable-alpine

# 複製 Svelte 建置的靜態文件到 Nginx 的預設目錄
COPY --from=builder /app/dist /usr/share/nginx/html

# SvelteKit 靜態路由 fallback，支援 /login、/items/1 等路由刷新
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
