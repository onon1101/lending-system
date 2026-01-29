package db

import (
	"fmt"
	"object-borrow-system/internal/model"
)

type MediaRepository struct {
	DB model.DBClient
}

func NewMediaRepository(db model.DBClient) *MediaRepository {
	return &MediaRepository{DB: db}
}
// ----------------------------------------------------
// CREATE: 創建影音紀錄
// ----------------------------------------------------
func (r *MediaRepository) CreateMedia(req model.CreateMediaRequest) (model.Media, error) {
	sqlStatement := `
		INSERT INTO media (order_id, object_id, type, url, link, description)
		VALUES ($1, $2, $3, $4, $5, $6)
		RETURNING media_id, created_at`

	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.Media{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
	}

	newMedia := model.Media{
		OrderID:     req.OrderID,
		ObjectID:    req.ObjectID,
		Type:        req.Type,
		URL:         req.URL,
		Description: req.Description,
		Link: req.Link,
	}

	err := dbConn.DB.QueryRow(sqlStatement,
		req.OrderID,
		req.ObjectID,
		req.Type,
		req.URL,
		req.Link,
		req.Description).Scan(&newMedia.MediaID, &newMedia.CreatedAt)

	if err != nil {
		return model.Media{}, fmt.Errorf("新增影音失敗: %w", err)
	}

	return newMedia, nil
}
