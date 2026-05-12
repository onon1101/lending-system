using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class remove_nickname_from_user_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nickname",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nickname",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
