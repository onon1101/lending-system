package model
import (
	"database/sql"
)

type DBClient interface {
	Ping() error
}

type RealDB struct {
	*sql.DB
}

