using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MergeObjectIntoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "items_object_id_fkey";
                ALTER TABLE "items" ADD COLUMN "maker" character varying(100) NOT NULL DEFAULT '';
                ALTER TABLE "items" ADD COLUMN "material" character varying(100) NOT NULL DEFAULT '';
                ALTER TABLE "items" ADD COLUMN "object_name" character varying(100) NOT NULL DEFAULT '';

                UPDATE "items" AS i
                SET
                    "object_name" = COALESCE(NULLIF(o."object", ''), 'Unknown'),
                    "maker" = COALESCE(o."maker", ''),
                    "material" = COALESCE(o."material", '')
                FROM "objects" AS o
                WHERE i."object_id" = o."object_id";

                UPDATE "items"
                SET "object_name" = 'Unknown'
                WHERE "object_name" = '';

                DROP TABLE IF EXISTS "objects";
                DROP INDEX IF EXISTS "IX_items_object_id";
                ALTER TABLE "items" DROP COLUMN IF EXISTS "object_id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "objects" (
                    "object_id" serial NOT NULL,
                    "club" character varying(100) NOT NULL,
                    "maker" character varying(100) NOT NULL,
                    "material" character varying(100) NOT NULL,
                    "object" character varying(100) NOT NULL,
                    "price" integer NOT NULL,
                    CONSTRAINT "objects_pkey" PRIMARY KEY ("object_id")
                );

                INSERT INTO "objects" ("object_id", "object", "club", "maker", "material", "price")
                SELECT
                    "item_id",
                    COALESCE(NULLIF("object_name", ''), 'Unknown'),
                    '',
                    COALESCE("maker", ''),
                    COALESCE("material", ''),
                    0
                FROM "items";

                SELECT setval(
                    pg_get_serial_sequence('objects', 'object_id'),
                    COALESCE((SELECT MAX("object_id") FROM "objects"), 1),
                    true
                );

                ALTER TABLE "items" ADD COLUMN "object_id" integer NOT NULL DEFAULT 0;

                UPDATE "items"
                SET "object_id" = "item_id";

                CREATE INDEX "IX_items_object_id" ON "items" ("object_id");

                ALTER TABLE "items"
                ADD CONSTRAINT "items_object_id_fkey"
                FOREIGN KEY ("object_id") REFERENCES "objects" ("object_id") ON DELETE RESTRICT;

                ALTER TABLE "items" DROP COLUMN "maker";
                ALTER TABLE "items" DROP COLUMN "material";
                ALTER TABLE "items" DROP COLUMN "object_name";
                """);
        }
    }
}
