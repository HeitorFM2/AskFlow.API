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
		ctx.JSON(http.StatusUnauthorized, models.ResponseUnauthorized("Token is missing!"))
		ctx.Abort()
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
			ctx.JSON(http.StatusUnauthorized, models.ResponseUnauthorized("Token has expired!"))
			ctx.Abort()
			return
		}

		var user models.Users
		result := configs.DB.Where("id = ?", claims["user"]).First(&user)

		if result.RowsAffected == 0 {
			ctx.JSON(http.StatusUnauthorized, models.ResponseUnauthorized("Invalid token!"))
			ctx.Abort()
			return
		}

		ctx.Set("user", claims["user"])

		ctx.Next()

	} else {
		ctx.JSON(http.StatusUnauthorized, models.ResponseUnauthorized("Invalid token!"))
		ctx.Abort()
		return
	}

}
