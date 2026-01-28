package app

import (
	"log"
	"net/http"
	"object-borrow-system/internal/api"
	"object-borrow-system/internal/config"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/storage"
)

type Application struct {
	cfg    *config.Config
	router http.Handler
}

func New() *Application {
	cfg := config.Load()

	dbClient, err := db.InitPostgresDB(cfg.DBHost, cfg.DBPort, cfg.DBUser, cfg.DBPassword, cfg.DBName)
	if err != nil {
		log.Fatalf("資料庫連線失敗: %v", err)
	}
	log.Println("資料庫連線成功")

	minioClient, err := storage.InitMinioClient(cfg.MinioEndpoint, cfg.MinioAccessKey, cfg.MinioSecretKey, cfg.MinioBucketName)
	if err != nil {
		log.Fatalf("Minio 連線失敗: %v", err)
	}
	log.Println("MinIO 連線成功")

	// Repo
	userRepo := db.NewUserRepository(dbClient)
	loanRepo := db.NewLoanRepository(dbClient)
	itemRepo := db.NewItemRepository(dbClient)
	mediaRepo := db.NewMediaRepository(dbClient)
	storageRepo := storage.NewStorageRepository(minioClient, cfg.MinioBucketName, cfg.MinioEndpoint)

	// Handlers
	userHandler := api.NewUserHandler(userRepo)
	loanHandler := api.NewLoanHandler(loanRepo)
	itemHandler := api.NewItemHandler(itemRepo, storageRepo, mediaRepo)
	systemHandler := api.NewAPIHandler(dbClient)

	router := NewRouter(systemHandler, userHandler, loanHandler, itemHandler)

	return &Application{
		cfg:    cfg,
		router: router,
	}
}

func (a *Application) Run() error {
	log.Printf("Server is listening on port %s...\n", a.cfg.AppPort)
	return http.ListenAndServe(":"+a.cfg.AppPort, a.router)
}
