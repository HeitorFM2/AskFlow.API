package models

import (
	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

type Response struct {
	StatusCode int         `json:"code"`
	Success    bool        `json:"success"`
	Message    string      `json:"message"`
	Data       interface{} `json:"data"`
}

type Users struct {
	gorm.Model
	First_name string `json:"first_name"`
	Last_name  string `json:"last_name"`
	Email      string `json:"email"`
	Img        string `json:"img"`
	Password   string `json:"password"`
}

type Questions struct {
	gorm.Model
	Iduser  int    `json:"iduser"`
	Message string `json:"message"`
	ImgPost string `json:"imgpost"`
}

type QuestionsReponse struct {
	gorm.Model
	Iduser     int    `json:"iduser"`
	Message    string `json:"message"`
	First_name string `json:"first_name"`
	Last_name  string `json:"last_name"`
	Response   string `json:"response"`
	Img        string `json:"img"`
	ImgPost    string `json:"imgpost"`
}

type Responses struct {
	gorm.Model
	Idquestion int    `json:"idquestion"`
	Iduser     int    `json:"iduser"`
	Message    string `json:"message"`
}

type ResponsesPost struct {
	gorm.Model
	Idquestion int    `json:"idquestion"`
	Iduser     int    `json:"iduser"`
	Message    string `json:"message"`
	First_name string `json:"first_name"`
	Last_name  string `json:"last_name"`
	Img        string `json:"img"`
	ImgPost    string `json:"imgpost"`
}

func HashPassword(password string) string {
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		return ""
	}
	return string(hashedPassword)
}

func VerifyPassword(hashedPassword, password string) error {
	return bcrypt.CompareHashAndPassword([]byte(hashedPassword), []byte(password))
}
