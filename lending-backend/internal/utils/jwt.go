package utils

import (
	"object-borrow-system/internal/model"
	"os"
	"time"

	"github.com/golang-jwt/jwt/v5"
)


func GenerateToken(user model.User) (string, string, error) {
	var secretKey = []byte(os.Getenv("SECRET_KEY"))
	claims := model.AuthClaims{
		UserID: user.ID,
		Email:  user.Email,
		Role: user.Role,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(15 * time.Minute)), // Access Token 建議短一點
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	}

	accessToken, err := jwt.NewWithClaims(jwt.SigningMethodHS256, claims).SignedString(secretKey)
    if err != nil {
        return "", "", err
    }

	refreshTokenClaims := jwt.RegisteredClaims{
		Subject:   string(user.ID),
		ExpiresAt: jwt.NewNumericDate(time.Now().Add(7 * 24 * time.Hour)),
	}
	refreshToken, err := jwt.NewWithClaims(jwt.SigningMethodHS256, refreshTokenClaims).SignedString(secretKey)
    if err != nil {
        return "", "", err
    }

	return accessToken, refreshToken, nil
}
