package middleware

import (
	"ask-flow/api/models"
	"ask-flow/configs"
	"fmt"
	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v4"
	"net/http"
	"os"
	"time"
)

func RequireAuth(ctx *gin.Context) {

	tokenString := ctx.GetHeader("Authorization")

	if tokenString == "" {
		ctx.AbortWithStatus(http.StatusUnauthorized)
		return
	}

	token, _ := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("Unexpected signing method: %v", token.Header["alg"])
		}

		return []byte(os.Getenv("SECRET")), nil
	})

	if claims, ok := token.Claims.(jwt.MapClaims); ok && token.Valid {

		if float64(time.Now().Unix()) > claims["exp"].(float64) {
			ctx.AbortWithStatus(http.StatusUnauthorized)
			return
		}

		var user models.Users
		configs.DB.Where("id = ? and email = ?", claims["user"], claims["email"]).First(&user)

		if user.ID == 0 {
			ctx.AbortWithStatus(http.StatusUnauthorized)
			return
		}

		ctx.Next()

	} else {
		ctx.AbortWithStatus(http.StatusUnauthorized)
		return
	}

}
