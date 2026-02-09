// internal/api/loan_handler.go

package api

import (
	"fmt"
	"net/http"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"strconv"

	"github.com/gin-gonic/gin"
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
func (h *LoanHandler) GetUserActiveLoans(c *gin.Context) {
	// 1. 使用 Gin 的 c.Param 取得路徑參數
	idStr := c.Param("user_id")

	userID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid user ID format"})
		return
	}

	// 2. 呼叫資料庫層邏輯
	loans, err := h.LoanRepo.GetActiveLoansByUserID(userID)
	if err != nil {
		// 修正：伺服器錯誤應回傳 500
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve loans due to server error"})
		return
	}

	// 3. 回應成功
	c.JSON(http.StatusOK, loans)
}

// @Summary 創建新的借閱訂單
// @Description 建立一筆新的借閱交易，並將所有指定物品的狀態更新為 'On Loan'。
// @Tags Loans
// @Accept json
// @Produce json
// @Param loan body model.CreateLoanRequest true "創建借閱請求"
// @Success 201 {object} model.UserLoanResponse "成功創建的訂單記錄"
// @Failure 400 {object} map[string]string "請求資料格式錯誤或物品不可用"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/loans [post]
func (h *LoanHandler) CreateLoan(c *gin.Context) {
	var req model.CreateLoanRequest

	// 使用 Gin 的 ShouldBindJSON 取代 json.NewDecoder(r.Body).Decode
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request body"})
		return
	}

	// 簡易驗證
	if req.UserID == 0 || len(req.ItemsID) == 0 || req.DurationHours == 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Missing required fields (user_id, items_id, duration_hours)"})
		return
	}

	loan, err := h.LoanRepo.CreateLoan(req)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Failed to create loan: %s", err.Error())})
		return
	}

	c.JSON(http.StatusCreated, loan)
}

// @Summary 查詢物品借閱歷史紀錄
// @Description 根據物品 ID (object_id) 獲取該物品過去所有的借閱紀錄。
// @Tags Loans
// @Accept json
// @Produce json
// @Param object_id path int true "物品 ID"
// @Success 200 {array} model.LoanRecord "借閱歷史紀錄列表"
// @Failure 400 {object} map[string]string "無效的物品 ID 格式"
// @Failure 500 {object} map[string]string "伺服器內部錯誤"
// @Router /api/loans/items/history/{object_id} [get]
func (h *LoanHandler) GetLoanHistoryByItemID(c *gin.Context) {
	// 修正：從 mux.Vars(r) 改為 c.Param
	idStr := c.Param("object_id")

	objectID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid object ID format"})
		return
	}

	records, err := h.LoanRepo.GetLoanHistoryByItemID(objectID)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "系統處理錯誤"})
		return
	}

	c.JSON(http.StatusOK, records)
}