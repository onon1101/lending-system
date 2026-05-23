using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" ALTER COLUMN "auth_provider" SET DEFAULT 'LOCAL';
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "users" ALTER COLUMN "auth_provider" SET DEFAULT 'local';
""");
        }
    }
}
