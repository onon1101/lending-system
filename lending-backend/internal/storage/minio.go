package storage

import (
	"context"
	"fmt"
	"io"
	"log"
	"net/url"
	"strings"
	"time"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"
)

type StorageClient interface {
	PutObject(ctx context.Context, bucketName, objectName string, reader io.Reader, objectSize int64, opts minio.PutObjectOptions) (minio.UploadInfo, error)
	BucketExists(ctx context.Context, bucketName string) (bool, error)
	MakeBucket(ctx context.Context, bucketName string, opts minio.MakeBucketOptions) error
	SetBucketPolicy(ctx context.Context, bucketName, policy string) error
RemoveObject(ctx context.Context, bucketName, objectName string, opts minio.RemoveObjectOptions) error 
}

type StorageRepository struct {
	Client         StorageClient
	BucketName     string
	PublicEndpoint string
}

func NewStorageRepository(client StorageClient, bucketName string, publicEndpoint string) *StorageRepository {
	return &StorageRepository{
		Client:         client,
		BucketName:     bucketName,
		PublicEndpoint: publicEndpoint,
	}
}

func InitMinioClient(endpoint, accessKey, secretKey, bucketName string) (StorageClient, error) {
	minioClient, err := minio.New(endpoint, &minio.Options{
		Creds:  credentials.NewStaticV4(accessKey, secretKey, ""),
		Secure: false,
	})

	if err != nil {
		return nil, fmt.Errorf("連線 Minio 失敗: %w", err)
	}

	ctx := context.Background()
	found, err := minioClient.BucketExists(ctx, bucketName)
	if err != nil {
		log.Printf("檢查 Bucket 錯誤: %v", err)
	}

	if !found {
		err = minioClient.MakeBucket(ctx, bucketName, minio.MakeBucketOptions{})
		if err != nil {
			log.Fatalf("無法創建 Minio Bucket %s: %v", bucketName, err)
			return nil, err
		}

		log.Printf("成功創建 MinIO Bucket: %s", bucketName)

		policy := fmt.Sprintf(`{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"AWS":["*"]},"Action":["s3:GetObject"],"Resource":["arn:aws:s3:::%s/*"]}]}`, bucketName)
		if err = minioClient.SetBucketPolicy(ctx, bucketName, policy); err != nil {
			log.Printf("設定 MinIO Bucket Policy 失敗: %v", err)
		} else {
			log.Println("成功設定 MinIO Bucket Policy 為公開讀取")
		}
	} else {
		log.Printf("MinIO Bucket '%s' 已存在", bucketName)
	}

	return minioClient, nil
}

// UploadItemImage 負責將檔案上傳到物件儲存，並返回公開 URL
// 實務上 MinIOClient 應該是真正的 MinIO SDK 客戶端，
// 這裡假設我們有一個 Minio SDK 的實作。
func (r *StorageRepository) UploadItemImage(file io.Reader, fileSize int64, filename, contentType string) (string, string, error) { 
	ctx := context.Background()

// ----------------------------------------------------
	// 👈 FIXED: 魯棒的檔名淨化邏輯
	
	// 1. 拆分檔名和副檔名
	lastDot := strings.LastIndex(filename, ".")
	baseName := filename
	ext := "" // 包含點的副檔名，例如 ".png"
	if lastDot != -1 {
		baseName = filename[:lastDot]
		ext = filename[lastDot:] 
	}

	// 2. 替換掉 baseName 中的所有潛在問題字元 (空格、冒號、以及時間中的點)
	sanitizedBaseName := strings.ReplaceAll(baseName, " ", "_")
	sanitizedBaseName = strings.ReplaceAll(sanitizedBaseName, ":", "-")
	sanitizedBaseName = strings.ReplaceAll(sanitizedBaseName, ".", "_") // 👈 替換掉時間部分的分隔點

	safeFilename := sanitizedBaseName + ext
	// ----------------------------------------------------

	objectName := fmt.Sprintf("item-%d-%s", time.Now().UnixNano(), safeFilename)

	uploadInfo, err := r.Client.PutObject(ctx, r.BucketName, objectName, file, fileSize, minio.PutObjectOptions{
		ContentType: contentType,
	})
	if err != nil {
		return "", "", fmt.Errorf("上傳檔案到 MinIO 失敗: %w", err) 
	}

	log.Printf("成功上傳檔案: %s (大小: %d bytes)", uploadInfo.Key, uploadInfo.Size)

	// 組合公開 URL
	imageURL, err := url.JoinPath(r.PublicEndpoint, r.BucketName, objectName)
	if err != nil {
		// MODIFIED: 失敗時返回空字串和空 objectName
		return "", "", fmt.Errorf("組合圖片 URL 失敗: %w", err)
	}

	return imageURL, objectName, nil // 👈 MODIFIED: 返回 imageURL 和 objectName
}

func (r *StorageRepository) DeleteObject(objectName string) error { 
	if objectName == "" {
		return nil 
	}
	
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	// 執行刪除
	err := r.Client.RemoveObject(ctx, r.BucketName, objectName, minio.RemoveObjectOptions{})
	if err != nil {
		return fmt.Errorf("MinIO 刪除物件失敗 %s: %w", objectName, err)
	}

	log.Printf("INFO: Successfully deleted MinIO object: %s", objectName)
	return nil
}