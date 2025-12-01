package main

import (
	_ "object-borrow-system/docs"
	"object-borrow-system/internal/app"
)

// @title 物品借閱系統 API
// @version 1.0
// @description 這是一個基於 Go 語言和 PostgreSQL 構建的物品借閱系統後端 API。
// @host 192.168.2.110:8000
// @BasePath /
func main() {
	application := app.New()
	application.Run()
}
