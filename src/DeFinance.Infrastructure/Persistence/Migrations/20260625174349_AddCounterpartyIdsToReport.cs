using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterpartyIdsToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "counterparty_ids",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("UPDATE \"Reports\" SET counterparty_ids = '[]' WHERE counterparty_ids = '' OR counterparty_ids IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "counterparty_ids",
                table: "Reports");
        }
    }
}
