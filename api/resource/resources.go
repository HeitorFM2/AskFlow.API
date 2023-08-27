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
		v1.POST("/send/email", service.SendMailSimple)

		v1.Use(middleware.RequireAuth)

		v1.GET("/posts/:id", service.FindUserPost)
		v1.GET("/posts", service.FindAll)
		v1.GET("/responses/:id", service.FindResponsesPost)
		v1.GET("/user/:id", service.FindUser)
		v1.GET("/post/:id", service.FindDetaisPost)
		v1.POST("/post", service.CreatePost)
		v1.POST("/response", service.CreateResponse)
		v1.POST("/upload", service.UploadHandler)
		v1.PUT("/email/:id", service.EditEmail)
		v1.PUT("/username/:id", service.EditUsername)
		v1.PUT("/img/:id", service.EditImg)
		v1.DELETE("/response/:id", service.DeleteResponse)
		v1.DELETE("/post/:id", service.DeletePost)
	}

	return v1

}
