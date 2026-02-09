// internal/api/item_handler.go

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

// ItemHandler 結構體
type ItemHandler struct {
	ItemRepo    *db.ItemRepository
	storageRepo *storage.StorageRepository
	mediaRepo   *db.MediaRepository
}

// NewItemHandler 創建 ItemHandler 實例
func NewItemHandler(repo *db.ItemRepository, storageRepo *storage.StorageRepository, mediaRepo *db.MediaRepository) *ItemHandler {
	return &ItemHandler{
		ItemRepo:    repo,
		storageRepo: storageRepo,
		mediaRepo:   mediaRepo,
	}
}

// @Summary 上傳圖片封面
// @Description 上傳圖片並將返回的 URL 更新到指定的物品 (object_id)。
// @Tags Items
// @Accept mpfd
// @Produce json
// @Param object_id path int true "物品 ID"
// @Param file formData file true "圖片檔案 (Key must be 'file')"
// @Success 200 {object} model.Item "成功更新圖片 URL 的物品記錄"
// @Router /api/items/{object_id}/image [post]
func (h *ItemHandler) UploadItemImage(c *gin.Context) {
	idStr := c.Param("object_id")
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object ID format"})
		return
	}

	// Gin 會自動處理 MultipartForm 解析，也可透過 c.Request.Body 限制大小
	handler, err := c.FormFile("file")
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Missing or invalid 'file' field in form"})
		return
	}

	contentType := handler.Header.Get("Content-Type")
	if !strings.HasPrefix(contentType, "image/") {
		c.JSON(http.StatusBadRequest, gin.H{"error": "File must be an image type"})
		return
	}

	// 開啟檔案流
	openedFile, _ := handler.Open()
	defer openedFile.Close()

	imageURL, objectName, err := h.storageRepo.UploadItemImage(openedFile, handler.Size, handler.Filename, contentType)
	if err != nil {
		log.Printf("Storage upload failed: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Image upload failed due to server error"})
		return
	}

	updateReq := model.UpdateItemRequest{
		ImageURL: imageURL,
	}

	updatedItem, err := h.ItemRepo.UpdateItem(objectID, updateReq)
	if err != nil {
		_ = h.storageRepo.DeleteObject(objectName) // 失敗時清理 MinIO
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "Item not found"})
			return
		}
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save image URL to database"})
		return
	}

	c.JSON(http.StatusOK, updatedItem)
}

// @Summary 創建新物品
// @Description 新增一個可供借閱的物品實體。
// @Tags Items
// @Accept json
// @Produce json
// @Param item body model.CreateItemRequest true "物品創建請求"
// @Success 201 {object} model.Item "成功創建的物品記錄"
// @Router /api/items [post]
func (h *ItemHandler) CreateItem(c *gin.Context) {
	var req model.CreateItemRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request body"})
		return
	}

	if req.ObjectName == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "ObjectName is required"})
		return
	}

	newItem, err := h.ItemRepo.CreateItem(req)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create item"})
		return
	}

	c.JSON(http.StatusCreated, newItem)
}

// @Summary 查詢特定物品
// @Description 根據物品 ID 查詢其詳細資訊。
// @Tags Items
// @Produce json
// @Param object_id path int true "物品 ID"
// @Success 200 {object} model.Item "成功找到並回傳物品資訊"
// @Router /api/items/{object_id} [get]
func (h *ItemHandler) GetItemByID(c *gin.Context) {
	idStr := c.Param("object_id")
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object ID format"})
		return
	}

	item, err := h.ItemRepo.GetItemByID(objectID)
	if err != nil {
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "Item not found"})
			return
		}
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve item"})
		return
	}

	c.JSON(http.StatusOK, item)
}

// @Summary 更新物品資訊
// @Description 根據物品 ID 更新物品名稱、描述或狀態。
// @Tags Items
// @Accept json
// @Produce json
// @Param object_id path int true "物品 ID"
// @Param item body model.UpdateItemRequest true "物品更新請求"
// @Success 200 {object} model.Item "成功更新的物品記錄"
// @Router /api/items/{object_id} [put]
func (h *ItemHandler) UpdateItem(c *gin.Context) {
	idStr := c.Param("object_id")
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object ID format"})
		return
	}

	var req model.UpdateItemRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request body"})
		return
	}

	updatedItem, err := h.ItemRepo.UpdateItem(objectID, req)
	if err != nil {
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "Item not found"})
			return
		}
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to update item"})
		return
	}

	c.JSON(http.StatusOK, updatedItem)
}

// @Summary 查詢所有物品列表
// @Description 獲取系統中所有可供借閱的物品列表。
// @Tags Items
// @Produce json
// @Success 200 {array} model.GetAllItemsResponse "成功回傳物品列表"
// @Router /api/items [get]
func (h *ItemHandler) GetAllItems(c *gin.Context) {
	items, err := h.ItemRepo.GetAllItems()
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve items"})
		return
	}

	c.JSON(http.StatusOK, items)
}

// @Summary 上傳物品的媒體 (影片或圖片)
// @Description 上傳檔案並在資料庫建立媒體記錄，關聯到特定訂單與物品。
// @Tags Items
// @Accept multipart/form-data
// @Produce json
// @Success 201 {object} model.Media "成功建立的媒體記錄"
// @Router /api/items/media [post]
func (h *ItemHandler) UploadItemMedia(c *gin.Context) {
	orderIDStr := c.PostForm("order_id")
	objectIDStr := c.PostForm("object_id")
	description := c.PostForm("description")
	link := c.PostForm("link")

	objectID, err := strconv.Atoi(objectIDStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object_id"})
		return
	}

	var orderID *int
	if orderIDStr != "" && orderIDStr != "0" {
		val, _ := strconv.Atoi(orderIDStr)
		orderID = &val
	}

	handler, err := c.FormFile("file")
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Missing File"})
		return
	}

	openedFile, _ := handler.Open()
	defer openedFile.Close()

	contentType := handler.Header.Get("Content-Type")
	var fileURL, objectName string
	var mediaType string

	if strings.HasPrefix(contentType, "video/") {
		mediaType = "video"
		fileURL, objectName, err = h.storageRepo.UploadItemVideo(openedFile, handler.Size, handler.Filename, contentType)
	} else if strings.HasPrefix(contentType, "image/") {
		mediaType = "image"
		fileURL, objectName, err = h.storageRepo.UploadItemImage(openedFile, handler.Size, handler.Filename, contentType)
	} else {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Unsupported file type"})
		return
	}

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Upload Failed"})
		return
	}

	// 網址處理
	newFileUrl, _ := url.Parse(fileURL)
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

	result, err := h.mediaRepo.CreateMedia(newMedia)
	if err != nil {
		_ = h.storageRepo.DeleteObject(objectName)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save media record"})
		return
	}

	c.JSON(http.StatusCreated, result)
}

// @Summary 取得物品的所有影音媒體
// @Description 根據物品 ID (object_id) 獲取該物品相關的所有媒體檔案。
// @Tags Items
// @Produce json
// @Param object_id path int true "物品 ID"
// @Success 200 {array} model.Media "媒體檔案列表"
// @Router /api/items/media/{object_id} [get]
func (h *ItemHandler) GetItemMedia(c *gin.Context) {
	idStr := c.Param("object_id")
	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object ID format"})
		return
	}

	mediaItem, err := h.ItemRepo.GetItemMediaByItemID(objectID)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve items"})
		return
	}

	c.JSON(http.StatusOK, mediaItem)
}