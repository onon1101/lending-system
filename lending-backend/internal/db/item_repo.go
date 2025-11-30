// internal/db/item_repo.go

package db

import (
	"database/sql"
	"errors"
	"fmt"
	"object-borrow-system/internal/model"
)

// ItemRepository 結構體，包含 DBClient
type ItemRepository struct {
	DB model.DBClient
}

// NewItemRepository 創建一個新的 ItemRepository 實例
func NewItemRepository(db model.DBClient) *ItemRepository {
	return &ItemRepository{DB: db}
}

// ----------------------------------------------------
// CREATE: 創建物品
// ----------------------------------------------------
func (r *ItemRepository) CreateItem(req model.CreateItemRequest) (model.Item, error) {
	sqlStatement := `
		INSERT INTO items (object_name, description, current_status)
		VALUES ($1, $2, 'Available') 
		RETURNING object_id, current_status;
	`
	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.Item{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
	}

	newItem := model.Item{
		ObjectName:  req.ObjectName,
		Description: req.Description,
	}

	// 執行查詢，Scan 返回的 ID 和 Status
	err := dbConn.DB.QueryRow(sqlStatement,
		req.ObjectName,
		req.Description,
	).Scan(&newItem.ObjectID, &newItem.CurrentStatus)

	if err != nil {
		return model.Item{}, fmt.Errorf("新增物品失敗: %w", err)
	}
	return newItem, nil
}

// ----------------------------------------------------
// READ: 查詢特定物品
// ----------------------------------------------------
func (r *ItemRepository) GetItemByID(objectID int) (model.Item, error) {
	sqlStatement := `
		SELECT object_id, object_name, description, current_status, image_url
		FROM items
		WHERE object_id = $1;
	`
	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.Item{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
	}

	item := model.Item{}
	var imageURL sql.NullString

	err := dbConn.DB.QueryRow(sqlStatement, objectID).Scan(
		&item.ObjectID,
		&item.ObjectName,
		&item.Description,
		&item.CurrentStatus,
		&imageURL,
	)

	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return model.Item{}, fmt.Errorf("物品 ID %d 不存在", objectID)
		}
		return model.Item{}, fmt.Errorf("查詢物品失敗: %w", err)
	}

	if imageURL.Valid {
		item.ImageURL = imageURL.String
	}

	return item, nil
}

// ----------------------------------------------------
// UPDATE: 更新物品資訊 (這裡只是一個簡化範例)
// ----------------------------------------------------
func (r *ItemRepository) UpdateItem(objectID int, req model.UpdateItemRequest) (model.Item, error) {
	// 實務上應建立動態 SQL，這裡為了簡化，僅更新所有可更新欄位
	sqlStatement := `
		UPDATE items SET 
			object_name = COALESCE(NULLIF($1, ''), object_name),
			description = COALESCE(NULLIF($2, ''), description),
			current_status = COALESCE(NULLIF($3, ''), current_status),
			image_url = $4
		WHERE object_id = $5
		RETURNING object_id, object_name, description, current_status, image_url;
	`
	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.Item{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
	}

	// 預處理空字串，以便 COALESCE 函數工作
	name := req.ObjectName
	if name == "" {
		name = ""
	}
	desc := req.Description
	if desc == "" {
		desc = ""
	}
	status := req.CurrentStatus
	if status == "" {
		status = ""
	}

	updatedItem := model.Item{}
	var imageURL sql.NullString

	// 執行更新
	err := dbConn.DB.QueryRow(sqlStatement,
		name,
		desc,
		status,
		req.ImageURL, // 直接傳入新的 ImageURL
		objectID,
	).Scan(
		&updatedItem.ObjectID,
		&updatedItem.ObjectName,
		&updatedItem.Description,
		&updatedItem.CurrentStatus,
		&imageURL,
	)

	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return model.Item{}, fmt.Errorf("更新失敗: 物品 ID %d 不存在", objectID)
		}
		return model.Item{}, fmt.Errorf("更新物品失敗: %w", err)
	}

	if imageURL.Valid {
		updatedItem.ImageURL = imageURL.String
	}

	return updatedItem, nil
}

// internal/db/item_repo.go (新增)

// GetAllItems 查詢所有物品，可選帶入狀態篩選 (這裡簡化為查詢全部)
func (r *ItemRepository) GetAllItems() ([]model.GetAllItemsResponse, error) {
	sqlStatement := `
		select 
			a.object_id, 
			a.object_name, 
			a.description, 
			a.current_status, 
			b.name as owner_name,
			b.email as owner_email,
			a.image_url
		from items a
		LEFT JOIN users b ON b.user_id = a.owner_id
		ORDER BY a.object_id;
	`

	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return nil, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
	}

	rows, err := dbConn.DB.Query(sqlStatement)
	if err != nil {
		return nil, fmt.Errorf("查詢所有物品失敗: %w", err)
	}
	defer rows.Close()

	var items []model.GetAllItemsResponse
	for rows.Next() {
		var item model.GetAllItemsResponse

		err := rows.Scan(
			&item.ObjectID,
			&item.ObjectName,
			&item.Description,
			&item.CurrentStatus,
			&item.OwnerName,
			&item.OwnerEmail,
			&item.ImageURL,
		)
		if err != nil {
			return nil, fmt.Errorf("掃描物品記錄時發生錯誤: %w", err)
		}

		items = append(items, item)
	}

	return items, nil
}