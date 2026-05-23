using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeItemOwnerAndDescriptionRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "FK_items_users_owner_id";

UPDATE "items" SET "description" = '' WHERE "description" IS NULL;
UPDATE "items"
SET "owner_id" = (SELECT "user_id" FROM "users" ORDER BY "user_id" LIMIT 1)
WHERE "owner_id" IS NULL AND EXISTS (SELECT 1 FROM "users");
UPDATE "items" SET "current_status" = 'Available' WHERE "current_status" IS NULL;

ALTER TABLE "items" ALTER COLUMN "owner_id" SET NOT NULL;
ALTER TABLE "items" ALTER COLUMN "description" SET DEFAULT '';
ALTER TABLE "items" ALTER COLUMN "description" SET NOT NULL;
ALTER TABLE "items" ALTER COLUMN "current_status" SET DEFAULT 'Available';
ALTER TABLE "items" ALTER COLUMN "current_status" SET NOT NULL;

ALTER TABLE "items"
ADD CONSTRAINT "FK_items_users_owner_id"
FOREIGN KEY ("owner_id") REFERENCES "users" ("user_id") ON DELETE RESTRICT;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "items" DROP CONSTRAINT IF EXISTS "FK_items_users_owner_id";

ALTER TABLE "items" ALTER COLUMN "owner_id" DROP NOT NULL;
ALTER TABLE "items" ALTER COLUMN "description" DROP DEFAULT;
ALTER TABLE "items" ALTER COLUMN "description" DROP NOT NULL;
ALTER TABLE "items" ALTER COLUMN "current_status" SET DEFAULT 'Available';
ALTER TABLE "items" ALTER COLUMN "current_status" DROP NOT NULL;

ALTER TABLE "items"
ADD CONSTRAINT "FK_items_users_owner_id"
FOREIGN KEY ("owner_id") REFERENCES "users" ("user_id") ON DELETE SET NULL;
""");
        }
    }
}
