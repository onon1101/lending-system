using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LendingDbContext))]
    [Migration("20260512130000_AddUserDisplayNameAndUsername")]
    public partial class AddUserDisplayNameAndUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "users"
                SET "display_name" = COALESCE(NULLIF("name", ''), NULLIF("email", ''), 'User');
                """);

            migrationBuilder.Sql("""
                WITH RECURSIVE encoded AS (
                    SELECT
                        "user_id",
                        "user_id" AS n,
                        ''::text AS suffix
                    FROM "users"

                    UNION ALL

                    SELECT
                        "user_id",
                        (n - 1) / 26,
                        chr(97 + ((n - 1) % 26)) || suffix
                    FROM encoded
                    WHERE n > 0
                ),
                suffixes AS (
                    SELECT "user_id", suffix
                    FROM encoded
                    WHERE n = 0
                ),
                normalized AS (
                    SELECT
                        "user_id",
                        COALESCE(
                            NULLIF(lower(regexp_replace(split_part(COALESCE("email", ''), '@', 1), '[^A-Za-z]', '', 'g')), ''),
                            'user'
                        ) AS base
                    FROM "users"
                )
                UPDATE "users" AS u
                SET "name" = left(n.base, greatest(1, 99 - length(s.suffix))) || 'u' || s.suffix
                FROM normalized AS n
                JOIN suffixes AS s ON s."user_id" = n."user_id"
                WHERE u."user_id" = n."user_id";
                """);

            migrationBuilder.CreateIndex(
                name: "users_name_key",
                table: "users",
                column: "name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users",
                sql: "name ~ '^[A-Za-z]+$'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_name_english_letters",
                table: "users");

            migrationBuilder.DropIndex(
                name: "users_name_key",
                table: "users");

            migrationBuilder.Sql("""
                UPDATE "users"
                SET "name" = COALESCE(NULLIF("display_name", ''), "name");
                """);

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "users");
        }
    }
}
