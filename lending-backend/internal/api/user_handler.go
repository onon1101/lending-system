package api

import (
	"encoding/json"
	"log"
	"net/http"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"strconv"
	"strings"

	"github.com/gorilla/mux"
)

type UserHandler struct {
	UserRepo *db.UserRepository
}

func NewUserHandler (repo *db.UserRepository) *UserHandler {
	return &UserHandler{
		UserRepo: repo,
	}
}

// @Summary 建立新使用者
// @Description 註冊一個新的物品借閱系統使用者。
// @Tags Users
// @Accept json
// @Produce json
// @Param user body model.CreateUserRequest true "使用者創建請求"
// @Success 201 {object} model.UserResponse "成功創建的使用者記錄"
// @Failure 400 {object} map[string]string "請求資料格式錯誤"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/users [post]
func (h *UserHandler) CreateUser(w http.ResponseWriter, r *http.Request) {
	var req model.CreateUserRequest

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, `{"error": "Invalid request body"}`, http.StatusBadRequest)
		return
	}

	newUser, err := h.UserRepo.CreateUser(req)
	if err != nil {
		log.Printf("DB Error creating user: %v", err)
		http.Error(w, `{"error": "Failed to create user due to server error"}`, http.StatusInternalServerError)
		return 
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(newUser)
}

// @Summary 查詢特定使用者
// @Description 根據使用者 ID (user_id) 查詢其詳細資訊。
// @Tags Users
// @Produce json
// @Param user_id path int true "使用者 ID"
// @Success 200 {object} model.UserResponse "成功找到並回傳使用者資訊"
// @Failure 400 {object} map[string]string "ID 格式錯誤"
// @Failure 404 {object} map[string]string "找不到指定 ID 的使用者"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/users/{user_id} [get]
func (h *UserHandler) GetUserByID(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	idStr := vars["user_id"]

	userID, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, `{"error": "Invalid user ID format"}`, http.StatusBadRequest)
		return 
	}

	user, err := h.UserRepo.GetUserByID(userID)
	if err != nil {
        // 處理找不到使用者 (404) 的情況
		if strings.Contains(err.Error(), "不存在") {
			http.Error(w, `{"error": "User not found"}`, http.StatusNotFound)
			return
		}
        
		// 處理其他資料庫錯誤
        log.Printf("DB Error fetching user: %v", err)
		http.Error(w, `{"error": "Failed to retrieve user due to server error"}`, http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(user)
}