using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
CREATE TABLE "items" (
    "object_id" serial NOT NULL,
    "object_name" character varying(100) NOT NULL,
    "description" text NULL,
    "current_status" character varying(50) NULL DEFAULT 'Available',
    "owner_id" integer NULL,
    "image_url" character varying(300) NULL,
    CONSTRAINT "items_pkey" PRIMARY KEY ("object_id")
);

CREATE TABLE "users" (
    "user_id" serial NOT NULL,
    "name" character varying(100) NOT NULL,
    "email" character varying(100) NULL,
    "password_hash" character varying(255) NULL,
    "is_deleted" boolean NOT NULL DEFAULT false,
    "nickname" text NULL,
    "role" text NULL DEFAULT 'user',
    "created_at" timestamp with time zone NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "users_pkey" PRIMARY KEY ("user_id")
);

CREATE TABLE "orders" (
    "order_id" serial NOT NULL,
    "user_id" integer NOT NULL,
    "start_time" timestamp with time zone NOT NULL,
    "end_time" timestamp with time zone NOT NULL,
    "status" character varying(50) NOT NULL,
    CONSTRAINT "orders_pkey" PRIMARY KEY ("order_id"),
    CONSTRAINT "orders_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "users" ("user_id")
);

CREATE TABLE "media" (
    "media_id" serial NOT NULL,
    "order_id" integer NULL,
    "object_id" integer NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text NULL,
    "description" text NULL,
    "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "media_pkey" PRIMARY KEY ("media_id"),
    CONSTRAINT "fk_media_item" FOREIGN KEY ("object_id") REFERENCES "items" ("object_id") ON DELETE SET NULL,
    CONSTRAINT "fk_media_order" FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE
);

CREATE TABLE "order_details" (
    "order_detail_id" serial NOT NULL,
    "order_id" integer NOT NULL,
    "object_id" integer NOT NULL,
    "detail_status" character varying(50) NOT NULL,
    "actual_return_time" timestamp with time zone NULL,
    CONSTRAINT "order_details_pkey" PRIMARY KEY ("order_detail_id"),
    CONSTRAINT "order_details_object_id_fkey" FOREIGN KEY ("object_id") REFERENCES "items" ("object_id") ON DELETE RESTRICT,
    CONSTRAINT "order_details_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE
);

CREATE INDEX "idx_media_order_id" ON "media" ("order_id");
CREATE INDEX "IX_media_object_id" ON "media" ("object_id");
CREATE INDEX "IX_order_details_object_id" ON "order_details" ("object_id");
CREATE INDEX "IX_order_details_order_id" ON "order_details" ("order_id");
CREATE INDEX "IX_orders_user_id" ON "orders" ("user_id");
CREATE UNIQUE INDEX "users_email_key" ON "users" ("email");
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DROP TABLE IF EXISTS "media";
DROP TABLE IF EXISTS "order_details";
DROP TABLE IF EXISTS "items";
DROP TABLE IF EXISTS "orders";
DROP TABLE IF EXISTS "users";
""");
        }
    }
}
