using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class remove_nullable_from_Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "user",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "text",
                nullable: true,
                defaultValue: "user",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "user");
        }
    }
}
