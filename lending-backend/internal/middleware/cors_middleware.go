package middleware

import (
	"net/http"

	"github.com/rs/cors"
)

func EnableCORS(next http.Handler) http.Handler{
	c := cors.New(cors.Options{
        // 允許來自 Svelte 預設埠 5173 的請求
        // AllowedOrigins:   []string{"http://localhost:5173", "http://127.0.0.1:5173"}, 
        AllowedOrigins:   []string{"*"}, 
        AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
        AllowedHeaders:   []string{"Content-Type", "Authorization"},
        AllowCredentials: true,
        // 開發階段可以開啟 Debug 模式來觀察 Header 是否正確送出
        Debug:            false, 
    })

	return c.Handler(next)
}