package model

type Media struct {
	MediaID     int    `json:"media_id"`
	OrderID     *int   `json:"order_id,omitempty"`
	ObjectID    int    `json:"object_id"`
	Type        string `json:"type"`
	Description string `json:"description"`
	URL         string `json:"url"`
	Link        string `jons:"link"`
	CreatedAt   string `json:"created_at"`
}

type CreateMediaRequest struct {
	OrderID     *int   `json:"order_id,omitempty"`
	ObjectID    int    `json:"object_id"`
	URL         string `json:"url"`
	Type        string `json:"type"`
	Link string `json:"link"`
	Description string `json:"description"`
}
