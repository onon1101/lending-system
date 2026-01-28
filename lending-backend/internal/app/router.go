package app

import (
	"net/http"
	"object-borrow-system/internal/api"

	"github.com/gorilla/mux"
	"github.com/rs/cors"
	httpSwagger "github.com/swaggo/http-swagger"
)

func NewRouter(
	   system *api.APIHandler,
    users *api.UserHandler,
    loans *api.LoanHandler,
    items *api.ItemHandler,
) http.Handler{
 	r := mux.NewRouter()

    // System
    r.HandleFunc("/api/health", system.HealthCheck).Methods("GET")
    r.HandleFunc("/api/status", system.GetSystemStatus).Methods("GET")

    // Users
    r.HandleFunc("/api/users", users.CreateUser).Methods("POST")
    r.HandleFunc("/api/users/{user_id:[0-9]+}", users.GetUserByID).Methods("GET")
    r.HandleFunc("/api/users/{username:[^/]+}", users.GetUserByName).Methods("GET")
    r.HandleFunc("/api/users/{user_id}/loans", loans.GetUserActiveLoans).Methods("GET")

    // Items
    r.HandleFunc("/api/items", items.GetAllItems).Methods("GET")
    r.HandleFunc("/api/items", items.CreateItem).Methods("POST")
    r.HandleFunc("/api/items/{object_id}", items.GetItemByID).Methods("GET")
    r.HandleFunc("/api/items/{object_id}", items.UpdateItem).Methods("PUT")
    r.HandleFunc("/api/items/{object_id}/image", items.UploadItemImage).Methods("POST")

    // Swagger
    r.PathPrefix("/swagger/").Handler(httpSwagger.WrapHandler)

    return cors.AllowAll().Handler(r)
}