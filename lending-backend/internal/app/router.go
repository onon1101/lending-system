package app

import (
	_ "object-borrow-system/docs" // 確保 swagger 文件已生成並匯入
	"object-borrow-system/internal/api"
	"object-borrow-system/internal/middleware" // 匯入 auth service 的中間件

	"github.com/gin-gonic/gin"
	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"
)

func NewRouter(
	system *api.APIHandler,
	users *api.AuthHandler,
	loans *api.LoanHandler,
	items *api.ItemHandler,
	media *api.MediaHandler,
) *gin.Engine {
	// 初始化 Gin 引擎
	r := gin.Default()

	// Swagger 路由切換為 gin-swagger
	r.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler))

	// --- 公開路由 (無需 Token) ---
	r.GET("/api/health", system.HealthCheck)
	r.GET("/api/status", system.GetSystemStatus)
	r.GET("/api/download", system.TestDownloadMedia)
	r.GET("/api/items", items.GetAllItems)

	// --- 受保護路由 (需要 JWT Token) ---
	apiV1 := r.Group("/api")
	// apiV1.Use(middleware.AuthMiddleware()) // 掛載身份驗證中間件
	{
		// Users 相關
		usersGroup := apiV1.Group("/users")
		{
			// 只有管理員可以手動建立使用者 (範例權限控管)
			usersGroup.POST("", middleware.RoleMiddleware("admin"), users.Register)
			usersGroup.GET("/:user_id", users.GetUserByID)
			usersGroup.GET("/name/:username", users.GetUserByName) // 建議增加 prefix 區隔 ID 與 Name
			usersGroup.GET("/:user_id/loans", loans.GetUserActiveLoans)
		}

		// Items 相關
		itemsGroup := apiV1.Group("/items")
		{
			// itemsGroup.GET("", items.GetAllItems)
			// 只有管理員可以新增或更新物品
			itemsGroup.POST("", middleware.RoleMiddleware("admin"), items.CreateItem)
			itemsGroup.GET("/:object_id", items.GetItemByID)
			itemsGroup.PUT("/:object_id", middleware.RoleMiddleware("admin"), items.UpdateItem)

			// 媒體上傳
			itemsGroup.POST("/:object_id/image", items.UploadItemImage)
			itemsGroup.POST("/media", items.UploadItemMedia)
			itemsGroup.GET("/media/:object_id", items.GetItemMedia)
		}

		// Loans 歷史紀錄
		apiV1.GET("/loans/items/history/:object_id", loans.GetLoanHistoryByItemID)

		// Media 專用路由 (Private)
		apiV1.POST("/media/private", media.UploadMediaPrivate)
	}

	return r
}
