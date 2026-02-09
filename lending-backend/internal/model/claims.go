// internal/model/claims.go
package model

import "github.com/golang-jwt/jwt/v5"

type AuthClaims struct {
    UserID int    `json:"id"`
    Email  string `json:"email"`
    Role   string `json:"role"`
    jwt.RegisteredClaims
}