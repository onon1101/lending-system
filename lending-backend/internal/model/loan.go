package model

import "time"

type LoanItemDetail struct {
	ObjectID int `json:"object_id"`
	ObjectName string `json:"object_name"`
	DetailStatus string `json:"detail_status"`
	ActualReturnTime time.Time `json:"actual_return_time,omitempty"`
}

type UserLoanResponse struct {
	OrderID int `json:"order_id"`
	UserID int `json:"user_id"`
	OrderStartTime time.Time `json:"start_time"`
	OrderEndTime time.Time `json:"end_time"`
	OrderStatus string `json:"order_status"`
	Items []LoanItemDetail `json:"items"`
}