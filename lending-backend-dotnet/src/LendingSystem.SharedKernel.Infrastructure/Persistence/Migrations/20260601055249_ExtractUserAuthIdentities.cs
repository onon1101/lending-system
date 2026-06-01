using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtractUserAuthIdentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "status" character varying(50) NOT NULL DEFAULT 'ACTIVE';

UPDATE "users"
SET "status" = CASE WHEN COALESCE("is_deleted", false) THEN 'DELETED' ELSE 'ACTIVE' END;

CREATE TABLE IF NOT EXISTS "user_auth_identities" (
    "id" bigint NOT NULL,
    "user_id" bigint NOT NULL,
    "type" character varying(50) NOT NULL,
    "identifier" character varying(255) NOT NULL,
    "metadata_json" jsonb NOT NULL,
    "created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "user_auth_identities_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "user_auth_identities_user_id_fkey"
        FOREIGN KEY ("user_id")
        REFERENCES "users" ("user_id")
        ON DELETE CASCADE
);

INSERT INTO "user_auth_identities" (
    "id",
    "user_id",
    "type",
    "identifier",
    "metadata_json",
    "created_at",
    "updated_at")
SELECT
    "user_id",
    "user_id",
    UPPER(COALESCE("auth_provider", 'LOCAL')),
    CASE
        WHEN UPPER(COALESCE("auth_provider", 'LOCAL')) = 'LOCAL' THEN "email"
        ELSE "provider_user_id"
    END,
    jsonb_strip_nulls(jsonb_build_object(
        'email', "email",
        'passwordHash', "password_hash"
    )),
    COALESCE("created_at", CURRENT_TIMESTAMP),
    COALESCE("updated_at", CURRENT_TIMESTAMP)
FROM "users"
WHERE (
    UPPER(COALESCE("auth_provider", 'LOCAL')) = 'LOCAL'
    AND "email" IS NOT NULL
)
OR (
    UPPER(COALESCE("auth_provider", 'LOCAL')) <> 'LOCAL'
    AND "provider_user_id" IS NOT NULL
)
ON CONFLICT DO NOTHING;

CREATE INDEX IF NOT EXISTS "idx_user_auth_identities_user_id"
    ON "user_auth_identities" ("user_id");

CREATE UNIQUE INDEX IF NOT EXISTS "user_auth_identities_type_identifier_key"
    ON "user_auth_identities" ("type", "identifier");

DROP INDEX IF EXISTS "users_email_key";
DROP INDEX IF EXISTS "users_auth_provider_provider_user_id_key";
ALTER TABLE "users" DROP COLUMN IF EXISTS "email";
ALTER TABLE "users" DROP COLUMN IF EXISTS "password_hash";
ALTER TABLE "users" DROP COLUMN IF EXISTS "auth_provider";
ALTER TABLE "users" DROP COLUMN IF EXISTS "provider_user_id";
ALTER TABLE "users" DROP COLUMN IF EXISTS "is_deleted";
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "email" character varying(100) NULL;
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "password_hash" character varying(255) NULL;
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "auth_provider" character varying(50) NOT NULL DEFAULT 'LOCAL';
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "provider_user_id" character varying(255) NULL;
ALTER TABLE "users" ADD COLUMN IF NOT EXISTS "is_deleted" boolean NOT NULL DEFAULT false;

UPDATE "users" u
SET
    "email" = COALESCE(a."metadata_json" ->> 'email', CASE WHEN a."type" = 'LOCAL' THEN a."identifier" ELSE NULL END),
    "password_hash" = a."metadata_json" ->> 'passwordHash',
    "auth_provider" = a."type",
    "provider_user_id" = CASE WHEN a."type" = 'LOCAL' THEN NULL ELSE a."identifier" END,
    "is_deleted" = u."status" = 'DELETED'
FROM LATERAL (
    SELECT *
    FROM "user_auth_identities" ai
    WHERE ai."user_id" = u."user_id"
    ORDER BY CASE WHEN ai."type" = 'LOCAL' THEN 0 ELSE 1 END, ai."id"
    LIMIT 1
) a;

CREATE UNIQUE INDEX IF NOT EXISTS "users_email_key"
    ON "users" ("email");
CREATE UNIQUE INDEX IF NOT EXISTS "users_auth_provider_provider_user_id_key"
    ON "users" ("auth_provider", "provider_user_id");

DROP TABLE IF EXISTS "user_auth_identities";
ALTER TABLE "users" DROP COLUMN IF EXISTS "status";
""");
        }
    }
}
