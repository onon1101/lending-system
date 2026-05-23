using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LendingDbContext))]
    [Migration("20260512130000_AddUserDisplayNameAndUsername")]
    public partial class AddUserDisplayNameAndUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "users"
                ADD COLUMN IF NOT EXISTS "display_name" character varying(100) NOT NULL DEFAULT '';

                UPDATE "users"
                SET "display_name" = COALESCE(NULLIF("name", ''), NULLIF("email", ''), 'User');

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

                CREATE UNIQUE INDEX IF NOT EXISTS "users_name_key" ON "users" ("name");

                ALTER TABLE "users"
                ADD CONSTRAINT "ck_users_name_english_letters" CHECK ("name" ~ '^[A-Za-z]+$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "users" DROP CONSTRAINT IF EXISTS "ck_users_name_english_letters";
                DROP INDEX IF EXISTS "users_name_key";

                UPDATE "users"
                SET "name" = COALESCE(NULLIF("display_name", ''), "name");

                ALTER TABLE "users" DROP COLUMN IF EXISTS "display_name";
                """);
        }
    }
}
