package model

type Item struct {
	ObjectID      int    `json:"object_id"`
	ObjectName    string `json:"object_name"`
	Description   string `json:"description"`
	CurrentStatus string `json:"current_status"`
	ImageURL      string `json:"image_url,omitempty"`
}

type CreateItemRequest struct {
	ObjectName  string `json:"object_name"`
	Description string `json:"description"`
}

type UpdateItemRequest struct {
	ObjectName    string `json:"object_name"`
	Description   string `json:"description,omitempty"`
	CurrentStatus string `json:"current_status,omitempty"`
	ImageURL      string `json:"image_url,omitempty"`
}

type GetAllItemsResponse struct {
	ObjectID      int    `json:"object_id"`
	ObjectName    string `json:"object_name"`
	Description   string `json:"description"`
	CurrentStatus string `json:"current_status"`
	OwnerName     string `json:"owner_name"`
	OwnerEmail    string `json:"owner_email"`
	ImageURL      string `json:"image_url,omitempty"`
}

type GetItemMediaByItemID struct {
	Type         string `json:"type"`
	Creator      *string `json:"name"`
	Description  string `json:"description"`
	OriginalLink string `json:"link"`
	Media        string `json:"url"`
	CreatedAt    string `json:"created_at"`
}
