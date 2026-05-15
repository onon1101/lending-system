using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowerDetailSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_id_fkey'
    ) THEN
        ALTER TABLE "orders" DROP CONSTRAINT "orders_borrower_id_fkey";
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS "borrower_details" (
    "borrower_detail_id" serial PRIMARY KEY,
    "user_id" integer NULL,
    "borrower_name" character varying(100) NOT NULL,
    "link" text NOT NULL DEFAULT '',
    "created_by" character varying(100) NOT NULL DEFAULT '',
    "created_at" date NOT NULL,
    "updated_by" character varying(100) NOT NULL DEFAULT '',
    "updated_at" date NOT NULL
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'borrower_details'
          AND constraint_name = 'borrower_details_user_id_fkey'
    ) THEN
        ALTER TABLE "borrower_details"
        ADD CONSTRAINT "borrower_details_user_id_fkey"
        FOREIGN KEY ("user_id") REFERENCES "users" ("user_id");
    END IF;
END $$;

INSERT INTO "borrower_details" ("user_id", "borrower_name", "created_at", "updated_at")
SELECT
    CASE WHEN u."user_id" IS NULL THEN NULL ELSE source."borrower_id" END,
    source."borrower_name",
    source."created_at",
    CURRENT_DATE
FROM (
    SELECT
        o."borrower_id",
        LEFT(COALESCE(NULLIF(o."borrower_name", ''), MAX(NULLIF(u."display_name", '')), MAX(NULLIF(u."name", '')), 'Unknown borrower'), 100) AS "borrower_name",
        COALESCE(MIN(o."start_time"::date), CURRENT_DATE) AS "created_at"
    FROM "orders" AS o
    LEFT JOIN "users" AS u ON u."user_id" = o."borrower_id"
    GROUP BY o."borrower_id", o."borrower_name"
) AS source
LEFT JOIN "users" AS u ON u."user_id" = source."borrower_id"
WHERE NOT EXISTS (
    SELECT 1
    FROM "borrower_details" AS existing
    WHERE existing."user_id" IS NOT DISTINCT FROM CASE WHEN u."user_id" IS NULL THEN NULL ELSE source."borrower_id" END
      AND existing."borrower_name" = source."borrower_name"
);

ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "actual_return_date" date;
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "borrower_detail_id" integer;
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "end_date" date;
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "item_id" integer;
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "start_date" date;

WITH first_details AS (
    SELECT DISTINCT ON (od."order_id")
        od."order_id",
        od."item_id",
        od."actual_return_time",
        od."detail_status"
    FROM "order_details" AS od
    ORDER BY od."order_id", od."order_detail_id"
)
UPDATE "orders" AS o
SET
    "borrower_detail_id" = bd."borrower_detail_id",
    "item_id" = fd."item_id",
    "start_date" = COALESCE(o."start_time"::date, CURRENT_DATE),
    "end_date" = COALESCE(o."end_time"::date, CURRENT_DATE),
    "actual_return_date" = fd."actual_return_time"::date,
    "status" = COALESCE(NULLIF(fd."detail_status", ''), NULLIF(o."status", ''), 'OnLoan')
FROM first_details AS fd,
     "borrower_details" AS bd
WHERE o."order_id" = fd."order_id"
  AND bd."user_id" IS NOT DISTINCT FROM (
      CASE
          WHEN EXISTS (SELECT 1 FROM "users" AS u WHERE u."user_id" = o."borrower_id")
          THEN o."borrower_id"
          ELSE NULL
      END
  )
  AND bd."borrower_name" = LEFT(COALESCE(NULLIF(o."borrower_name", ''), (
      SELECT COALESCE(NULLIF(u."display_name", ''), NULLIF(u."name", ''))
      FROM "users" AS u
      WHERE u."user_id" = o."borrower_id"
  ), 'Unknown borrower'), 100);

INSERT INTO "orders" ("borrower_detail_id", "item_id", "start_date", "end_date", "actual_return_date", "status")
SELECT
    bd."borrower_detail_id",
    od."item_id",
    COALESCE(o."start_time"::date, CURRENT_DATE),
    COALESCE(o."end_time"::date, CURRENT_DATE),
    od."actual_return_time"::date,
    COALESCE(NULLIF(od."detail_status", ''), NULLIF(o."status", ''), 'OnLoan')
FROM (
    SELECT
        od.*,
        ROW_NUMBER() OVER (PARTITION BY od."order_id" ORDER BY od."order_detail_id") AS rn
    FROM "order_details" AS od
) AS od
JOIN "orders" AS o ON o."order_id" = od."order_id"
JOIN "borrower_details" AS bd
  ON bd."user_id" IS NOT DISTINCT FROM (
      CASE
          WHEN EXISTS (SELECT 1 FROM "users" AS u WHERE u."user_id" = o."borrower_id")
          THEN o."borrower_id"
          ELSE NULL
      END
  )
 AND bd."borrower_name" = LEFT(COALESCE(NULLIF(o."borrower_name", ''), (
      SELECT COALESCE(NULLIF(u."display_name", ''), NULLIF(u."name", ''))
      FROM "users" AS u
      WHERE u."user_id" = o."borrower_id"
  ), 'Unknown borrower'), 100)
WHERE od.rn > 1;

DELETE FROM "orders"
WHERE "borrower_detail_id" IS NULL
   OR "item_id" IS NULL
   OR "start_date" IS NULL
   OR "end_date" IS NULL;

ALTER TABLE "orders" ALTER COLUMN "borrower_detail_id" SET NOT NULL;
ALTER TABLE "orders" ALTER COLUMN "item_id" SET NOT NULL;
ALTER TABLE "orders" ALTER COLUMN "start_date" SET NOT NULL;
ALTER TABLE "orders" ALTER COLUMN "end_date" SET NOT NULL;

DROP TABLE IF EXISTS "order_details";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "borrower_id";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "borrower_name";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "start_time";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "end_time";
DROP INDEX IF EXISTS "IX_orders_borrower_id";

CREATE INDEX IF NOT EXISTS "IX_borrower_details_user_id" ON "borrower_details" ("user_id");
CREATE INDEX IF NOT EXISTS "IX_orders_borrower_detail_id" ON "orders" ("borrower_detail_id");
CREATE INDEX IF NOT EXISTS "IX_orders_item_id" ON "orders" ("item_id");

SELECT setval(
    pg_get_serial_sequence('"orders"', 'order_id'),
    COALESCE((SELECT MAX("order_id") FROM "orders"), 1),
    true
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_detail_id_fkey'
    ) THEN
        ALTER TABLE "orders"
        ADD CONSTRAINT "orders_borrower_detail_id_fkey"
        FOREIGN KEY ("borrower_detail_id") REFERENCES "borrower_details" ("borrower_detail_id");
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_item_id_fkey'
    ) THEN
        ALTER TABLE "orders"
        ADD CONSTRAINT "orders_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
        ON DELETE RESTRICT;
    END IF;
END $$;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_detail_id_fkey'
    ) THEN
        ALTER TABLE "orders" DROP CONSTRAINT "orders_borrower_detail_id_fkey";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_item_id_fkey'
    ) THEN
        ALTER TABLE "orders" DROP CONSTRAINT "orders_item_id_fkey";
    END IF;
END $$;

ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "borrower_id" integer;
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "borrower_name" character varying(100) NOT NULL DEFAULT '';
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "start_time" timestamp with time zone NOT NULL DEFAULT '-infinity';
ALTER TABLE "orders" ADD COLUMN IF NOT EXISTS "end_time" timestamp with time zone NOT NULL DEFAULT '-infinity';

UPDATE "orders" AS o
SET
    "borrower_id" = bd."user_id",
    "borrower_name" = bd."borrower_name",
    "start_time" = o."start_date"::timestamp with time zone,
    "end_time" = o."end_date"::timestamp with time zone
FROM "borrower_details" AS bd
WHERE bd."borrower_detail_id" = o."borrower_detail_id";

CREATE TABLE IF NOT EXISTS "order_details" (
    "order_detail_id" serial PRIMARY KEY,
    "item_id" integer NOT NULL,
    "order_id" integer NOT NULL,
    "actual_return_time" timestamp with time zone NULL,
    "detail_status" character varying(50) NOT NULL
);

INSERT INTO "order_details" ("item_id", "order_id", "actual_return_time", "detail_status")
SELECT
    o."item_id",
    o."order_id",
    o."actual_return_date"::timestamp with time zone,
    o."status"
FROM "orders" AS o
WHERE NOT EXISTS (
    SELECT 1
    FROM "order_details" AS od
    WHERE od."order_id" = o."order_id"
      AND od."item_id" = o."item_id"
);

ALTER TABLE "orders" DROP COLUMN IF EXISTS "actual_return_date";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "borrower_detail_id";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "end_date";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "item_id";
ALTER TABLE "orders" DROP COLUMN IF EXISTS "start_date";

DROP INDEX IF EXISTS "IX_orders_borrower_detail_id";
DROP INDEX IF EXISTS "IX_orders_item_id";
CREATE INDEX IF NOT EXISTS "IX_orders_borrower_id" ON "orders" ("borrower_id");
CREATE INDEX IF NOT EXISTS "IX_order_details_item_id" ON "order_details" ("item_id");
CREATE INDEX IF NOT EXISTS "IX_order_details_order_id" ON "order_details" ("order_id");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_id_fkey'
    ) THEN
        ALTER TABLE "orders"
        ADD CONSTRAINT "orders_borrower_id_fkey"
        FOREIGN KEY ("borrower_id") REFERENCES "users" ("user_id");
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'order_details'
          AND constraint_name = 'order_details_item_id_fkey'
    ) THEN
        ALTER TABLE "order_details"
        ADD CONSTRAINT "order_details_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "items" ("item_id") ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'order_details'
          AND constraint_name = 'order_details_order_id_fkey'
    ) THEN
        ALTER TABLE "order_details"
        ADD CONSTRAINT "order_details_order_id_fkey"
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE;
    END IF;
END $$;

DROP TABLE IF EXISTS "borrower_details";
""");
        }
    }
}
