package service

import (
	"ask-flow/api/models"
	"ask-flow/configs"
	"net/http"

	"github.com/gin-gonic/gin"
)

var users []models.Users
var response models.Response
var questions []models.Questions

// fazendo requisição por where email

func Login(ctx *gin.Context) {
	var user models.Users
	if err := ctx.BindJSON(&user); err != nil {
		return
	}
	var getUser models.Users
	result := configs.DB.Where("email = ?", &user.Email).Find(&getUser)

	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	if result.RowsAffected == 0 || models.VerifyPassword(getUser.Password, user.Password) != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = "Invalid user or password!"
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Login successfully!"
	response.Data = &getUser

	ctx.JSON(http.StatusOK, response)
}

func FindUserPost(ctx *gin.Context) {
	id := ctx.Param("id")

	configs.DB.Where("iduser = ?", id).Find(&questions)

	response.Status_code = http.StatusOK
	response.Success = true
	response.Data = questions

	ctx.JSON(http.StatusOK, response)
}

func FindUser(ctx *gin.Context) {
	id := ctx.Param("id")
	var user models.Users
	configs.DB.Find(&user, id)

	response.Status_code = http.StatusOK
	response.Success = true
	response.Data = user

	ctx.JSON(http.StatusOK, response)
}

func FindDetaisPost(ctx *gin.Context) {
	id := ctx.Param("id")
	var questionReponse []models.QuestionsReponse

	configs.DB.Raw("SELECT q.id,q.iduser,q.message, u.first_name, u.last_name FROM questions q INNER JOIN users u on u.id = q.iduser WHERE q.id = ?", id).Scan(&questionReponse)

	response.Status_code = http.StatusOK
	response.Success = true
	response.Data = &questionReponse

	ctx.JSON(http.StatusOK, response)
}

func FindResponsesPost(ctx *gin.Context) {
	id := ctx.Param("id")
	var responsesPost []models.ResponsesPost
	configs.DB.Raw(`select r.idquestion, r.iduser, r.message, u.first_name, u.last_name from responses r 
	inner join users u on u.id = r.iduser where r.idquestion = ?`, id).Scan(&responsesPost)

	response.Status_code = http.StatusOK
	response.Success = true
	response.Data = &responsesPost

	ctx.JSON(http.StatusOK, response)
}

// fazendo requisição pegando todos da tabela
func FindAll(ctx *gin.Context) {
	var questionReponse []models.QuestionsReponse
	configs.DB.Raw(`
		SELECT
			q.id,
			q.iduser,
			q.message,
			u.first_name,
			u.last_name,
			COALESCE(COUNT(r.idquestion), 0) AS response
		FROM
			questions q
		INNER JOIN users u ON u.id = q.iduser
		LEFT JOIN responses r ON r.idquestion = q.id
		WHERE q.deleted_at IS NULL
		GROUP BY q.id, q.iduser, q.message, u.first_name, u.last_name
		ORDER BY q.id;
	`).Scan(&questionReponse)
	response.Status_code = http.StatusOK
	response.Success = true
	response.Data = &questionReponse
	ctx.JSON(http.StatusOK, response)
}

func CreateUser(ctx *gin.Context) {
	var user models.Users
	if err := ctx.BindJSON(&user); err != nil {
		return
	}

	hashedPassword := models.HashPassword(user.Password)

	user.Password = hashedPassword
	result := configs.DB.Create(&user)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "User created successfully!"
	ctx.JSON(http.StatusOK, response)

}

func CreatePost(ctx *gin.Context) {

	var post models.Questions

	if err := ctx.BindJSON(&post); err != nil {
		return
	}

	result := configs.DB.Create(&post)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Post created successfully!"
	ctx.JSON(http.StatusOK, response)

}

func CreateResponse(ctx *gin.Context) {

	var res models.Responses

	if err := ctx.BindJSON(&res); err != nil {
		return
	}

	result := configs.DB.Create(&res)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, res)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Response created successfully!"
	ctx.JSON(http.StatusOK, response)

}

func UpdateTweet(ctx *gin.Context) {
	var user models.Users
	id := ctx.Param("id")

	if err := ctx.BindJSON(&user); err != nil {
		return
	}

	result := configs.DB.Model(&user).Where("email = ?", id).Updates(&user)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, gin.H{"message": "InternalServerError"})
		return
	}

	ctx.JSON(http.StatusOK, user)

}

func Delete(ctx *gin.Context) {
	id := ctx.Param("id")

	result := configs.DB.Delete(&users, "email = ?", id)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, gin.H{"message": "InternalServerError"})
		return
	}

	ctx.JSON(http.StatusOK, gin.H{"success": true, "message": "Successfully deleted user"})
}

func DeletePost(ctx *gin.Context) {
	id := ctx.Param("id")

	var res models.Questions

	result := configs.DB.Delete(&res, id)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, gin.H{"message": "InternalServerError"})
		return
	}

	ctx.JSON(http.StatusOK, gin.H{"success": true, "message": "Successfully deleted post"})

}
