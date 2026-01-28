package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	AppPort string

	DBHost     string
	DBPort     string
	DBUser     string
	DBPassword string
	DBName     string

	MinioEndpoint   string
	MinioAccessKey  string
	MinioSecretKey  string
	MinioBucketName string
}

func Load() *Config {
	if err := godotenv.Load(); err != nil {
		log.Println("找不到 .env 檔案，將使用預設環境變數")
	}

	return &Config{
		AppPort: getenv("APP_PORT", "8000"),

		DBHost:     getenv("DB_HOST", ""),
		DBPort:     getenv("DB_PORT", ""),
		DBUser:     getenv("DB_USER", ""),
		DBPassword: getenv("DB_PASSWORD", ""),
		DBName:     getenv("DB_NAME", ""),

		MinioEndpoint:   getenv("MINIO_ENDPOINT", ""),
		MinioAccessKey:  getenv("MINIO_ACCESS_KEY", ""),
		MinioSecretKey:  getenv("MINIO_SECRET_KEY", ""),
		MinioBucketName: getenv("MINIO_BUCKET_NAME", ""),
	}
}

func getenv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}
