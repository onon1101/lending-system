using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SplitMediaTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS "item_media" (
    "media_id" serial PRIMARY KEY,
    "item_id" integer NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text NULL,
    "description" text NULL,
    "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "lending_media" (
    "media_id" serial PRIMARY KEY,
    "order_id" integer NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text NULL,
    "description" text NULL,
    "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO "item_media" ("media_id", "item_id", "type", "url", "link", "description", "created_at")
SELECT "media_id", "object_id", "type", "url", "link", "description", "created_at"
FROM "media"
WHERE "order_id" IS NULL
ON CONFLICT ("media_id") DO NOTHING;

INSERT INTO "lending_media" ("media_id", "order_id", "type", "url", "link", "description", "created_at")
SELECT "media_id", "order_id", "type", "url", "link", "description", "created_at"
FROM "media"
WHERE "order_id" IS NOT NULL
ON CONFLICT ("media_id") DO NOTHING;

CREATE INDEX IF NOT EXISTS "idx_item_media_item_id" ON "item_media" ("item_id");
CREATE INDEX IF NOT EXISTS "idx_lending_media_order_id" ON "lending_media" ("order_id");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'item_media'
          AND constraint_name = 'fk_item_media_item'
    ) THEN
        ALTER TABLE "item_media"
        ADD CONSTRAINT "fk_item_media_item"
        FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
        ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'lending_media'
          AND constraint_name = 'fk_lending_media_order'
    ) THEN
        ALTER TABLE "lending_media"
        ADD CONSTRAINT "fk_lending_media_order"
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id")
        ON DELETE CASCADE;
    END IF;
END $$;

SELECT setval(
    pg_get_serial_sequence('"item_media"', 'media_id'),
    COALESCE((SELECT MAX("media_id") FROM "item_media"), 1),
    true
);

SELECT setval(
    pg_get_serial_sequence('"lending_media"', 'media_id'),
    COALESCE((SELECT MAX("media_id") FROM "lending_media"), 1),
    true
);

DROP TABLE IF EXISTS "media";
""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS "media" (
    "media_id" serial PRIMARY KEY,
    "order_id" integer NULL,
    "object_id" integer NOT NULL,
    "type" character varying(20) NOT NULL,
    "url" text NOT NULL,
    "link" text NULL,
    "description" text NULL,
    "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO "media" ("media_id", "order_id", "object_id", "type", "url", "link", "description", "created_at")
SELECT "media_id", NULL, "item_id", "type", "url", "link", "description", "created_at"
FROM "item_media"
ON CONFLICT ("media_id") DO NOTHING;

INSERT INTO "media" ("media_id", "order_id", "object_id", "type", "url", "link", "description", "created_at")
SELECT lm."media_id", lm."order_id", o."item_id", lm."type", lm."url", lm."link", lm."description", lm."created_at"
FROM "lending_media" lm
JOIN "orders" o ON o."order_id" = lm."order_id"
ON CONFLICT ("media_id") DO NOTHING;

CREATE INDEX IF NOT EXISTS "idx_media_order_id" ON "media" ("order_id");
CREATE INDEX IF NOT EXISTS "IX_media_object_id" ON "media" ("object_id");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'media'
          AND constraint_name = 'fk_media_item'
    ) THEN
        ALTER TABLE "media"
        ADD CONSTRAINT "fk_media_item"
        FOREIGN KEY ("object_id") REFERENCES "items" ("item_id");
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'media'
          AND constraint_name = 'fk_media_order'
    ) THEN
        ALTER TABLE "media"
        ADD CONSTRAINT "fk_media_order"
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id")
        ON DELETE CASCADE;
    END IF;
END $$;

SELECT setval(
    pg_get_serial_sequence('"media"', 'media_id'),
    COALESCE((SELECT MAX("media_id") FROM "media"), 1),
    true
);

DROP TABLE IF EXISTS "lending_media";
DROP TABLE IF EXISTS "item_media";
""");
    }
}
