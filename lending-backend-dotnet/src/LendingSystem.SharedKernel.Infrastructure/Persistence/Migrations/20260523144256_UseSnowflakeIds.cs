using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseSnowflakeIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DROP SEQUENCE IF EXISTS "users_user_id_seq" CASCADE;
DROP SEQUENCE IF EXISTS "items_item_id_seq" CASCADE;
DROP SEQUENCE IF EXISTS "orders_order_id_seq" CASCADE;
DROP SEQUENCE IF EXISTS "borrower_details_borrower_detail_id_seq" CASCADE;
DROP SEQUENCE IF EXISTS "item_media_media_id_seq" CASCADE;
DROP SEQUENCE IF EXISTS "lending_media_media_id_seq" CASCADE;

ALTER TABLE "item_media" DROP CONSTRAINT IF EXISTS "fk_item_media_item";
ALTER TABLE "lending_media" DROP CONSTRAINT IF EXISTS "fk_lending_media_order";
ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "orders_borrower_detail_id_fkey";
ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "orders_item_id_fkey";
ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "FK_items_users_owner_id";
ALTER TABLE "borrower_details" DROP CONSTRAINT IF EXISTS "borrower_details_user_id_fkey";

ALTER TABLE "users" ALTER COLUMN "user_id" TYPE bigint;
ALTER TABLE "items" ALTER COLUMN "item_id" TYPE bigint;
ALTER TABLE "items" ALTER COLUMN "owner_id" TYPE bigint;
ALTER TABLE "borrower_details" ALTER COLUMN "borrower_detail_id" TYPE bigint;
ALTER TABLE "borrower_details" ALTER COLUMN "user_id" TYPE bigint;
ALTER TABLE "orders" ALTER COLUMN "order_id" TYPE bigint;
ALTER TABLE "orders" ALTER COLUMN "borrower_detail_id" TYPE bigint;
ALTER TABLE "orders" ALTER COLUMN "item_id" TYPE bigint;
ALTER TABLE "item_media" ALTER COLUMN "media_id" TYPE bigint;
ALTER TABLE "item_media" ALTER COLUMN "item_id" TYPE bigint;
ALTER TABLE "lending_media" ALTER COLUMN "media_id" TYPE bigint;
ALTER TABLE "lending_media" ALTER COLUMN "order_id" TYPE bigint;

ALTER TABLE "borrower_details"
    ADD CONSTRAINT "borrower_details_user_id_fkey"
    FOREIGN KEY ("user_id") REFERENCES "users" ("user_id")
    ON DELETE NO ACTION;

ALTER TABLE "items"
    ADD CONSTRAINT "FK_items_users_owner_id"
    FOREIGN KEY ("owner_id") REFERENCES "users" ("user_id")
    ON DELETE RESTRICT;

ALTER TABLE "orders"
    ADD CONSTRAINT "orders_borrower_detail_id_fkey"
    FOREIGN KEY ("borrower_detail_id") REFERENCES "borrower_details" ("borrower_detail_id")
    ON DELETE NO ACTION;

ALTER TABLE "orders"
    ADD CONSTRAINT "orders_item_id_fkey"
    FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
    ON DELETE RESTRICT;

ALTER TABLE "item_media"
    ADD CONSTRAINT "fk_item_media_item"
    FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
    ON DELETE CASCADE;

ALTER TABLE "lending_media"
    ADD CONSTRAINT "fk_lending_media_order"
    FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id")
    ON DELETE CASCADE;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "item_media" DROP CONSTRAINT IF EXISTS "fk_item_media_item";
ALTER TABLE "lending_media" DROP CONSTRAINT IF EXISTS "fk_lending_media_order";
ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "orders_borrower_detail_id_fkey";
ALTER TABLE "orders" DROP CONSTRAINT IF EXISTS "orders_item_id_fkey";
ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "FK_items_users_owner_id";
ALTER TABLE "borrower_details" DROP CONSTRAINT IF EXISTS "borrower_details_user_id_fkey";

ALTER TABLE "users" ALTER COLUMN "user_id" TYPE integer;
ALTER TABLE "items" ALTER COLUMN "item_id" TYPE integer;
ALTER TABLE "items" ALTER COLUMN "owner_id" TYPE integer;
ALTER TABLE "borrower_details" ALTER COLUMN "borrower_detail_id" TYPE integer;
ALTER TABLE "borrower_details" ALTER COLUMN "user_id" TYPE integer;
ALTER TABLE "orders" ALTER COLUMN "order_id" TYPE integer;
ALTER TABLE "orders" ALTER COLUMN "borrower_detail_id" TYPE integer;
ALTER TABLE "orders" ALTER COLUMN "item_id" TYPE integer;
ALTER TABLE "item_media" ALTER COLUMN "media_id" TYPE integer;
ALTER TABLE "item_media" ALTER COLUMN "item_id" TYPE integer;
ALTER TABLE "lending_media" ALTER COLUMN "media_id" TYPE integer;
ALTER TABLE "lending_media" ALTER COLUMN "order_id" TYPE integer;

CREATE SEQUENCE IF NOT EXISTS "users_user_id_seq" AS integer OWNED BY "users"."user_id";
CREATE SEQUENCE IF NOT EXISTS "items_item_id_seq" AS integer OWNED BY "items"."item_id";
CREATE SEQUENCE IF NOT EXISTS "orders_order_id_seq" AS integer OWNED BY "orders"."order_id";
CREATE SEQUENCE IF NOT EXISTS "borrower_details_borrower_detail_id_seq" AS integer OWNED BY "borrower_details"."borrower_detail_id";
CREATE SEQUENCE IF NOT EXISTS "item_media_media_id_seq" AS integer OWNED BY "item_media"."media_id";
CREATE SEQUENCE IF NOT EXISTS "lending_media_media_id_seq" AS integer OWNED BY "lending_media"."media_id";

SELECT setval('"users_user_id_seq"', COALESCE((SELECT max("user_id") FROM "users"), 0) + 1, false);
SELECT setval('"items_item_id_seq"', COALESCE((SELECT max("item_id") FROM "items"), 0) + 1, false);
SELECT setval('"orders_order_id_seq"', COALESCE((SELECT max("order_id") FROM "orders"), 0) + 1, false);
SELECT setval('"borrower_details_borrower_detail_id_seq"', COALESCE((SELECT max("borrower_detail_id") FROM "borrower_details"), 0) + 1, false);
SELECT setval('"item_media_media_id_seq"', COALESCE((SELECT max("media_id") FROM "item_media"), 0) + 1, false);
SELECT setval('"lending_media_media_id_seq"', COALESCE((SELECT max("media_id") FROM "lending_media"), 0) + 1, false);

ALTER TABLE "users" ALTER COLUMN "user_id" SET DEFAULT nextval('"users_user_id_seq"');
ALTER TABLE "items" ALTER COLUMN "item_id" SET DEFAULT nextval('"items_item_id_seq"');
ALTER TABLE "orders" ALTER COLUMN "order_id" SET DEFAULT nextval('"orders_order_id_seq"');
ALTER TABLE "borrower_details" ALTER COLUMN "borrower_detail_id" SET DEFAULT nextval('"borrower_details_borrower_detail_id_seq"');
ALTER TABLE "item_media" ALTER COLUMN "media_id" SET DEFAULT nextval('"item_media_media_id_seq"');
ALTER TABLE "lending_media" ALTER COLUMN "media_id" SET DEFAULT nextval('"lending_media_media_id_seq"');

ALTER TABLE "borrower_details"
    ADD CONSTRAINT "borrower_details_user_id_fkey"
    FOREIGN KEY ("user_id") REFERENCES "users" ("user_id")
    ON DELETE NO ACTION;

ALTER TABLE "items"
    ADD CONSTRAINT "FK_items_users_owner_id"
    FOREIGN KEY ("owner_id") REFERENCES "users" ("user_id")
    ON DELETE RESTRICT;

ALTER TABLE "orders"
    ADD CONSTRAINT "orders_borrower_detail_id_fkey"
    FOREIGN KEY ("borrower_detail_id") REFERENCES "borrower_details" ("borrower_detail_id")
    ON DELETE NO ACTION;

ALTER TABLE "orders"
    ADD CONSTRAINT "orders_item_id_fkey"
    FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
    ON DELETE RESTRICT;

ALTER TABLE "item_media"
    ADD CONSTRAINT "fk_item_media_item"
    FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
    ON DELETE CASCADE;

ALTER TABLE "lending_media"
    ADD CONSTRAINT "fk_lending_media_order"
    FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id")
    ON DELETE CASCADE;
""");
        }
    }
}
