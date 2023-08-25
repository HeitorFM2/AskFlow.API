package main

import (
	"ask-flow/api/models"
	"ask-flow/api/resource"
	"ask-flow/configs"
	"log"
	"os"
	"time"

	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
)

func init() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loadgin .env file")
	}
	configs.Connection()
}

func main() {
	app := gin.Default()

	app.Use(cors.New(cors.Config{
		AllowOrigins:     []string{"https://askflow-webapp.vercel.app/"},
		AllowMethods:     []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowHeaders:     []string{"Content-Type", "Content-Length", "Accept-Encoding", "Authorization", "Cache-Control"},
		ExposeHeaders:    []string{"Content-Length"},
		AllowCredentials: true,
		MaxAge:           12 * time.Hour,
	}))

	resource.AppRoutes(app)

	configs.DB.AutoMigrate(&models.Responses{})
	configs.DB.AutoMigrate(&models.Users{})
	configs.DB.AutoMigrate(&models.Questions{})

	app.Run(os.Getenv("APP_PORT"))
}
