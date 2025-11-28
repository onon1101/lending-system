package api

import (
	"encoding/json"
	"net/http"
	"object-borrow-system/internal/model"
)

type APIHandler struct {
	DBClient model.DBClient
}

func NewAPIHandler(db model.DBClient) *APIHandler {
	return &APIHandler{
		DBClient: db,
	}
}

// @Summary 服務與資料庫健康檢查
// @Description 檢查 API 服務是否運行，以及 PostgreSQL 資料庫連線是否成功。
// @Tags System
// @Produce json
// @Success 200 {object} map[string]string "狀態: ok, DB: ok"
// @Failure 503 {object} map[string]string "狀態: ok, DB: error"
// @Router /api/status [get]
func (h *APIHandler) GetSystemStatus(w http.ResponseWriter, r *http.Request) {
	dbStatus := "ok"
	statusCode := http.StatusOK

	if err := h.DBClient.Ping(); err != nil{
		dbStatus = "error"
		statusCode = http.StatusServiceUnavailable
	}

	response := map[string]string {
		"service" :"ok",
		"database": dbStatus,
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(response)
}