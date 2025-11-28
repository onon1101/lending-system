package model

import "time"

type User struct {
	UserID int `json:"user_id"`
	Name string `json:"name"`
	Email string `json:"email"`
	PasswordHash string `json:"password_hash`
	CreatedAt time.Time `json:"created_at"`
}

type CreateUserRequest struct {
	Name string `json:"name"`
	Email string `json:"email"`
	PasswordHash string `json:"password_hash"`
}

type UserResponse struct {
	UserID int `json:"user_id"`
	Name string `json:"name"`
	Email string `json:"email"`
}