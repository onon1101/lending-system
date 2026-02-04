// internal/api/media_handler.go

package api

import (
	"encoding/json"
	"log"
	"net/http"
	"net/url"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"object-borrow-system/internal/storage"
	"strconv"
	"strings"
)

type MediaHandler struct {
	MediaRepo *db.MediaRepository
	StorageRepo *storage.StorageRepository
}

func NewMediaHandler(repo *db.MediaRepository, storageRepo *storage.StorageRepository) *MediaHandler {
	return &MediaHandler{
		MediaRepo: repo,
		StorageRepo: storageRepo, 
	}
}

// 這是給 private video 用的，因為有些爬蟲抓不到。
func (h *MediaHandler) UploadMediaPrivate(w http.ResponseWriter, r *http.Request) {
	// 1. 設定最大上傳限制 (例如影片較大，設為 100MB)
	const maxUploadSize = 500 << 20
	if err := r.ParseMultipartForm(maxUploadSize); err != nil {
		http.Error(w, `{"error": "File too large"}`, http.StatusBadRequest)
		return
	}

	orderIDStr := r.FormValue("order_id")
	objectIDStr := r.FormValue("object_id")
	description := r.FormValue("description")
	link := r.FormValue("link")

	// objectIDStr 轉換型別從 string 到 int
	objectID, err := strconv.Atoi(objectIDStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid object_id"}`, http.StatusBadRequest)
		return
	}

	// orderID 轉換從 string 到 int
	var orderID *int
	if orderIDStr != "" && orderIDStr != "0" {
		val, err := strconv.Atoi(orderIDStr)
		if err != nil {
			http.Error(w, `{"error": "Invalid order_id"}`, http.StatusBadRequest)
			return
		}

		orderID = &val
	}

	file, handler, err := r.FormFile("file")
	if err != nil {
		http.Error(w, `{"error": "Missing File"}`, http.StatusBadRequest)
		return
	}
	defer file.Close()

	// 分類照片或者影片
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
		http.Error(w, `{"error": "Unsupported file type"}`, http.StatusBadRequest)
		return
	}

	if err != nil {
		log.Printf("Upload file: %v", err)
		http.Error(w, `{"error": "Upload Failed"}`, http.StatusBadRequest)
		RecycleMinioResource(h.StorageRepo, objectName)
		return
	}

	newFileUrl, err := url.Parse(fileURL);
	if err != nil {
		http.Error(w, `{"error": "網址解析問題"}`, http.StatusBadRequest)
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

	// 將 Media 的 metadata 寫進資料庫當中
	result, err := h.MediaRepo.CreateMedia(newMedia)
	if err != nil {
		RecycleMinioResource(h.StorageRepo, objectName)

		if strings.Contains(err.Error(), "不存在") {
			http.Error(w, `{"error": "Item not found"}`, http.StatusNotFound)
			return
		}

		log.Printf("DB update failed: %v", err)
		http.Error(w, `{"error": "Failed to save image URL to database"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(result)
}