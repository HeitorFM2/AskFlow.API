package resource

import (
	"ask-flow/api/middleware"
	"ask-flow/api/service"

	"github.com/gin-gonic/gin"
)

func AppRoutes(router *gin.Engine) *gin.RouterGroup {

	v1 := router.Group("/v1")
	{
		v1.POST("/login", service.Login)
		v1.POST("/user", service.CreateUser)
		v1.GET("/posts/:id", middleware.RequireAuth, service.FindUserPost)
		v1.GET("/posts", middleware.RequireAuth, service.FindAll)
		v1.GET("/responses/:id", middleware.RequireAuth, service.FindResponsesPost)
		v1.GET("/user/:id", middleware.RequireAuth, service.FindUser)
		v1.GET("/post/:id", middleware.RequireAuth, service.FindDetaisPost)
		v1.POST("/post", middleware.RequireAuth, service.CreatePost)
		v1.POST("/response", middleware.RequireAuth, service.CreateResponse)
		v1.POST("/upload", middleware.RequireAuth, service.Upload)
		v1.PUT("/email/:id", middleware.RequireAuth, service.EditEmail)
		v1.PUT("/img/:id", middleware.RequireAuth, service.EditImg)
		v1.DELETE("/response/:id", middleware.RequireAuth, service.DeleteResponse)
		v1.DELETE("/post/:id", middleware.RequireAuth, service.DeletePost)
	}

	return v1

}
