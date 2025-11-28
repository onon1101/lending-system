package db

import (
	"database/sql"
	"fmt"

	_ "github.com/lib/pq"

	"object-borrow-system/internal/model"
)

func InitPostgresDB(host, port, user, password, dbname string) (model.DBClient, error) {
	connStr := fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=disable",
			host, port, user,password, dbname)

	db, err := sql.Open("postgres", connStr)
	if err != nil{
		return nil, fmt.Errorf("無法開啟資料庫連線: %w", err)
	}

	if err = db.Ping(); err != nil {
		db.Close()
		return nil, fmt.Errorf("無法離線到資料庫：%w", err)
	}

	db.SetMaxOpenConns(25)
	db.SetMaxIdleConns(25)

	return &model.RealDB{DB: db}, nil
}