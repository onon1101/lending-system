package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
	"strings"

	_ "object-borrow-system/docs"
	"object-borrow-system/internal/api"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/storage"

	"github.com/joho/godotenv"
	"github.com/rs/cors"
	httpSwagger "github.com/swaggo/http-swagger"

	"github.com/gorilla/mux"
)

// @title 物品借閱系統 API
// @version 1.0
// @description 這是一個基於 Go 語言和 PostgreSQL 構建的物品借閱系統後端 API。
// @host 192.168.2.110:8000
// @BasePath /
func main() {
	if err := godotenv.Load(); err != nil {
		log.Println("找不到 .env 檔案，將使用預設環境變數")
	}

	port := os.Getenv("APP_PORT")
	if port == "" {
		port = "8000"
	}

	dbHost := os.Getenv("DB_HOST")
	dbPort := os.Getenv("DB_PORT")
	dbUser := os.Getenv("DB_USER")
	dbPass := os.Getenv("DB_PASSWORD")
	dbName := os.Getenv("DB_NAME")

	dbClient, err := db.InitPostgresDB(dbHost, dbPort, dbUser, dbPass, dbName)
	if err != nil {
		log.Fatalf("資料庫連線失敗: %v", err)
	}
	log.Println("成功連線到資料庫!")

	minioEndPoint := os.Getenv("MINIO_ENDPOINT")
	minioAccessKey := os.Getenv("MINIO_ACCESS_KEY")
	minioSecretKey := os.Getenv("MINIO_SECRET_KEY")
	minioBucket := os.Getenv("MINIO_BUCKET_NAME")

	publicEndPoint := minioEndPoint
	if strings.HasPrefix(minioEndPoint, "minio:") {
		publicEndPoint = "http://localhost:9000"
	} else {
		publicEndPoint = minioEndPoint
	}

	minioClient, err := storage.InitMinioClient(minioEndPoint, minioAccessKey, minioSecretKey, minioBucket)
	if err != nil {
		log.Fatalf("Minio 連線失敗。URL: %s, err: %v", publicEndPoint, err)
	}
	log.Println("成功連線到 Minio")

	storageRepo := storage.NewStorageRepository(minioClient, minioBucket, publicEndPoint)

	userRepo := db.NewUserRepository(dbClient)
	loanRepo := db.NewLoanRepository(dbClient)
	itemRepo := db.NewItemRepository(dbClient)

	handler := api.NewAPIHandler(dbClient)
	userHandler := api.NewUserHandler(userRepo)
	loanHandler := api.NewLoanHandler(loanRepo)
	itemHandler := api.NewItemHandler(itemRepo, storageRepo)

	r := mux.NewRouter()

	r.HandleFunc("/api/health", healthCheck).Methods("GET")
	r.HandleFunc("/api/status", handler.GetSystemStatus).Methods("GET")

	r.HandleFunc("/api/users", userHandler.CreateUser).Methods("POST")
	r.HandleFunc("/api/users/{user_id}", userHandler.GetUserByID).Methods("GET")
	r.HandleFunc("/api/users/{user_id}/loans", loanHandler.GetUserActiveLoans).Methods("GET")

	r.HandleFunc("/api/items", itemHandler.GetAllItems).Methods("GET")
	r.HandleFunc("/api/items", itemHandler.CreateItem).Methods("POST")
	r.HandleFunc("/api/items/{object_id}", itemHandler.GetItemByID).Methods("GET")
	r.HandleFunc("/api/items/{object_id}", itemHandler.UpdateItem).Methods("PUT")
	r.HandleFunc("/api/items/{object_id}/image", itemHandler.UploadItemImage).Methods("POST")

	r.PathPrefix("/swagger/").Handler(httpSwagger.WrapHandler)

	// --- CORS 配置與處理器包裝 ---
	c := cors.New(cors.Options{
		// 允許 Svelte 前端連線
		// AllowedOrigins: []string{"", "http://192.168.2.110:5173"},
		AllowedOrigins: []string{
			"http://localhost:5173",     // 宿主機的 Vite/前端
			"http://127.0.0.1:5173",     // 宿主機的 Vite/前端
			"http://192.168.2.110:5173", // 虛擬機 IP (Svelte frontend)
			"http://192.168.2.110:80",   // 虛擬機 IP (Docker Nginx Port 80)},
			"http://192.168.50.67",
			"http://192.168.2.228",
			"http://daki.onon1101.org",
			"https://daki.onon1101.org",
			"https://lending-api.onon1101.org",
		},
		AllowedMethods: []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowedHeaders: []string{"Content-Type", "Authorization"}, // 通常需要 Content-Type
		// AllowCredentials: true,
	})

	// 使用 c.Handler(r) 包裝路由器，得到一個新的 handler
	finalHandler := c.Handler(r) // 使用新的變數名 finalHandler

	fmt.Printf("Server listening on port %s...\n", port)
	log.Fatal(http.ListenAndServe(":"+port, finalHandler))
}

// @Summary 健康檢查
// @Description 檢查服務是否正常運行
// @Tags System
// @Produce plain
// @Success 200 {string} string "Service is running!"
// @Router /api/health [get]
func healthCheck(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader((http.StatusOK))
	fmt.Fprintf(w, "Service is running!")
}
