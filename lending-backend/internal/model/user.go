package model

import "time"

type User struct {
    ID           int       `json:"id" db:"id"`
    Email        string    `json:"email" db:"email"`
    PasswordHash string    `json:"-" db:"password_hash"` // 密碼雜湊不回傳給前端
    Nickname     string    `json:"nickname" db:"nickname"`
    Role         string    `json:"role" db:"role"`       // 用於 RoleMiddleware 權限控管
    CreatedAt    time.Time `json:"created_at" db:"created_at"`
    UpdatedAt    time.Time `json:"updated_at" db:"updated_at"`
}

type CreateUserRequest struct {
	Name         string `json:"name"`
	Email        string `json:"email"`
	PasswordHash string `json:"password_hash"`
}

type UserResponse struct {
	UserID int    `json:"user_id"`
	Name   string `json:"name"`
	Email  string `json:"email"`
}
