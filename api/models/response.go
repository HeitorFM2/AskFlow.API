package models

import "net/http"

type response struct {
	StatusCode int         `json:"code"`
	Success    bool        `json:"success"`
	Message    string      `json:"message"`
	Data       interface{} `json:"data"`
}

func ResponseOK(data interface{}) interface{} {
	var response response
	response.StatusCode = http.StatusOK
	response.Data = data
	response.Success = true

	return &response
}

func ResponseError(message string) interface{} {
	var response response
	response.StatusCode = http.StatusInternalServerError
	response.Success = true
	response.Message = message

	return &response
}

func ResponseBadRequest(message string) interface{} {
	var response response
	response.StatusCode = http.StatusBadRequest
	response.Success = false
	response.Message = message

	return &response
}
