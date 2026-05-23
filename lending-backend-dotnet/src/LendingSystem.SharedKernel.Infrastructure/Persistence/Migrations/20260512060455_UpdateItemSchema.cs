using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "media" DROP CONSTRAINT IF EXISTS "fk_media_item";
                ALTER TABLE "order_details" DROP CONSTRAINT IF EXISTS "order_details_object_id_fkey";
                ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "items_pkey";
                ALTER TABLE "order_details" RENAME COLUMN "object_id" TO "item_id";

                DO $$
                BEGIN
                    IF to_regclass('"IX_order_details_object_id"') IS NOT NULL THEN
                        ALTER INDEX "IX_order_details_object_id" RENAME TO "IX_order_details_item_id";
                    ELSIF to_regclass('"IX_order_details_item_id"') IS NULL THEN
                        CREATE INDEX "IX_order_details_item_id" ON "order_details" ("item_id");
                    END IF;
                END $$;

                ALTER TABLE "items" ALTER COLUMN "object_id" DROP DEFAULT;
                ALTER TABLE "items" ADD COLUMN "item_id" serial NOT NULL;

                UPDATE "items"
                SET "item_id" = "object_id";

                ALTER TABLE "items" ADD CONSTRAINT "items_pkey" PRIMARY KEY ("item_id");

                CREATE TABLE "objects" (
                    "object_id" serial NOT NULL,
                    "object" character varying(100) NOT NULL,
                    "club" character varying(100) NOT NULL,
                    "maker" character varying(100) NOT NULL,
                    "material" character varying(100) NOT NULL,
                    "price" integer NOT NULL,
                    CONSTRAINT "objects_pkey" PRIMARY KEY ("object_id")
                );

                INSERT INTO "objects" ("object_id", "object", "club", "maker", "material", "price")
                SELECT DISTINCT
                    "object_id",
                    COALESCE(NULLIF("object_name", ''), 'Unknown'),
                    '',
                    '',
                    '',
                    0
                FROM "items";

                SELECT setval(
                    pg_get_serial_sequence('objects', 'object_id'),
                    COALESCE((SELECT MAX("object_id") FROM "objects"), 1),
                    true
                );

                ALTER TABLE "items" DROP COLUMN "object_name";
                CREATE INDEX "IX_items_object_id" ON "items" ("object_id");
                CREATE INDEX "IX_items_owner_id" ON "items" ("owner_id");

                ALTER TABLE "items"
                ADD CONSTRAINT "FK_items_users_owner_id"
                FOREIGN KEY ("owner_id") REFERENCES "users" ("user_id") ON DELETE SET NULL;

                ALTER TABLE "items"
                ADD CONSTRAINT "items_object_id_fkey"
                FOREIGN KEY ("object_id") REFERENCES "objects" ("object_id") ON DELETE RESTRICT;

                ALTER TABLE "media"
                ADD CONSTRAINT "fk_media_item"
                FOREIGN KEY ("object_id") REFERENCES "items" ("item_id") ON DELETE SET NULL;

                ALTER TABLE "order_details"
                ADD CONSTRAINT "order_details_item_id_fkey"
                FOREIGN KEY ("item_id") REFERENCES "items" ("item_id") ON DELETE RESTRICT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "FK_items_users_owner_id";
                ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "items_object_id_fkey";
                ALTER TABLE "media" DROP CONSTRAINT IF EXISTS "fk_media_item";
                ALTER TABLE "order_details" DROP CONSTRAINT IF EXISTS "order_details_item_id_fkey";
                DROP TABLE IF EXISTS "objects";
                ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "items_pkey";
                DROP INDEX IF EXISTS "IX_items_object_id";
                DROP INDEX IF EXISTS "IX_items_owner_id";
                ALTER TABLE "items" DROP COLUMN IF EXISTS "item_id";
                ALTER TABLE "order_details" RENAME COLUMN "item_id" TO "object_id";

                DO $$
                BEGIN
                    IF to_regclass('"IX_order_details_item_id"') IS NOT NULL THEN
                        ALTER INDEX "IX_order_details_item_id" RENAME TO "IX_order_details_object_id";
                    ELSIF to_regclass('"IX_order_details_object_id"') IS NULL THEN
                        CREATE INDEX "IX_order_details_object_id" ON "order_details" ("object_id");
                    END IF;
                END $$;

                DO $$
                DECLARE
                    seq_name text;
                BEGIN
                    seq_name := pg_get_serial_sequence('"items"', 'object_id');
                    IF seq_name IS NOT NULL THEN
                        EXECUTE format('ALTER TABLE "items" ALTER COLUMN "object_id" SET DEFAULT nextval(%L)', seq_name);
                    END IF;
                END $$;

                ALTER TABLE "items" ADD COLUMN "object_name" character varying(100) NOT NULL DEFAULT '';
                ALTER TABLE "items" ADD CONSTRAINT "items_pkey" PRIMARY KEY ("object_id");

                ALTER TABLE "media"
                ADD CONSTRAINT "fk_media_item"
                FOREIGN KEY ("object_id") REFERENCES "items" ("object_id") ON DELETE SET NULL;

                ALTER TABLE "order_details"
                ADD CONSTRAINT "order_details_object_id_fkey"
                FOREIGN KEY ("object_id") REFERENCES "items" ("object_id") ON DELETE RESTRICT;
                """);
        }
    }
}
