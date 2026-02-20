package api

import (
	"log"
	"net/http"
	"net/url"
	"object-borrow-system/internal/db"
	"object-borrow-system/internal/model"
	"object-borrow-system/internal/utils"
	"strconv"
	"strings"

	"github.com/gin-gonic/gin"
)

type AuthHandler struct {
	UserRepo *db.AuthRepository
}

func NewAuthHandler(repo *db.AuthRepository) *AuthHandler{
	return & AuthHandler{
		UserRepo: repo,
	}
}


// LoginHandler 登入並取得 Token
// @Summary      使用者登入
// @Description  透過 Email 與密碼進行驗證，成功後回傳 Access Token 與 Refresh Token
// @Tags         auth
// @Accept       json
// @Produce      json
// @Param        request  body      LoginRequest  true  "登入資訊"
// @Success      200      {object}  map[string]string "成功回傳 token"
// @Failure      401      {object}  map[string]string "帳號密碼錯誤"
// @Router       /auth/login [post]
func (d *AuthHandler) LoginHandler() gin.HandlerFunc {
	return func(c *gin.Context) {
		var req model.LoginRequest
		if err := c.ShouldBindJSON(&req); err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "格式錯誤"})
			return
		}

		user, err := d.UserRepo.FindUserByEmail(req.Email)
		if err != nil {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "帳號或密碼錯誤"})
			return
		}

		if !utils.CheckPasswordHash(req.Password, user.PasswordHash) {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "帳號或密碼錯誤"})
		}

		access, refresh, err := utils.GenerateToken(user)
		if err != nil {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "系統錯誤"})
		}

		c.JSON(http.StatusOK, gin.H{
			"access_token":  access,
			"refresh_token": refresh,
		})
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
func(h *AuthHandler) Register(c *gin.Context) {
		var req model.CreateUserRequest

	// 使用 Gin 的 ShouldBindJSON 取代 json.NewDecoder(r.Body).Decode
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request body"})
		return
	}

	newUser, err := h.UserRepo.CreateUser(req)
	if err != nil {
		log.Printf("DB Error creating user: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create user due to server error"})
		return
	}

	// 使用 c.JSON 簡化回傳邏輯
	c.JSON(http.StatusCreated, newUser)
}

// @Summary 使用者 ID 查詢特定使用者
// @Description 根據使用者 ID (user_id) 查詢其詳細資訊。
// @Tags Users
// @Produce json
// @Param user_id path int true "使用者 ID"
// @Success 200 {object} model.UserResponse "成功找到並回傳使用者資訊"
// @Failure 400 {object} map[string]string "ID 格式錯誤"
// @Failure 404 {object} map[string]string "找不到指定 ID 的使用者"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/users/{user_id} [get]
func (h *AuthHandler) GetUserByID(c *gin.Context) {
	// 從 mux.Vars 改為使用 c.Param 取得路徑參數
	idStr := c.Param("user_id")

	// 將使用者 ID String 轉換成 Int 
	userID, err := strconv.Atoi(idStr)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid user ID format"})
		return
	}

	user, err := h.UserRepo.GetUserByID(userID)
	if err != nil {
		// 處理找不到使用者 (404) 的情況
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "User not found"})
			return
		}

		// 處理其他資料庫錯誤
		log.Printf("DB Error fetching user: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve user due to server error"})
		return
	}

	c.JSON(http.StatusOK, user)
}

func (h *AuthHandler) GetUserByEmail(c *gin.Context) {
	// TODO: 實作 Email 
	email := c.Param("email")

	user, err := h.UserRepo.FindUserByEmail(email)
	if err != nil {
		// 處理找不到使用者 (404) 的情況
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "User not found"})
			return
		}

		// 處理其他資料庫錯誤
		log.Printf("DB Error fetching user: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve user due to server error"})
		return
	}

	c.JSON(http.StatusOK, user)
}

func (h *AuthHandler) DeleteUser(c *gin.Context) {

}

// @Summary 使用者名稱查詢特定使用者
// @Description 根據使用者 Name 查詢其詳細資訊。
// @Tags Users
// @Produce json
// @Param username path string true "使用者名稱"
// @Success 200 {object} model.UserResponse "成功找到並回傳使用者資訊"
// @Failure 400 {object} map[string]string "姓名格式錯誤"
// @Failure 404 {object} map[string]string "找不到指定姓名的使用者"
// @Failure 500 {object} map[string]string "內部伺服器或資料庫錯誤"
// @Router /api/users/{username} [get]
func (h *AuthHandler) GetUserByName(c *gin.Context) {
		// 從 mux.Vars 改為使用 c.Param 取得路徑參數
	rawUsername := c.Param("username")

	// 如果 encoding 失敗的話
	username, err := url.QueryUnescape(rawUsername)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid encoding in username."})
		return
	}

	// 使用模糊查詢使用者
	user, err := h.UserRepo.SearchUserByName(username)
	if err != nil {
		// 處理使用者不存在的情況
		if strings.Contains(err.Error(), "不存在") {
			c.JSON(http.StatusNotFound, gin.H{"error": "User not found"})
			return
		}

		// 其他資料庫錯誤的情況
		log.Printf("DB Error fetching user: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve user due to server error"})
		return
	}

	c.JSON(http.StatusOK, user)
}