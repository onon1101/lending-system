using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MergeObjectIntoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "items_object_id_fkey",
                table: "items");

            migrationBuilder.AddColumn<string>(
                name: "maker",
                table: "items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "material",
                table: "items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "object_name",
                table: "items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.DropTable(
                name: "objects");

            migrationBuilder.DropIndex(
                name: "IX_items_object_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "object_id",
                table: "items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "objects",
                columns: table => new
                {
                    object_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    club = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    maker = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    material = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    @object = table.Column<string>(name: "object", type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("objects_pkey", x => x.object_id);
                });

            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.AddColumn<int>(
                name: "object_id",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "items"
                SET "object_id" = "item_id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_items_object_id",
                table: "items",
                column: "object_id");

            migrationBuilder.AddForeignKey(
                name: "items_object_id_fkey",
                table: "items",
                column: "object_id",
                principalTable: "objects",
                principalColumn: "object_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "maker",
                table: "items");

            migrationBuilder.DropColumn(
                name: "material",
                table: "items");

            migrationBuilder.DropColumn(
                name: "object_name",
                table: "items");
        }
    }
}
