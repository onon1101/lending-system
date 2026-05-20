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
            migrationBuilder.DropForeignKey(
                name: "fk_media_item",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "order_details_object_id_fkey",
                table: "order_details");

            migrationBuilder.DropPrimaryKey(
                name: "items_pkey",
                table: "items");

            migrationBuilder.RenameColumn(
                name: "object_id",
                table: "order_details",
                newName: "item_id");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regclass('"IX_order_details_object_id"') IS NOT NULL THEN
                        ALTER INDEX "IX_order_details_object_id" RENAME TO "IX_order_details_item_id";
                    ELSIF to_regclass('"IX_order_details_item_id"') IS NULL THEN
                        CREATE INDEX "IX_order_details_item_id" ON "order_details" ("item_id");
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "object_id",
                table: "items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<int>(
                name: "item_id",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.Sql("""
                UPDATE "items"
                SET "item_id" = "object_id";
                """);

            migrationBuilder.AddPrimaryKey(
                name: "items_pkey",
                table: "items",
                column: "item_id");

            migrationBuilder.CreateTable(
                name: "objects",
                columns: table => new
                {
                    object_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    @object = table.Column<string>(name: "object", type: "character varying(100)", maxLength: 100, nullable: false),
                    club = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    maker = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    material = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("objects_pkey", x => x.object_id);
                });

            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.DropColumn(
                name: "object_name",
                table: "items");

            migrationBuilder.CreateIndex(
                name: "IX_items_object_id",
                table: "items",
                column: "object_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_owner_id",
                table: "items",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "FK_items_users_owner_id",
                table: "items",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "items_object_id_fkey",
                table: "items",
                column: "object_id",
                principalTable: "objects",
                principalColumn: "object_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_media_item",
                table: "media",
                column: "object_id",
                principalTable: "items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_details_item_id_fkey",
                table: "order_details",
                column: "item_id",
                principalTable: "items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_users_owner_id",
                table: "items");

            migrationBuilder.DropForeignKey(
                name: "items_object_id_fkey",
                table: "items");

            migrationBuilder.DropForeignKey(
                name: "fk_media_item",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "order_details_item_id_fkey",
                table: "order_details");

            migrationBuilder.DropTable(
                name: "objects");

            migrationBuilder.DropPrimaryKey(
                name: "items_pkey",
                table: "items");

            migrationBuilder.DropIndex(
                name: "IX_items_object_id",
                table: "items");

            migrationBuilder.DropIndex(
                name: "IX_items_owner_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "item_id",
                table: "items");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "order_details",
                newName: "object_id");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regclass('"IX_order_details_item_id"') IS NOT NULL THEN
                        ALTER INDEX "IX_order_details_item_id" RENAME TO "IX_order_details_object_id";
                    ELSIF to_regclass('"IX_order_details_object_id"') IS NULL THEN
                        CREATE INDEX "IX_order_details_object_id" ON "order_details" ("object_id");
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "object_id",
                table: "items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<string>(
                name: "object_name",
                table: "items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "items_pkey",
                table: "items",
                column: "object_id");

            migrationBuilder.AddForeignKey(
                name: "fk_media_item",
                table: "media",
                column: "object_id",
                principalTable: "items",
                principalColumn: "object_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_details_object_id_fkey",
                table: "order_details",
                column: "object_id",
                principalTable: "items",
                principalColumn: "object_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
