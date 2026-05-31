using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetEntryLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetEntryLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetEntryLines_BudgetEntries_BudgetEntryId",
                        column: x => x.BudgetEntryId,
                        principalTable: "BudgetEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEntryLines_BudgetEntryId",
                table: "BudgetEntryLines",
                column: "BudgetEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetEntryLines");
        }
    }
}
