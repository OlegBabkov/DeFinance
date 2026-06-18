using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: assign SortOrder per-user ordered by Name
            migrationBuilder.Sql(@"
                UPDATE ""Accounts"" a
                SET ""SortOrder"" = ranked.rn
                FROM (
                    SELECT ""Id"", ROW_NUMBER() OVER (PARTITION BY ""UserId"" ORDER BY ""Name"") - 1 AS rn
                    FROM ""Accounts""
                ) ranked
                WHERE a.""Id"" = ranked.""Id"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SortOrder", table: "Accounts");
        }
    }
}
