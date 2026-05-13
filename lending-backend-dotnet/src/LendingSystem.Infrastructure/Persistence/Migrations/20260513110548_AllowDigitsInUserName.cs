using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowDigitsInUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users",
                sql: "name ~ '^[A-Za-z0-9]+$'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users",
                sql: "name ~ '^[A-Za-z]+$'");
        }
    }
}
