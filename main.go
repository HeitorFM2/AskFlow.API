package main

import (
	"ask-flow/api/resource"
	"ask-flow/configs"
	"log"
	"os"

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

	app.Use(cors.Default())
	resource.AppRoutes(app)

	app.Run(os.Getenv("APP_PORT"))
	// configs.DB.AutoMigrate(&models.Responses{})
}
