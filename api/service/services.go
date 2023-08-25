package service

import (
	"ask-flow/api/models"
	"ask-flow/configs"
	"github.com/golang-jwt/jwt/v4"
	"io"
	"net/http"
	"os"
	"time"

	"github.com/gin-gonic/gin"
)

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

	response.Status_code = http.StatusInternalServerError
	response.Success = false
	if result.Error != nil {
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	if result.RowsAffected == 0 || models.VerifyPassword(getUser.Password, user.Password) != nil {

		response.Message = "Invalid user or password!"
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	var token = jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		"exp": time.Now().Add(time.Hour * 24).Unix(),
		"sub": "getUser",
	})

	tokenString, err := token.SignedString([]byte(os.Getenv("SECRET")))

	if err != nil {
		response.Message = "Failed to create token!"
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Login successfully!"
	response.Data = &getUser

	ctx.JSON(http.StatusOK, gin.H{
		"data":  response,
		"token": tokenString,
	})
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
	configs.DB.Raw(`select r.id, r.idquestion, r.iduser, r.message, u.first_name, u.last_name, u.img from responses r 
	inner join users u on u.id = r.iduser where r.idquestion = ? order by r.created_at`, id).Scan(&responsesPost)

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
			u.img,
			COALESCE(COUNT(r.idquestion), 0) AS response
		FROM
			questions q
		INNER JOIN users u ON u.id = q.iduser
		LEFT JOIN responses r ON r.idquestion = q.id
		WHERE q.deleted_at IS NULL
		GROUP BY q.id, q.iduser, q.message, u.first_name, u.last_name, u.img
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
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Response created successfully!"
	ctx.JSON(http.StatusOK, response)

}

func EditEmail(ctx *gin.Context) {
	id := ctx.Param("id")

	var user struct {
		Email string `json:"email"`
	}

	if err := ctx.BindJSON(&user); err != nil {
		return
	}

	result := configs.DB.Model(&user).Where("id = ?", id).Updates(&user)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Email successfully edited!"
	ctx.JSON(http.StatusOK, response)

}

func EditImg(ctx *gin.Context) {
	id := ctx.Param("id")

	var user struct {
		Img string `json:"img"`
	}

	if err := ctx.BindJSON(&user); err != nil {
		return
	}

	result := configs.DB.Model(&user).Where("id = ?", id).Updates(&user)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Img successfully edited!"
	ctx.JSON(http.StatusOK, response)

}

func DeleteResponse(ctx *gin.Context) {
	id := ctx.Param("id")

	var res models.Response

	result := configs.DB.Delete(&res, id)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Successfully deleted post!"

	ctx.JSON(http.StatusOK, &response)

}

func DeletePost(ctx *gin.Context) {
	id := ctx.Param("id")

	var res models.Questions

	result := configs.DB.Delete(&res, id)
	if result.Error != nil {
		response.Status_code = http.StatusInternalServerError
		response.Success = false
		response.Message = result.Error.Error()
		ctx.JSON(http.StatusInternalServerError, response)
		return
	}

	response.Status_code = http.StatusOK
	response.Success = true
	response.Message = "Successfully deleted post!"

	ctx.JSON(http.StatusOK, &response)

}

func Upload(ctx *gin.Context) {
	url := "https://api.imgur.com/3/image"
	method := "POST"

	// Lê o corpo da requisição
	body := ctx.Request.Body

	client := &http.Client{}
	req, err := http.NewRequest(method, url, body)
	if err != nil {
		ctx.String(http.StatusInternalServerError, err.Error())
		return
	}

	req.Header.Add("Authorization", "Client-ID 5f24e4cd127e1ac")
	req.Header.Set("Content-Type", ctx.Request.Header.Get("Content-Type"))

	res, err := client.Do(req)
	if err != nil {
		ctx.String(http.StatusInternalServerError, err.Error())
		return
	}
	defer res.Body.Close()

	bodyBytes, err := io.ReadAll(res.Body)
	if err != nil {
		ctx.String(http.StatusInternalServerError, err.Error())
		return
	}

	ctx.String(http.StatusOK, string(bodyBytes))

}
