using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""orders""
ADD COLUMN IF NOT EXISTS ""borrower_id"" integer;

ALTER TABLE ""orders""
ADD COLUMN IF NOT EXISTS ""borrower_name"" character varying(100);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'orders'
          AND column_name = 'user_id'
    ) THEN
        EXECUTE 'UPDATE ""orders"" SET ""borrower_id"" = ""user_id"" WHERE ""borrower_id"" IS NULL';
    END IF;
END $$;

UPDATE ""orders"" AS o
SET ""borrower_name"" = LEFT(COALESCE(NULLIF(u.""display_name"", ''), u.""name"", ''), 100)
FROM ""users"" AS u
WHERE o.""borrower_id"" = u.""user_id""
  AND (o.""borrower_name"" IS NULL OR o.""borrower_name"" = '');

UPDATE ""orders""
SET ""borrower_name"" = ''
WHERE ""borrower_name"" IS NULL;

ALTER TABLE ""orders""
ALTER COLUMN ""borrower_name"" SET NOT NULL;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_user_id_fkey'
    ) THEN
        ALTER TABLE ""orders"" DROP CONSTRAINT ""orders_user_id_fkey"";
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'orders'
          AND column_name = 'user_id'
    ) THEN
        ALTER TABLE ""orders"" DROP COLUMN ""user_id"";
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_id_fkey'
    ) THEN
        ALTER TABLE ""orders""
        ADD CONSTRAINT ""orders_borrower_id_fkey""
        FOREIGN KEY (""borrower_id"") REFERENCES ""users"" (""user_id"");
    END IF;
END $$;

DROP INDEX IF EXISTS ""IX_orders_user_id"";

CREATE INDEX IF NOT EXISTS ""IX_orders_borrower_id""
ON ""orders"" (""borrower_id"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM ""orders""
        WHERE ""borrower_id"" IS NULL
    ) THEN
        RAISE EXCEPTION 'Cannot roll back UpdateOrderField because external borrower rows have no user_id equivalent.';
    END IF;
END $$;

ALTER TABLE ""orders""
ADD COLUMN IF NOT EXISTS ""user_id"" integer;

UPDATE ""orders""
SET ""user_id"" = ""borrower_id""
WHERE ""user_id"" IS NULL;

ALTER TABLE ""orders""
ALTER COLUMN ""user_id"" SET NOT NULL;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_borrower_id_fkey'
    ) THEN
        ALTER TABLE ""orders"" DROP CONSTRAINT ""orders_borrower_id_fkey"";
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'orders'
          AND column_name = 'borrower_id'
    ) THEN
        ALTER TABLE ""orders"" DROP COLUMN ""borrower_id"";
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'orders'
          AND column_name = 'borrower_name'
    ) THEN
        ALTER TABLE ""orders"" DROP COLUMN ""borrower_name"";
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = current_schema()
          AND table_name = 'orders'
          AND constraint_name = 'orders_user_id_fkey'
    ) THEN
        ALTER TABLE ""orders""
        ADD CONSTRAINT ""orders_user_id_fkey""
        FOREIGN KEY (""user_id"") REFERENCES ""users"" (""user_id"");
    END IF;
END $$;

DROP INDEX IF EXISTS ""IX_orders_borrower_id"";

CREATE INDEX IF NOT EXISTS ""IX_orders_user_id""
ON ""orders"" (""user_id"");
");
        }
    }
}
