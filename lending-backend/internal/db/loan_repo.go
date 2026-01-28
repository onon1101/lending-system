// internal/db/loan_repo.go

package db

import (
	"database/sql"
	"fmt"
	"object-borrow-system/internal/model"
	"time"
)

// LoanRepository 結構體
type LoanRepository struct {
	DB model.DBClient
}

// NewLoanRepository 創建一個新的 LoanRepository 實例
func NewLoanRepository(db model.DBClient) *LoanRepository {
	return &LoanRepository{DB: db}
}

// GetActiveLoansByUserID 查詢特定使用者 ID 下所有處於 "On Loan" 狀態的借閱記錄
func (r *LoanRepository) GetActiveLoansByUserID(userID int) ([]model.UserLoanResponse, error) {
	// 複雜的 SQL JOIN 語句，用於獲取訂單主體和所有細項物品資訊
	sqlStatement := `
		SELECT
			o.order_id, o.start_time, o.end_time, o.status AS order_status,
			od.object_id, i.object_name, od.detail_status, od.actual_return_time
		FROM orders o
		JOIN order_details od ON o.order_id = od.order_id
		JOIN items i ON od.object_id = i.object_id
		WHERE o.user_id = $1 AND o.status = 'On Loan' -- 篩選特定使用者和進行中的訂單
		ORDER BY o.order_id, i.object_id;
	`
    
    dbConn, ok := r.DB.(*model.RealDB)
    if !ok || dbConn.DB == nil {
        return nil, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
    }
    
	rows, err := dbConn.DB.Query(sqlStatement, userID)
	if err != nil {
		return nil, fmt.Errorf("查詢借閱記錄失敗: %w", err)
	}
	defer rows.Close()

	// 處理多筆記錄並組合 (將多個細項 Items 組合到同一個 Order 結構中)
	loansMap := make(map[int]model.UserLoanResponse)
	
	for rows.Next() {
		var (
			orderID int
			loan model.UserLoanResponse
			item model.LoanItemDetail
			actualReturnTime sql.NullTime // 使用 sql.NullTime 處理可為 NULL 的欄位
		)

		err := rows.Scan(
			&orderID, &loan.OrderStartTime, &loan.OrderEndTime, &loan.OrderStatus,
			&item.ObjectID, &item.ObjectName, &item.DetailStatus, &actualReturnTime,
		)
		if err != nil {
			return nil, fmt.Errorf("掃描借閱記錄時發生錯誤: %w", err)
		}
        
        // 處理 NULL 時間
        if actualReturnTime.Valid {
            item.ActualReturnTime = actualReturnTime.Time
        }

		// 檢查是否已在 Map 中 (如果 OrderID 已經存在，則只新增 Items)
		if existingLoan, found := loansMap[orderID]; found {
			existingLoan.Items = append(existingLoan.Items, item)
			loansMap[orderID] = existingLoan
		} else {
			// 如果是新的 Order，則初始化 Order 資訊
			loan.OrderID = orderID
			loan.UserID = userID
			loan.Items = append(loan.Items, item)
			loansMap[orderID] = loan
		}
	}

	// 轉換 Map 為 Slice 進行回傳
	loans := make([]model.UserLoanResponse, 0, len(loansMap))
	for _, loan := range loansMap {
		loans = append(loans, loan)
	}

	return loans, nil
}

// internal/db/loan_repo.go (新增)

// CreateLoan 建立新的借閱訂單，並更新物品狀態
func (r *LoanRepository) CreateLoan(req model.CreateLoanRequest) (model.UserLoanResponse, error) {
    // 獲取底層 DB 實例
    dbConn, ok := r.DB.(*model.RealDB)
    if !ok || dbConn.DB == nil {
        return model.UserLoanResponse{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
    }

    // 1. 開始資料庫交易 (Transaction)
    tx, err := dbConn.DB.Begin()
    if err != nil {
        return model.UserLoanResponse{}, fmt.Errorf("無法開始交易: %w", err)
    }
    defer tx.Rollback() // 確保在函數結束前 Rollback，除非明確 Commit

    // 2. 檢查物品狀態與鎖定
    // 實務上應檢查 item 是否為 'Available'，並使用 SELECT FOR UPDATE 鎖定
    // 這裡我們跳過鎖定邏輯以簡化

    // 3. 創建 Orders 主記錄
    var orderID int
    now := time.Now()
    endTime := now.Add(time.Hour * time.Duration(req.DurationHours))
    
    orderSQL := `
        INSERT INTO orders (user_id, start_time, end_time, status)
        VALUES ($1, $2, $3, 'On Loan')
        RETURNING order_id;
    `
    err = tx.QueryRow(orderSQL, req.UserID, now, endTime).Scan(&orderID)
    if err != nil {
        return model.UserLoanResponse{}, fmt.Errorf("創建訂單主記錄失敗: %w", err)
    }

    // 4. 創建 Order Details 記錄並更新 Items 狀態
    for _, objectID := range req.ItemsID {
        // 插入 Order Details 記錄
        detailSQL := `
            INSERT INTO order_details (order_id, object_id, detail_status)
            VALUES ($1, $2, 'On Loan');
        `
        _, err = tx.Exec(detailSQL, orderID, objectID)
        if err != nil {
            return model.UserLoanResponse{}, fmt.Errorf("創建訂單細項 %d 失敗: %w", objectID, err)
        }
        
        // 更新 Item 實體的狀態
        itemStatusUpdateSQL := `
            UPDATE items SET current_status = 'On Loan' WHERE object_id = $1 AND current_status = 'Available';
        `
        result, err := tx.Exec(itemStatusUpdateSQL, objectID)
        if err != nil {
            return model.UserLoanResponse{}, fmt.Errorf("更新物品狀態失敗 %d: %w", objectID, err)
        }
        
        // 檢查是否真的更新了（確保物品可用）
        rowsAffected, _ := result.RowsAffected()
        if rowsAffected == 0 {
            // 如果更新影響行數為 0，則表示物品不可用
            tx.Rollback()
            return model.UserLoanResponse{}, fmt.Errorf("物品 ID %d 不可用或不存在，交易取消", objectID)
        }
    }

    // 5. 提交交易
    err = tx.Commit()
    if err != nil {
        return model.UserLoanResponse{}, fmt.Errorf("提交交易失敗: %w", err)
    }

    // 6. 返回回應 (簡化，實際應用中應查詢完整結構)
    return model.UserLoanResponse{
        OrderID: orderID,
        UserID: req.UserID,
        OrderStatus: "On Loan",
        OrderStartTime: now,
        OrderEndTime: endTime,
        // Items 列表省略，可調用 GetItemByID 填充
    }, nil
}

// 查詢某物品的使用紀錄
func (r *LoanRepository) GetLoanHistoryByItemID(itemID int) ([]model.LoanRecord, error) {
    query := `
        SELECT
            c.start_time,
            c.end_time,
            d.name
        FROM items a
        LEFT JOIN order_details b ON b.object_id = a.object_id
        LEFT JOIN orders c ON c.order_id = b.order_id
        LEFT JOIN users d ON d.user_id = c.user_id
        WHERE a.object_id = $1
        ORDER BY c.start_time DESC`

    dbConn, ok := r.DB.(*model.RealDB)
    if !ok || dbConn.DB == nil {
        return []model.LoanRecord{}, fmt.Errorf("資料庫連線錯誤: 無法取得底層 DB 實例")
    }

    rows, err := dbConn.DB.Query(query, itemID)
    if err != nil {
        return nil, fmt.Errorf("查詢借閱紀錄失敗: %w", err)
    }
    defer rows.Close()

    var history []model.LoanRecord
    for rows.Next() {
        var record model.LoanRecord

        err := rows.Scan(
            &record.StartTime,
            &record.EndTime,
            &record.ObjectName,
        )
        if err != nil {
            return nil, fmt.Errorf("解析借閱紀錄失敗: %w", err)
        }

        history = append(history, record)
    }

    if err = rows.Err(); err != nil {
        return nil, err
    }

    return history, nil
} 