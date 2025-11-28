# Dockerfile (位於 object-borrow-system/ 根目錄)
# --- 階段 1: 建置 (Builder) ---
FROM golang:1.24-alpine AS builder

WORKDIR /app

# 複製並下載 Go 依賴
COPY go.mod go.sum ./
RUN go mod download

# 複製專案程式碼
COPY . .

# 編譯 Go 應用程式
# CGO_ENABLED=0 用於建立靜態二進制文件
RUN CGO_ENABLED=0 go build -o /app/server cmd/server/main.go

# --- 階段 2: 運行 (Final) ---
FROM alpine:latest
RUN apk --no-cache add ca-certificates

WORKDIR /root/
# 從 builder 階段複製編譯好的執行檔
COPY --from=builder /app/server .

# 暴露服務埠號
EXPOSE 8000

# 運行編譯後的程式
CMD ["./server"]