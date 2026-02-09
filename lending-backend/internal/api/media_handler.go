// internal/api/media_handler.go

package api

import (
	"log"
	"net/http"
	"net/url"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"object-borrow-system/internal/storage"
	"strconv"
	"strings"

	"github.com/gin-gonic/gin"
)

type MediaHandler struct {
	MediaRepo   *db.MediaRepository
	StorageRepo *storage.StorageRepository
}

func NewMediaHandler(repo *db.MediaRepository, storageRepo *storage.StorageRepository) *MediaHandler {
	return &MediaHandler{
		MediaRepo:   repo,
		StorageRepo: storageRepo,
	}
}

// UploadMediaPrivate 這是給 private video 用的，因為有些爬蟲抓不到。
func (h *MediaHandler) UploadMediaPrivate(c *gin.Context) {
	// 1. 取得表單欄位 (Gin 使用 PostForm 或 PostFormValue)
	orderIDStr := c.PostForm("order_id")
	objectIDStr := c.PostForm("object_id")
	description := c.PostForm("description")
	link := c.PostForm("link")

	// objectIDStr 轉換型別從 string 到 int
	objectID, err := strconv.Atoi(objectIDStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object_id"})
		return
	}

	// orderID 轉換從 string 到 int
	var orderID *int
	if orderIDStr != "" && orderIDStr != "0" {
		val, err := strconv.Atoi(orderIDStr)
		if err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid order_id"})
			return
		}
		orderID = &val
	}

	// 2. 取得影音檔案 (修正：c.FormFile 回傳 2 個值)
	handler, err := c.FormFile("file")
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Missing File"})
		return
	}

	// 開啟檔案流
	file, err := handler.Open()
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to open file"})
		return
	}
	defer file.Close()

	// 3. 分類照片或者影片
	contentType := handler.Header.Get("Content-Type")
	var fileURL, objectName string
	var mediaType string

	if strings.HasPrefix(contentType, "video/") {
		mediaType = "video"
		fileURL, objectName, err = h.StorageRepo.UploadItemVideo(file, handler.Size, handler.Filename, contentType)
	} else if strings.HasPrefix(contentType, "image/") {
		mediaType = "image"
		fileURL, objectName, err = h.StorageRepo.UploadItemImage(file, handler.Size, handler.Filename, contentType)
	} else {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Unsupported file type"})
		return
	}

	if err != nil {
		log.Printf("Upload file error: %v", err)
		RecycleMinioResource(h.StorageRepo, objectName) // 調用外部定義的清理函式
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Upload Failed"})
		return
	}

	// 4. 網址解析與重組
	newFileUrl, err := url.Parse(fileURL)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "網址解析問題"})
		return
	}

	newFileUrl.Scheme = "https"
	newFileUrl.Host = "lending-minio.onon1101.org"

	newMedia := model.CreateMediaRequest{
		OrderID:     orderID,
		ObjectID:    objectID,
		Type:        mediaType,
		URL:         newFileUrl.String(),
		Link:        link,
		Description: description,
	}

	// 5. 將 Media 的 metadata 寫進資料庫
	result, err := h.MediaRepo.CreateMedia(newMedia)
	if err != nil {
		RecycleMinioResource(h.StorageRepo, objectName)

		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "Item not found"})
			return
		}

		log.Printf("DB update failed: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save media record to database"})
		return
	}

	// 6. 回應成功
	c.JSON(http.StatusCreated, result)
}

func RecycleMinioResource(storageRepo *storage.StorageRepository, objectName string) {
	if err := storageRepo.DeleteObject(objectName); err != nil {
		log.Printf("WARNING: Failed to clean up MinIO object '%s' after DB transaction failure: %v", objectName, err)
	} else {
		log.Printf("INFO:  Successfully cleaned up MinIO object '%s' after DB failure.", objectName)
	}
}