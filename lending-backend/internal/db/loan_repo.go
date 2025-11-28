// internal/db/loan_repo.go

package db

import (
	"database/sql"
	"fmt"
	"object-borrow-system/internal/model"
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