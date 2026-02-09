package middleware

import (
	"net/http"
	"object-borrow-system/internal/model"
	"os"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

func AuthMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		// 1. 取得 Authorization Header
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "未提供認證標頭"})
			c.Abort() // 停止後續 Handler 執行
			return
		}

		// 2. 檢查格式是否為 Bearer <token>
		parts := strings.SplitN(authHeader, " ", 2)
		if !(len(parts) == 2 && parts[0] == "Bearer") {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "認證格式錯誤"})
			c.Abort()
			return
		}

		// 3. 解析與驗證 Token
		tokenString := parts[1]
		claims := &model.AuthClaims{} // 使用專案定義的 Claims 結構

		token, err := jwt.ParseWithClaims(tokenString, claims, func(token *jwt.Token) (interface{}, error) {
			return []byte(os.Getenv("SECRET_KEY")), nil // 從環境變數讀取密鑰
		})

		if err != nil || !token.Valid {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "無效或過期的 Token"})
			c.Abort()
			return
		}

		// 4. 將解析出的資訊存入 Context，方便 Handler 使用
		c.Set("userID", claims.UserID)
		c.Set("userEmail", claims.Email)
		c.Set("userRole", claims.Role)

		c.Next() // 繼續執行下一個 Handler
	}
}

// 檢查使用者是否具備指定的角色權限
func RoleMiddleware(requiredRole string) gin.HandlerFunc {
	return func(c *gin.Context) {
		// 從 Context 中取出 AuthMiddleware 存入的資訊
		role, exists := c.Get("userRole")
		if !exists || role != requiredRole {
			c.JSON(http.StatusForbidden, gin.H{"error": "權限不足，僅限管理員訪問"})
			c.Abort()
			return
		}
		c.Next()
	}
}