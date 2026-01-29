package model

import "time"

type LoanItemDetail struct {
	ObjectDetailID   int       `json:"object_detail_id"`
	ObjectID         int       `json:"object_id"`
	ObjectName       string    `json:"object_name"`
	DetailStatus     string    `json:"detail_status"`
	ActualReturnTime time.Time `json:"actual_return_time,omitempty"`
}

type UserLoanResponse struct {
	OrderID        int              `json:"order_id"`
	UserID         int              `json:"user_id"`
	OrderStartTime time.Time        `json:"start_time"`
	OrderEndTime   time.Time        `json:"end_time"`
	OrderStatus    string           `json:"order_status"`
	Items          []LoanItemDetail `json:"items"`
}

// internal/model/loan.go (新增)

// CreateLoanRequest 用於接收創建借閱訂單的請求
type CreateLoanRequest struct {
	UserID int `json:"user_id" example:"1" binding:"required"`
	// 由於我們假設一個物品只有一個實體，ItemsID 是一個要借出的物品 ID 列表
	ItemsID       []int `json:"items_id" example:"[101, 102]" binding:"required"`
	DurationHours int   `json:"duration_hours" example:"72" binding:"required"` // 借閱時長 (小時)
}

// 全部都用 pointer 是因為有可能會有 null 值
type LoanRecord struct {
	StartTime  *time.Time `json:"start_time"`
	EndTime    *time.Time `json:"end_time"`
	ObjectName *string    `json:"name"`
	Status     *string    `json:"status"`
}
