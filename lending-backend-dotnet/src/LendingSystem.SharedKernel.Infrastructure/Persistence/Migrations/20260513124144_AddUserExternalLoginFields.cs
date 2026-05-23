using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExternalLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "auth_provider" character varying(50) NOT NULL DEFAULT 'local';
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "provider_user_id" character varying(255) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "users_auth_provider_provider_user_id_key"
ON "users" ("auth_provider", "provider_user_id");
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DROP INDEX IF EXISTS "users_auth_provider_provider_user_id_key";
ALTER TABLE "users" DROP COLUMN IF EXISTS "auth_provider";
ALTER TABLE "users" DROP COLUMN IF EXISTS "provider_user_id";
""");
        }
    }
}
