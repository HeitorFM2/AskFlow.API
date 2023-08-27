package service

import (
	"ask-flow/api/models"
	"ask-flow/configs"
	"encoding/json"
	"errors"
	"github.com/golang-jwt/jwt/v4"
	"io"
	"net/http"
	"net/smtp"
	"os"
	"time"

	"github.com/gin-gonic/gin"
)

var questions []models.Questions

func Login(ctx *gin.Context) {
	var user models.Users
	if err := ctx.BindJSON(&user); err != nil {
		return
	}

	if len(user.Password) > 120 || len(user.Email) > 120 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Email or password exceeded 120 characters!"))
		return
	}

	var getUser models.Users
	result := configs.DB.Where("email = ?", &user.Email).Find(&getUser)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	if models.VerifyPassword(getUser.Password, user.Password) != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Invalid user or password!"))
		return
	}

	var token = jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		"exp":   time.Now().Add(time.Hour * 24).Unix(),
		"user":  getUser.ID,
		"email": getUser.Email,
	})

	ctx.Set("UserID", getUser.ID)

	tokenString, err := token.SignedString([]byte(os.Getenv("SECRET")))

	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Failed to create token!"))
		return
	}

	ctx.JSON(http.StatusOK, gin.H{
		"data":  models.ResponseOK(&getUser),
		"token": tokenString,
	})
}

func FindUserPost(ctx *gin.Context) {
	id := ctx.Param("id")

	result := configs.DB.Where("iduser = ?", id).Find(&questions)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(&questions))
}

func FindUser(ctx *gin.Context) {
	id := ctx.Param("id")
	var user models.Users

	result := configs.DB.Find(&user, id)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(&user))
}

func FindDetaisPost(ctx *gin.Context) {
	id := ctx.Param("id")
	var questionReponse []models.QuestionsReponse

	result := configs.DB.Raw(`
		SELECT
			q.id,
			q.created_at,
			q.iduser,
			q.message,
			u.first_name,
			u.last_name,
			u.img,
			q.img_post
		FROM
			questions q
		INNER JOIN
			users u ON u.id = q.iduser
		WHERE
			q.id = ?;`, id).Scan(&questionReponse)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(&questionReponse))
}

func FindResponsesPost(ctx *gin.Context) {
	id := ctx.Param("id")
	var responsesPost []models.ResponsesPost
	result := configs.DB.Raw(`
		SELECT
			r.id,
			r.idquestion,
			r.iduser,
			r.message,
			u.first_name,
			u.last_name,
			u.img,
			r.created_at
		FROM
			responses r
		INNER JOIN
			users u ON u.id = r.iduser
		WHERE
			r.idquestion = ?
			AND r.deleted_at IS NULL
		ORDER BY
			r.id DESC;`, id).Scan(&responsesPost)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(&responsesPost))
}

func FindAll(ctx *gin.Context) {
	var questionReponse []models.QuestionsReponse
	result := configs.DB.Raw(`
		SELECT
			q.id,
			q.iduser,
			q.created_at,
			q.message,
			u.first_name,
			u.last_name,
			u.img,
			q.img_post,
			COALESCE(COUNT(r.idquestion), 0) AS response
		FROM
			questions q
		INNER JOIN users u ON u.id = q.iduser
		LEFT JOIN responses r ON r.idquestion = q.id
		WHERE q.deleted_at IS NULL and r.deleted_at IS NULL
		GROUP BY q.id, q.iduser, q.message, u.first_name, u.last_name, u.img
		ORDER BY q.id DESC;
	`).Scan(&questionReponse)

	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(&questionReponse))
}

func CreateUser(ctx *gin.Context) {
	var user models.Users
	if err := ctx.BindJSON(&user); err != nil {
		ctx.JSON(http.StatusBadRequest, models.ResponseBadRequest("Invalid JSON data"))
		return
	}

	if err := validateUserFields(&user); err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError(err.Error()))
		return
	}

	var getUser models.Users
	results := configs.DB.Where("email = ?", &user.Email).Find(&getUser)

	if results.RowsAffected > 0 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Email already registered!"))
		return
	}
	hashedPassword := models.HashPassword(user.Password)

	user.Password = hashedPassword

	result := configs.DB.Create(&user)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Sorry, there was an error - try again later!"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func CreatePost(ctx *gin.Context) {
	var postCreate models.Questions

	if err := ctx.BindJSON(&postCreate); err != nil {
		return
	}

	if len(postCreate.Message) > 300 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Message exceeded 300 characters!"))
		return
	}
	result := configs.DB.Create(&postCreate)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func CreateResponse(ctx *gin.Context) {

	var res models.Responses

	if err := ctx.BindJSON(&res); err != nil {
		return
	}

	if len(res.Message) > 300 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Message exceeded 300 characters!"))
		return
	}

	result := configs.DB.Create(&res)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func EditEmail(ctx *gin.Context) {
	id := ctx.Param("id")

	type user struct {
		Email string `json:"email"`
	}

	var users user
	if err := ctx.BindJSON(&users); err != nil {
		return
	}

	if len(users.Email) > 120 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Email exceeded 120 characters!"))
		return
	}

	result := configs.DB.Model(&users).Where("id = ?", id).Updates(&users)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func EditUsername(ctx *gin.Context) {
	id := ctx.Param("id")

	type user struct {
		FirstName string `json:"first_name"`
		LastName  string `json:"last_name"`
	}

	var users user
	if err := ctx.BindJSON(&users); err != nil {
		return
	}

	if len(users.FirstName) > 120 || len(users.LastName) > 120 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("First name or last name exceeded 120 characters!"))
		return
	}

	result := configs.DB.Model(&users).Where("id = ?", id).Updates(&users)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func EditImg(ctx *gin.Context) {
	id := ctx.Param("id")

	imgString := Upload(ctx)

	type user struct {
		Img string `json:"img"`
	}

	var users user
	users.Img = imgString

	result := configs.DB.Model(&users).Where("id = ?", id).Updates(&users)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func DeleteResponse(ctx *gin.Context) {
	id := ctx.Param("id")
	var res models.Responses

	result := configs.DB.Delete(&res, &id)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func DeletePost(ctx *gin.Context) {
	id := ctx.Param("id")

	var res models.Questions

	result := configs.DB.Delete(&res, id)
	if result.Error != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func Upload(ctx *gin.Context) string {
	url := "https://api.imgur.com/3/image"
	method := "POST"

	body := ctx.Request.Body

	client := &http.Client{}
	req, err := http.NewRequest(method, url, body)
	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error upload image"))
		return ""
	}

	req.Header.Add("Authorization", "Client-ID 5f24e4cd127e1ac")
	req.Header.Set("Content-Type", ctx.Request.Header.Get("Content-Type"))

	res, err := client.Do(req)
	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error upload image"))
		return ""
	}
	defer res.Body.Close()

	bodyBytes, err := io.ReadAll(res.Body)
	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error upload image"))
		return ""
	}

	var response map[string]interface{}
	err = json.Unmarshal(bodyBytes, &response)

	data := response["data"].(map[string]interface{})
	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error upload image"))
		return ""
	}

	return data["link"].(string)

}

func UploadHandler(ctx *gin.Context) {
	imgString := Upload(ctx)

	ctx.JSON(http.StatusOK, imgString)
}

func SendMailSimple(ctx *gin.Context) {

	var emailPost models.Email

	if err := ctx.BindJSON(&emailPost); err != nil {
		return
	}

	if len(emailPost.Email) > 120 || len(emailPost.Name) > 120 || len(emailPost.Message) > 600 {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Fields exceeded 120 characters!"))
		return
	}

	auth := smtp.PlainAuth(
		"",
		"heitorfm.dev@gmail.com",
		os.Getenv("SECRET_EMAIL"),
		"smtp.gmail.com",
	)

	msg := "Subject: " + emailPost.Name + "\nEmail " + emailPost.Email + "\nMessage: " + emailPost.Message

	err := smtp.SendMail(
		"smtp.gmail.com:587",
		auth,
		"heitorfm.dev@gmail.com",
		[]string{"heitorfm.dev@gmail.com"},
		[]byte(msg),
	)

	if err != nil {
		ctx.JSON(http.StatusInternalServerError, models.ResponseError("Error send email"))
		return
	}

	ctx.JSON(http.StatusOK, models.ResponseOK(nil))
}

func validateUserFields(user *models.Users) error {
	if user.Password == "" || user.Email == "" || user.First_name == "" || user.Last_name == "" {
		return errors.New("Fill in all the fields!")
	}

	if len(user.Password) > 120 || len(user.Email) > 120 || len(user.First_name) > 120 || len(user.Last_name) > 120 {
		return errors.New("Fields exceeded 120 characters!")
	}

	return nil
}
