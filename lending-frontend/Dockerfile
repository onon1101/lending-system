# Dockerfile (位於 lending-frontend/ 根目錄)
# --- 階段 1: 建置 Svelte (Builder) ---
FROM node:20-alpine AS builder

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

# 複製程式碼並建置
COPY . .
RUN npm run build  # 執行 Svelte/Vite 的建置命令，產生 /dist

# --- 階段 2: 運行 Nginx (Final) ---
FROM nginx:stable-alpine

# 複製 Svelte 建置的靜態文件到 Nginx 的預設目錄
COPY --from=builder /app/dist /usr/share/nginx/html

# 複製 Nginx 配置檔 (如果需要自定義，這裡省略)
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]