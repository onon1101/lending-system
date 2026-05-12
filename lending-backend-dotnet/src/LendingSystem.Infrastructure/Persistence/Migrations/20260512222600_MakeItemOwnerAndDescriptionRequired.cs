using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeItemOwnerAndDescriptionRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_users_owner_id",
                table: "items");

            migrationBuilder.Sql("UPDATE items SET description = '' WHERE description IS NULL;");
            migrationBuilder.Sql("""
                UPDATE items
                SET owner_id = (SELECT user_id FROM users ORDER BY user_id LIMIT 1)
                WHERE owner_id IS NULL AND EXISTS (SELECT 1 FROM users);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "items",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "current_status",
                table: "items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "Available");

            migrationBuilder.AddForeignKey(
                name: "FK_items_users_owner_id",
                table: "items",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_users_owner_id",
                table: "items");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "items",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "items",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "current_status",
                table: "items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Available");

            migrationBuilder.AddForeignKey(
                name: "FK_items_users_owner_id",
                table: "items",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
