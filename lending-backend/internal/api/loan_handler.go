// internal/api/loan_handler.go

package api

import (
	"encoding/json"
	"net/http"
	"object-borrow-system/internal/db"
	"strconv"

	"github.com/gorilla/mux"
)

// LoanHandler 結構體
type LoanHandler struct {
	LoanRepo *db.LoanRepository
}

// NewLoanHandler 創建 LoanHandler 實例
func NewLoanHandler(repo *db.LoanRepository) *LoanHandler {
	return &LoanHandler{
		LoanRepo: repo,
	}
}

// @Summary 查詢特定使用者進行中的借閱記錄
// @Description 根據使用者 ID 查詢該使用者所有尚未完全歸還或狀態為 'On Loan' 的訂單詳情。
// @Tags Loans
// @Produce json
// @Param user_id path int true "使用者 ID"
// @Success 200 {array} model.UserLoanResponse "成功回傳借閱訂單列表"
// @Failure 400 {object} map[string]string "ID 格式錯誤"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/users/{user_id}/loans [get]
func (h *LoanHandler) GetUserActiveLoans(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	idStr := vars["user_id"]
    
    // 1. 解析路徑參數 ID
	userID, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid user ID format"}`, http.StatusBadRequest)
		return
	}

	// 2. 呼叫資料庫層邏輯
	loans, err := h.LoanRepo.GetActiveLoansByUserID(userID)
	if err != nil {
		http.Error(w, `{"error": "Failed to retrieve loans due to server error"}`, http.StatusInternalServerError)
		return
	}
    
	// 3. 回應成功
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	// 如果沒有記錄，會回傳空列表 []
	json.NewEncoder(w).Encode(loans)
}