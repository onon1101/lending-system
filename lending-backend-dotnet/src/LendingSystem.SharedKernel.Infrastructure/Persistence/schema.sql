CREATE TABLE IF NOT EXISTS "users" (
    "user_id" bigint NOT NULL,
    "name" character varying(100) NOT NULL,
    "status" character varying(50) NOT NULL DEFAULT 'ACTIVE',
    "role" text NOT NULL DEFAULT 'user',
    "created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "users_pkey" PRIMARY KEY ("user_id"),
    CONSTRAINT "ck_users_name_english_letters" CHECK ("name" ~ '^[A-Za-z0-9]+$')
);

CREATE UNIQUE INDEX IF NOT EXISTS "users_name_key"
    ON "users" ("name");

CREATE TABLE IF NOT EXISTS "user_auth_identities" (
    "id" bigint NOT NULL,
    "user_id" bigint NOT NULL,
    "type" character varying(50) NOT NULL,
    "identifier" character varying(255) NOT NULL,
    "metadata_json" jsonb NOT NULL,
    "created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "user_auth_identities_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "user_auth_identities_user_id_fkey"
        FOREIGN KEY ("user_id")
        REFERENCES "users" ("user_id")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "idx_user_auth_identities_user_id"
    ON "user_auth_identities" ("user_id");

CREATE UNIQUE INDEX IF NOT EXISTS "user_auth_identities_type_identifier_key"
    ON "user_auth_identities" ("type", "identifier");

CREATE TABLE IF NOT EXISTS "items" (
    "item_id" bigint NOT NULL,
    "owner_id" bigint NOT NULL,
    "object_name" character varying(100) NOT NULL,
    "maker" character varying(100) NOT NULL,
    "material" character varying(100) NOT NULL,
    "current_status" character varying(50) NOT NULL DEFAULT 'Available',
    "image_url" character varying(300),
    "description" text NOT NULL,
    CONSTRAINT "items_pkey" PRIMARY KEY ("item_id"),
    CONSTRAINT "FK_items_users_owner_id"
        FOREIGN KEY ("owner_id")
        REFERENCES "users" ("user_id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_items_owner_id"
    ON "items" ("owner_id");

CREATE TABLE IF NOT EXISTS "borrower_details" (
    "borrower_detail_id" bigint NOT NULL,
    "user_id" bigint,
    "borrower_name" character varying(100) NOT NULL,
    "link" text NOT NULL DEFAULT '',
    "created_by" character varying(100) NOT NULL DEFAULT '',
    "created_at" date NOT NULL,
    "updated_by" character varying(100) NOT NULL DEFAULT '',
    "updated_at" date NOT NULL,
    CONSTRAINT "borrower_details_pkey" PRIMARY KEY ("borrower_detail_id"),
    CONSTRAINT "borrower_details_user_id_fkey"
        FOREIGN KEY ("user_id")
        REFERENCES "users" ("user_id")
        ON DELETE NO ACTION
);

CREATE INDEX IF NOT EXISTS "IX_borrower_details_user_id"
    ON "borrower_details" ("user_id");

CREATE TABLE IF NOT EXISTS "orders" (
    "order_id" bigint NOT NULL,
    "borrower_detail_id" bigint NOT NULL,
    "item_id" bigint NOT NULL,
    "start_date" date NOT NULL,
    "end_date" date NOT NULL,
    "actual_return_date" date,
    "status" character varying(50) NOT NULL,
    CONSTRAINT "orders_pkey" PRIMARY KEY ("order_id"),
    CONSTRAINT "orders_borrower_detail_id_fkey"
        FOREIGN KEY ("borrower_detail_id")
        REFERENCES "borrower_details" ("borrower_detail_id")
        ON DELETE NO ACTION,
    CONSTRAINT "orders_item_id_fkey"
        FOREIGN KEY ("item_id")
        REFERENCES "items" ("item_id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_orders_borrower_detail_id"
    ON "orders" ("borrower_detail_id");

CREATE INDEX IF NOT EXISTS "IX_orders_item_id"
    ON "orders" ("item_id");

CREATE TABLE IF NOT EXISTS "item_media" (
    "media_id" bigint NOT NULL,
    "item_id" bigint NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text,
    "description" text,
    "created_at" timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "item_media_pkey" PRIMARY KEY ("media_id"),
    CONSTRAINT "fk_item_media_item"
        FOREIGN KEY ("item_id")
        REFERENCES "items" ("item_id")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "idx_item_media_item_id"
    ON "item_media" ("item_id");

CREATE TABLE IF NOT EXISTS "lending_media" (
    "media_id" bigint NOT NULL,
    "order_id" bigint NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text,
    "description" text,
    "created_at" timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "lending_media_pkey" PRIMARY KEY ("media_id"),
    CONSTRAINT "fk_lending_media_order"
        FOREIGN KEY ("order_id")
        REFERENCES "orders" ("order_id")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "idx_lending_media_order_id"
    ON "lending_media" ("order_id");
