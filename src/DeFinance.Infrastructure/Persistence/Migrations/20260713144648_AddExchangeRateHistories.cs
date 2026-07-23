using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRateHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRateHistories_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_CurrencyId_Date",
                table: "ExchangeRateHistories",
                columns: new[] { "CurrencyId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRateHistories");
        }
    }
}
