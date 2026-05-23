using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowDigitsInUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" DROP CONSTRAINT IF EXISTS "ck_users_name_english_letters";
ALTER TABLE "users" ADD CONSTRAINT "ck_users_name_english_letters" CHECK ("name" ~ '^[A-Za-z0-9]+$');
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" DROP CONSTRAINT IF EXISTS "ck_users_name_english_letters";
ALTER TABLE "users" ADD CONSTRAINT "ck_users_name_english_letters" CHECK ("name" ~ '^[A-Za-z]+$');
""");
        }
    }
}
