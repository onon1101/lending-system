// internal/api/item_handler.go

package api

import (
	"encoding/json"
	"log"
	"net/http"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"object-borrow-system/internal/storage"
	"strconv"
	"strings"

	"github.com/gorilla/mux"
)

// ItemHandler 結構體
type ItemHandler struct {
	ItemRepo    *db.ItemRepository
	storageRepo *storage.StorageRepository
}

// NewItemHandler 創建 ItemHandler 實例
func NewItemHandler(repo *db.ItemRepository, storageRepo *storage.StorageRepository) *ItemHandler {
	return &ItemHandler{
		ItemRepo:    repo,
		storageRepo: storageRepo,
	}
}

// ADDED: 圖片上傳函式
// @Summary 上傳物品圖片
// @Description 上傳圖片並將返回的 URL 更新到指定的物品 (object_id)。
// @Tags Items
// @Accept mpfd
// @Produce json
// @Param object_id path int true "物品 ID"
// @Param file formData file true "圖片檔案 (Key must be 'file')"
// @Success 200 {object} model.Item "成功更新圖片 URL 的物品記錄"
// @Failure 400 {object} map[string]string "請求錯誤或檔案類型錯誤"
// @Failure 404 {object} map[string]string "找不到指定 ID 的物品"
// @Failure 500 {object} map[string]string "圖片上傳或資料庫更新失敗"
// @Router /api/items/{object_id}/image [post]
func (h *ItemHandler) UploadItemImage(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	idStr := vars["object_id"]
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid object ID format"}`, http.StatusBadRequest)
		return
	}

	

	const maxUploadSize = 10 << 20 // 10MB
	r.Body = http.MaxBytesReader(w, r.Body, maxUploadSize)
	if err := r.ParseMultipartForm(maxUploadSize); err != nil {
		http.Error(w, `{"error": "File too large or failed to parse form"}`, http.StatusBadRequest)
		return
	}

	file, handler, err := r.FormFile("file")
	if err != nil {
		http.Error(w, `{"error": "Missing or invalid 'file' field in form"}`, http.StatusBadRequest)
		return
	}
	defer file.Close()

	contentType := handler.Header.Get("Content-Type")
	if !strings.HasPrefix(contentType, "image/") {
		http.Error(w, `{"error": "File must be an image type"}`, http.StatusBadRequest)
		return
	}

	imageURL, objectName,err := h.storageRepo.UploadItemImage(file, handler.Size, handler.Filename, contentType)
	if err != nil {
		log.Printf("Storage upload failed: %v", err)
		http.Error(w, `{"error": "Image upload failed due to server error"}`, http.StatusInternalServerError)
		return
	}

	updateReq := model.UpdateItemRequest{
		ImageURL: imageURL,
	}

	updatedItem, err := h.ItemRepo.UpdateItem(objectID, updateReq)
	if err != nil {
		if err := h.storageRepo.DeleteObject(objectName); err != nil {

			log.Printf("WARNING: Failed to clean up MinIO object '%s' after DB transaction failure: %v", objectName, err)
		} else {
			log.Printf("INFO:  Successfully cleaned up MinIO object '%s' after DB failure.", objectName)
		}

		if strings.Contains(err.Error(), "不存在") {
			http.Error(w, `{"error": "Item not found"}`, http.StatusNotFound)
			return
		}

		log.Printf("DB update failed: %v", err)

		http.Error(w, `{"error": "Failed to save image URL to database"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(updatedItem)
}

// @Summary 創建新物品
// @Description 新增一個可供借閱的物品實體。
// @Tags Items
// @Accept json
// @Produce json
// @Param item body model.CreateItemRequest true "物品創建請求"
// @Success 201 {object} model.Item "成功創建的物品記錄"
// @Failure 400 {object} map[string]string "請求資料格式錯誤"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/items [post]
func (h *ItemHandler) CreateItem(w http.ResponseWriter, r *http.Request) {
	var req model.CreateItemRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, `{"error": "Invalid request body"}`, http.StatusBadRequest)
		return
	}
	if req.ObjectName == "" {
		http.Error(w, `{"error": "ObjectName is required"}`, http.StatusBadRequest)
		return
	}

	newItem, err := h.ItemRepo.CreateItem(req)
	if err != nil {
		http.Error(w, `{"error": "Failed to create item"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(newItem)
}

// @Summary 查詢特定物品
// @Description 根據物品 ID 查詢其詳細資訊。
// @Tags Items
// @Produce json
// @Param object_id path int true "物品 ID"
// @Success 200 {object} model.Item "成功找到並回傳物品資訊"
// @Failure 404 {object} map[string]string "找不到指定 ID 的物品"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/items/{object_id} [get]
func (h *ItemHandler) GetItemByID(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	idStr := vars["object_id"]

	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid object ID format"}`, http.StatusBadRequest)
		return
	}

	item, err := h.ItemRepo.GetItemByID(objectID)
	if err != nil {
		if strings.Contains(err.Error(), "不存在") {
			http.Error(w, `{"error": "Item not found"}`, http.StatusNotFound)
			return
		}
		http.Error(w, `{"error": "Failed to retrieve item"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(item)
}

// @Summary 更新物品資訊
// @Description 根據物品 ID 更新物品名稱、描述或狀態。
// @Tags Items
// @Accept json
// @Produce json
// @Param object_id path int true "物品 ID"
// @Param item body model.UpdateItemRequest true "物品更新請求"
// @Success 200 {object} model.Item "成功更新的物品記錄"
// @Failure 400 {object} map[string]string "請求資料格式錯誤"
// @Failure 404 {object} map[string]string "找不到指定 ID 的物品"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/items/{object_id} [put]
func (h *ItemHandler) UpdateItem(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	idStr := vars["object_id"]
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid object ID format"}`, http.StatusBadRequest)
		return
	}

	var req model.UpdateItemRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, `{"error": "Invalid request body"}`, http.StatusBadRequest)
		return
	}

	updatedItem, err := h.ItemRepo.UpdateItem(objectID, req)
	if err != nil {
		if strings.Contains(err.Error(), "不存在") {
			http.Error(w, `{"error": "Item not found"}`, http.StatusNotFound)
			return
		}
		log.Printf("Update item DB error: %v", err)
		http.Error(w, `{"error": "Failed to update item"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(updatedItem)
}

// internal/api/item_handler.go (新增)

// @Summary 查詢所有物品列表
// @Description 獲取系統中所有可供借閱的物品列表。
// @Tags Items
// @Produce json
// @Success 200 {array} model.GetAllItemsResponse "成功回傳物品列表"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/items [get]
func (h *ItemHandler) GetAllItems(w http.ResponseWriter, r *http.Request) {
	items, err := h.ItemRepo.GetAllItems()
	if err != nil {
		http.Error(w, `{"error": "Failed to retrieve items"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(items)
}
