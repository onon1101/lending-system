package db

import (
	"database/sql"
	"errors"
	"fmt"
	"object-borrow-system/internal/model"
)

type UserRepository struct {
	DB model.DBClient
}

func NewUserRepository(db model.DBClient) *UserRepository {
	return &UserRepository{DB: db}
}

func (r *UserRepository) CreateUser(req model.CreateUserRequest) (model.UserResponse, error) {
	sql := `
		INSERT INTO users (name, email, password_hash)
		VALUES ($1, $2, $3)
		RETURNING user_id;
	`

	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.UserResponse{}, fmt.Errorf("資料庫連線錯誤：無法取得底層 DB 實例")
	}

	newUser := model.UserResponse {
		Name: req.Name,
		Email: req.Email,
	}

	err := dbConn.DB.QueryRow(sql,
	req.Name,
	req.Email,
	req.PasswordHash).Scan(&newUser.UserID)

	if err != nil {
		return model.UserResponse{}, fmt.Errorf("新增使用者失敗: %w" ,err)
	}

	return newUser, nil
}

func (r *UserRepository) GetUserByID (userID int) (model.UserResponse, error) {
	sqlStatement := `SELECT user_id, name, email
			FROM users
			WHERE user_id = $1;`

	dbConn, ok := r.DB.(*model.RealDB)
	if !ok || dbConn.DB == nil {
		return model.UserResponse{}, fmt.Errorf("資料庫連線錯誤：無法取得底層 DB 實例")
	}

	user := model.UserResponse{}

	err := dbConn.DB.QueryRow(sqlStatement, userID).Scan(
		&user.UserID,
		&user.Name,
		&user.Email,
	)

if err != nil {
		if errors.Is(err, sql.ErrNoRows) { 
			return model.UserResponse{}, fmt.Errorf("使用者 ID %d 不存在", userID)
		}
		// 其他資料庫錯誤
		return model.UserResponse{}, fmt.Errorf("查詢使用者失敗: %w", err)
	}

	return user, nil
}